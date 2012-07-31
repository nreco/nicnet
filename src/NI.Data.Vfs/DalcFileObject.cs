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
using NI.Vfs;
using System.IO;
namespace NI.Data.Vfs {

    /// <summary>
    /// Dalc based IFileObject implementation
    /// </summary>
    public class DalcFileObject: IFileObject {

        private string _Name;
        private string _ParentName;
        private FileType _Type;
        int _CopyBufferLength = 64 * 1024; //64kb
        
        private DalcFileSystem DalcFs;        
        private long InitialContentLength;
        protected DalcFileContent FileContent = null;

        /// <summary>
        /// Get or set buffer length used when copying file
        /// </summary>
        public int CopyBufferLength
        {
            get { return _CopyBufferLength; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException();
                _CopyBufferLength = value;
            }
        }

        public string Name {
            get { return _Name; }
            set { _Name = value; }
        }
        
        public FileType Type {
            get { return _Type; }
            set { _Type = value; }
        }

        /// <summary>
        /// Name of parent folder
        /// </summary>
        public string ParentName {
            get { return _ParentName; }
            set { _ParentName = value; }
        }

        /// <summary>
        /// Instance of parent folder 
        /// </summary>
        public IFileObject Parent {
            get {   
                if (ParentName == null) return null; //root
                return DalcFs.ResolveFile(ParentName); }
        }
        
        public void Close() {          
            DalcFs.SaveFile(this);
            if (FileContent != null) {                         
                FileContent.Close();             
                FileContent = null;                
            }
        }

		
        public virtual void CopyFrom(IFileObject srcFile) {
            if (srcFile.Type == FileType.File)
            {
                using (Stream inputStream = srcFile.GetContent().InputStream)
                {
                    CopyFrom(inputStream);
                }
            }
            if (srcFile.Type == FileType.Folder)
            {
                CopyFrom(srcFile.GetChildren());
            }
        }

        public void CopyFrom(IFileObject[] srcEntries) {
            if (Type == FileType.File) Delete();
            if (Type == FileType.Imaginary) CreateFolder();

            foreach (IFileObject srcFile in srcEntries) {
                string destFileName = Path.Combine(DalcFs.UnformatPath(Name), 
                                          Path.GetFileName(DalcFs.UnformatPath(srcFile.Name)));
                IFileObject destFile = DalcFs.ResolveFile(destFileName);
                destFile.CopyFrom(srcFile);
            }
        }

        public void CopyFrom(Stream inputStream) {
            if (Type != FileType.Imaginary) Delete();
            CreateFile();
            Stream outputStream = GetContent().OutputStream;

            byte[] buf = new byte[CopyBufferLength];
            try
            {
                int bytesRead = 0;
                do
                {
                    bytesRead = inputStream.Read(buf, 0, CopyBufferLength);
                    if (bytesRead > 0)
                        outputStream.Write(buf, 0, bytesRead);
                } while (bytesRead > 0);
            }
            finally
            {
                outputStream.Close();
            }			
        }

        public virtual void CreateFile() {
            if (Type == FileType.Imaginary) {                
                if (ParentName.Length > 0) {
                    IFileObject parentFolder = DalcFs.ResolveFile(ParentName);
                    if (parentFolder.Type != FileType.Folder) parentFolder.CreateFolder();
                }
            }
            _Type = FileType.File;
            Close();
        }

        public virtual void CreateFolder() {
            _Type = FileType.Folder;
            DalcFs.SaveFile(this);
        }

        public void Delete() {
            if (Type == FileType.Folder) {
                foreach (IFileObject dalcFile in GetChildren())                    
                        dalcFile.Delete();                    
            }
             DalcFs.DeleteFile(Name);
            _Type = FileType.Imaginary;
            
        }

        public bool Exists() { 
            return Type != FileType.Imaginary; 
        }

        public virtual IFileObject[] GetChildren() {
            if (Type != FileType.Folder)
                throw new FileSystemException(); // TODO: more structured exception            
            return DalcFs.GetChildren(Name);
        }

        public IFileContent GetContent() {
            if (FileContent == null) {
                FileContent = new DalcFileContent(this);
                if (Type != FileType.Imaginary) {
                    FileContent = DalcFs.GetContent(FileContent, Name);
                    
                }
                if (Type == FileType.File) 
                InitialContentLength = FileContent.OutputStream.Length;
            }
            

            return FileContent;
        }

        public void SaveContent() {
            if (FileContent != null) {
                if (InitialContentLength != FileContent.OutputStream.Length)
                    DalcFs.SaveContent(FileContent);
            }
        }
        
        public IFileObject[] FindFiles(IFileSelector selector) {
            if (Type != FileType.Folder)
                throw new FileSystemException(); // TODO: more structured exception

            ArrayList resultList = new ArrayList();
            IFileObject[] children = GetChildren();
            foreach (IFileObject file in children)
            {
                if (selector.IncludeFile(file)) resultList.Add(file);
                if (file.Type == FileType.Folder)
                    if (selector.TraverseDescendents(file)) {
                        IFileObject[] foundFiles = file.FindFiles(selector);
                        resultList.AddRange(foundFiles);
                    }
            }
            return (IFileObject[])resultList.ToArray(typeof(IFileObject));
        }

        public void MoveTo(IFileObject destFile) {
            // copy-delete
            destFile.CopyFrom(this);
            this.Delete();
        }
      
        public DalcFileObject(string name, FileType type, DalcFileSystem dalcFileSystem) {
            Name = name;
            ParentName = name != String.Empty ? dalcFileSystem.FormatPath(
                Path.GetDirectoryName(dalcFileSystem.UnformatPath(name))) : null;
            DalcFs = dalcFileSystem;
            Type = type;
            
        }

        public DalcFileObject(DalcFileSystem dalcFileSystem) {
            DalcFs = dalcFileSystem;            
        }
		
    }
}
