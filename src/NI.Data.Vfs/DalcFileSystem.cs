#region License
/*
 * NIC.NET library
 * Copyright 2004-2012 NewtonIdeas
 * Distributed under the LGPL licence
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.Collections;
using System.IO;
using System.Data;
using NI.Vfs;
using NI.Data;

namespace NI.Data.Vfs {
    /// <summary>
    /// Dalc based IFileSystem implementation
    /// </summary>
    public class DalcFileSystem: IFileSystem {

        private IDalc _Dalc;              
        private DalcFileSystemHelper _FileSystemHelper;
        private string _TableName;
        private string _ContentTableName;
        private string _KeyFieldName;
        private string _ParentFieldName;
        private char _DirectorySeparatorChar = '/';
        private IFileObject _Root;

        public char DirectorySeparatorChar {
            get { return _DirectorySeparatorChar; }
            set { _DirectorySeparatorChar = value; }
        }

        public DalcFileSystemHelper FileSystemHelper {
            get { return _FileSystemHelper; }
            set { _FileSystemHelper = value; }
        }

        public IDalc Dalc {
            get { return _Dalc; }
            set { _Dalc = value; }
        }

        /// <summary>
        /// File storage source name
        /// </summary>
        public string TableName {
            get { return _TableName; }
            set { _TableName = value; }
        }

        /// <summary>
        /// Content storage source name
        /// </summary>
        public string ContentTableName {
            get { return _ContentTableName; }
            set { _ContentTableName = value; }
        }

        /// <summary>
        /// Filename field name
        /// </summary>
        public string KeyFieldName {
            get { return _KeyFieldName; }
            set { _KeyFieldName = value; }
        }

        /// <summary>
        /// Parent folder field name
        /// </summary>
        public string ParentFieldName {
            get { return _ParentFieldName; }
            set { _ParentFieldName = value; }
        }

        /// <summary>
        /// Root file object
        /// </summary>
        public IFileObject Root {
            get {
                if (_Root == null) {
                    _Root = ResolveFile(Convert.ToString(DirectorySeparatorChar));
                    if (_Root.Type == FileType.Imaginary)  _Root.CreateFolder();                   
                }
                return _Root;
            }
        }        

       
        public IFileObject ResolveFile(string name) {
            if (name.Length > 0)  name = FormatPath(name);                                      
            Query query = new Query(TableName,
                                    new QueryConditionNode((QField)KeyFieldName, 
                                                             Conditions.Equal, (QConst)name));
            IDictionary fileData = Dalc.LoadRecord(query);
			if (fileData!=null) {
                return (IFileObject)FileSystemHelper.SetFileProperties(new DalcFileObject(this), fileData);
            }
            else  {
              IFileObject fileObj =  new DalcFileObject(name, FileType.Imaginary, this);
              if (name.Length == 0)  fileObj.CreateFolder();              
              return fileObj;
            }  
        }


       /// <summary>
       /// Method gets children of folder by folder name
       /// </summary>
       /// <param name="name">Folder name</param>
       /// <returns>Array of children instances</returns>
        public DalcFileObject[] GetChildren(string name) {
            if (name.Length > 0) name = FormatPath(name);    
            Query q = new Query(TableName, new QueryConditionNode((QField)ParentFieldName,Conditions.Equal,(QConst)name));
            DataSet ds = new DataSet();
			Dalc.Load(q,ds);
            ArrayList fileList = new ArrayList();
            foreach (DataRow row in ds.Tables[TableName].Rows) 
                fileList.Add(FileSystemHelper.SetFileProperties(new DalcFileObject(this), 
                                                                    new DataRowDictionary(row)));           
            return (DalcFileObject[])fileList.ToArray(typeof(DalcFileObject));
        }

        /// <summary>
        /// Method fills DalcFileContent instance by data from storage they exists
        /// </summary>
        /// <param name="fileContent">DalcFileContent instance</param>
        /// <param name="name">Filename</param>
        /// <returns></returns>
        public DalcFileContent GetContent(DalcFileContent fileContent,string name) {
            if (name.Length > 0) name = FormatPath(name);    
            Query q = new Query(ContentTableName,
                                new QueryConditionNode((QField)KeyFieldName,
                                                        Conditions.Equal,(QConst)name));
            IDictionary contentData = Dalc.LoadRecord(q);
			if (contentData!=null)
				fileContent = FileSystemHelper.SetContentProperties(fileContent, contentData);
            return fileContent;
        }

        /// <summary>
        /// Method saves file
        /// </summary>
        /// <param name="fileObject">File object</param>
        public void SaveFile(DalcFileObject fileObject) {                
            SaveInternal(FileSystemHelper.SetFileDictionaryValues(new Hashtable(), fileObject),
                            fileObject, TableName, KeyFieldName);         
        }

        /// <summary>
        /// Method saves file content
        /// </summary>
        /// <param name="fileContent">Instance of file content</param>
        public void SaveContent(DalcFileContent fileContent) {
            IDictionary data = FileSystemHelper.SetContentDictionaryValues(new Hashtable(), fileContent);
            SaveInternal(FileSystemHelper.SetContentDictionaryValues(new Hashtable(), fileContent),
                           fileContent.File, ContentTableName,KeyFieldName);               
        }        

        /// <summary>
        /// Method delete file by name
        /// </summary>
        /// <param name="name">Filename</param>
        public void DeleteFile(string name) {
            if (name.Length > 0) name = FormatPath(name);    
            DeleteInternal(name, ContentTableName, KeyFieldName);
            DeleteInternal(name, TableName, KeyFieldName);
        }

        #region Internal methods
        private void SaveInternal(IDictionary data, IFileObject fileObject, 
                                    string tableName,string keyFieldName) {          
           QueryConditionNode conditions =  new QueryConditionNode((QField)keyFieldName,
                                            Conditions.Equal, (QConst)fileObject.Name);
           Hashtable recordData = new Hashtable();
           int result = Dalc.RecordsCount(new Query(tableName, conditions));
           if (result > 0) {
               if ( data.Contains(keyFieldName) ){
                   data.Remove(keyFieldName); // fixed DB bug on update
               }
			   Dalc.Update(new Query(tableName, conditions), data);
		   } else
			   Dalc.Insert(tableName, data);           
        }

        private void DeleteInternal(string name, string tableName, string keyFieldName) {
            if (name.Length > 0) name = FormatPath(name);    
            Query q = new Query(tableName);
            q.Condition = new QueryConditionNode((QField)keyFieldName, Conditions.Equal, (QConst)name);
            Dalc.Delete(q);
        }

        #endregion

        #region Service methods
        /// <summary>
        /// Method format path by filesystem separator
        /// </summary>
        /// <param name="path">Path string</param>
        /// <returns>Formated path string</returns>
        public string FormatPath(string path)  {
            if (path == String.Empty) return path;
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            // normalize file name
            path = Path.Combine(Path.GetDirectoryName(path) != null ? Path.GetDirectoryName(path): String.Empty, 
                                Path.GetFileName(path));
            path = path.Replace(Path.DirectorySeparatorChar, DirectorySeparatorChar);
            return path;
        }

        /// <summary>
        /// Method unformat path (format by default Path.DirectorySeparatorChar)
        /// </summary>
        /// <param name="path">Formatted path string</param>
        /// <returns>Unformated path string</returns>
        public string UnformatPath(string path) {
            if (path == String.Empty) return path;
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            path = path.Replace(DirectorySeparatorChar, Path.DirectorySeparatorChar);
            // normalize file name
            path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path));
            return path;
        }
        #endregion

    }
}
