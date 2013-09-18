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
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Linq;

using NI.Vfs;
using NI.Data;

using NI.Vfs;

namespace NI.Data {
	
	public class FileSystemDalc : IDalc {
		IFileSystem _FileSystem;
		ObjectQueryConditionEvaluator ConditionEvaluator;
		
		public IFileSystem FileSystem {
			get { return _FileSystem; }
			set { _FileSystem = value; }
		}
		
		public FileSystemDalc() {
			ConditionEvaluator = new ObjectQueryConditionEvaluator();
			ConditionEvaluator.QFieldResolver = GetFileObjectFieldValue;
		}

		public DataTable Load(Query query, DataSet ds) {
			if (ds.Tables.Contains(query.SourceName))
				ds.Tables.Remove(query.SourceName);
			DataTable tbl = ds.Tables.Add(query.SourceName);

			IFileObject[] files = Select(query.SourceName, query.Condition);

			if (query.Fields.Length == 1 && query.Fields[0] == "count(*)") {
				tbl.Columns.Add("count", typeof(int));
				var cntRow = tbl.NewRow();
				cntRow["count"] = files.Length;
				tbl.Rows.Add(cntRow);
				return tbl;
			}

			tbl.Columns.Add(CreateColumn("is_file", typeof(bool),false,true) );
			tbl.Columns.Add(CreateColumn("is_folder", typeof(bool), false, false));
			tbl.Columns.Add(CreateColumn("name", typeof(string), false, String.Empty));
			tbl.Columns.Add(CreateColumn("full_name", typeof(string), false, String.Empty));
			tbl.Columns.Add(CreateColumn("folder_name", typeof(string), false, String.Empty));
			tbl.Columns.Add(CreateColumn("ext", typeof(string), false, String.Empty));
			tbl.Columns.Add(CreateColumn("size", typeof(long), false, 0));
			tbl.Columns.Add(CreateColumn("last_modified", typeof(DateTime), true, null));
			tbl.Columns.Add(CreateColumn("shared_file_id", typeof(int), true, null));
			tbl.Columns.Add(CreateColumn("shared_public_id", typeof(string), true, null));

			files = ApplySortAndPaging(query, files);

			for (int i=0; i<files.Length; i++) {
				DataRow r = tbl.NewRow();
				var f = files[i];
				r["is_file"] = GetFileObjectField("is_file",f);
				r["is_folder"] = GetFileObjectField("is_folder",f);
				r["name"] = GetFileObjectField("name",f);
				r["full_name"] = GetFileObjectField("full_name",f);
				r["folder_name"] = GetFileObjectField("folder_name",f);
				r["ext"] = GetFileObjectField("ext",f);
				r["size"] = GetFileObjectField("size",f);
				r["last_modified"] = GetFileObjectField("last_modified",f);

				tbl.Rows.Add(r);
			}
			ds.AcceptChanges();

			return tbl;
		}

		protected IFileObject[] ApplySortAndPaging(Query q, IFileObject[] files) {
			if (q.Sort!=null) {
				if (q.Sort.Length>1)
					throw new Exception("FileSystemDalc doesn't support sorting by multiple fields");
				var qSortFld = new QSort(q.Sort[0]);
				files = files.OrderBy( f => GetFileObjectField( qSortFld.Field, f) ).ToArray();
				if (qSortFld.SortDirection==System.ComponentModel.ListSortDirection.Descending)
					Array.Reverse( files );
			}
			var folders = files.Where( f => f.Type==FileType.Folder);
			var onlyFiles = files.Where( f => f.Type==FileType.File );
			return folders.Union(onlyFiles).Skip(q.StartRecord).Take(q.RecordCount).ToArray();
		}
		
		protected DataColumn CreateColumn(string name, Type type, bool nullable, object defaultValue) {
			DataColumn c = new DataColumn(name, type);
			c.AllowDBNull = nullable;
			c.DefaultValue = defaultValue;
			return c;
		}
		

		public void Update(DataTable tbl) {
			//TODO: implement update for file attributes
			throw new NotSupportedException("FileSystemDalc does not supports update operations.");
		}

		public int Update(Query query, IDictionary<string,IQueryValue> data) {
			if (!data.ContainsKey("name") || !(data["name"] is QConst) )
				return 0;

			var newName = ((QConst)data["name"]).Value as string;
			if (String.IsNullOrEmpty(newName))
				return 0;
			IFileObject[] files = Select(query.SourceName, query.Condition);
			
			foreach (var f in files) {
				var newFileName = Path.Combine( Path.GetDirectoryName( f.Name ), newName );
				f.MoveTo( FileSystem.ResolveFile( newFileName ) );
			}
			return files.Length;
		}

		public void Insert(string sourceName, IDictionary<string,IQueryValue> data) {
			throw new NotSupportedException("FileSystemDalc does not supports insert operations.");
		}

		public int Delete(Query query) {
			IFileObject[] files = Select(query.SourceName, query.Condition);
			foreach (var f in files) {
				f.Delete();
			}
			return files.Length;
		}

		public void ExecuteReader(Query q, Action<IDataReader> handler) {
			var ds = new DataSet();
			var tbl = Load(q, ds);
			var rdr = new DataTableReader(tbl);
			handler(rdr);
		}

		public bool LoadRecord(IDictionary data, Query query) {
			if (query.Fields.Length==1 && query.Fields[0]=="count(*)") {
				data["count(*)"] = RecordsCount( query.SourceName, query.Condition );
				return true;
			}
			
			var res = Select( query.SourceName, query.Condition );
			if (res.Length>0) {
				var firstRow = res[0];
				data["is_file"] = GetFileObjectField("is_file", firstRow);
				data["is_folder"] = GetFileObjectField("is_folder", firstRow);
				data["name"] = GetFileObjectField("name", firstRow);
				data["full_name"] = GetFileObjectField("full_name", firstRow);
				data["folder_name"] = GetFileObjectField("folder_name", firstRow);
				data["ext"] = GetFileObjectField("ext", firstRow);
				data["size"] = GetFileObjectField("size", firstRow);
				data["last_modified"] = GetFileObjectField("last_modified", firstRow);
				return true;
			}
			return false;
		}

		public int RecordsCount(string sourceName, QueryNode condition) {
			return Select(sourceName, condition).Length;
		}
		
		protected IFileObject[] Select(string sourceName, QueryNode condition) {
			if (sourceName=="." || sourceName==Path.AltDirectorySeparatorChar.ToString() || sourceName==Path.DirectorySeparatorChar.ToString())
				sourceName = "";
			IFileObject fileObj = FileSystem.ResolveFile(sourceName);
			if (fileObj==null)
				throw new ArgumentException(String.Format("File {0} does not exist",sourceName));
			QueryFileSelector qFileSelector = new QueryFileSelector(false, ConditionEvaluator, condition);
			IFileObject[] foundFiles = fileObj.Type==FileType.Folder ? fileObj.FindFiles(qFileSelector) : new IFileObject[0];
			return foundFiles;
		}
		
		protected object GetFileObjectField(string name, IFileObject file) {
			if (name=="is_file") return file.Type==FileType.File;
			if (name=="is_folder") return file.Type == FileType.Folder;
			string fName = file.Name.Replace('\\', '/');
			if (name == "name") return Path.GetFileName(fName);
			if (name == "full_name") return fName;
			if (name == "ext") return Path.GetExtension(fName);
			if (name == "folder_name") return Path.GetDirectoryName(fName);
			IFileContent fContent = file.GetContent();
			if (name=="size") return fContent.Size;
			if (name=="last_modified") return fContent.LastModifiedTime;
			throw new ArgumentException("Unknown field name: "+name);
		}

		protected object GetFileObjectFieldValue(ObjectQueryConditionEvaluator.ResolveNodeContext resolveContext) {
			QField fldValue = (QField)resolveContext.Node;
			return GetFileObjectField(fldValue.Name, (IFileObject)resolveContext.Context["file"]);
		}
			
		internal class QueryFileSelector : IFileSelector {
			bool FindAll = false;
			ObjectQueryConditionEvaluator Evaluator;
			QueryNode CondNode;
			IDictionary Context;
			
			internal QueryFileSelector(bool findAll,ObjectQueryConditionEvaluator evaluator,QueryNode condNode) {
				FindAll = findAll;
				Evaluator = evaluator;
				CondNode = condNode;
				Context = new Hashtable();
			}
			
			public bool IncludeFile(IFileObject file) {
				Context["file"] = file;
				return Convert.ToBoolean( Evaluator.Evaluate(Context,CondNode) );
			}

			public bool TraverseDescendents(IFileObject file) {
				return FindAll;
			}
			
		}
		
	}
	
}
