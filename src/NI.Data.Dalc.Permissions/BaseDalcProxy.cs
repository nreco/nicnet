#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2008 NewtonIdeas
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
using System.Data;
using System.Collections;
using System.Security;
using System.Threading;

using NI.Common;
using NI.Common.Collections;
using NI.Security.Permissions;

namespace NI.Data.Dalc.Permissions {
	/// <summary>
	/// Permission-checking DALC proxy.
	/// </summary>
	public abstract class BaseDalcProxy : IDalc {
		
		IDalcConditionComposer _DalcConditionComposer;
		IPermissionChecker _PermissionChecker;
		bool _Enabled = true;
		
		public bool Enabled {
			get { return _Enabled; }
			set { _Enabled = value; }
		}
		
		/// <summary>
		/// Get or set underlying DALC component
		/// </summary>
		[Dependency]
		protected abstract IDalc Dalc { get; }
		
		/// <summary>
		/// Get or set DALC condition composer component
		/// </summary>
		[Dependency]
		public IDalcConditionComposer DalcConditionComposer {
			get { return _DalcConditionComposer; }
			set { _DalcConditionComposer = value; }
		}

		/// <summary>
		/// Get or set DALC permission checker component
		/// </summary>
		[Dependency]
		public IPermissionChecker PermissionChecker {
			get { return _PermissionChecker; }
			set { _PermissionChecker = value; }
		}

		
				
		public BaseDalcProxy() {
		}
		
		protected Query CloneQuery(IQuery query) {
			Query q = new Query(query.SourceName);
			q.Fields = query.Fields;
			q.Root = query.Root;
			q.RecordCount = query.RecordCount;
			q.StartRecord = query.StartRecord;
			q.Sort = query.Sort;
			return q;
		}
		
		protected IQuery AddPermissionCondition(DalcOperation operation, IQuery query) {
			IQueryNode permissionCondition = DalcConditionComposer.Compose(ContextSubject, operation, query.SourceName);
			if (permissionCondition!=null) {
				Query modifiedQuery = CloneQuery(query);
				QueryGroupNode newRoot = new QueryGroupNode(GroupType.And);
				newRoot.Nodes.Add( modifiedQuery.Root );
				newRoot.Nodes.Add( permissionCondition );
				modifiedQuery.Root = newRoot;
				return modifiedQuery;
			}
			return query;
		}
		
		protected object ContextSubject {
			get {
				return Thread.CurrentPrincipal.Identity.Name;
			}
		}

		public void Load(DataSet ds, IQuery query) {
			if (Enabled) {
				IQuery modifiedQuery = AddPermissionCondition(DalcOperation.Retrieve, query);
				Dalc.Load(ds, modifiedQuery);
			} else {
				Dalc.Load(ds, query);
			}
		}

		protected DalcOperation ConvertToDalcOperation(DataRowState rowState) {
			switch (rowState) {
				case DataRowState.Added:
					return DalcOperation.Create;
				case DataRowState.Deleted:
					return DalcOperation.Delete;
				default:
					return DalcOperation.Update;
			}
		}
		
		protected string FormatRowIndentification(DataRow row) {
			IList idColumns;
			if (row.RowState!=DataRowState.Added)
				idColumns = (IList)row.Table.PrimaryKey;
			else {
				DataColumn[] columns = new DataColumn[row.Table.Columns.Count];
				row.Table.Columns.CopyTo(columns, 0);
				idColumns = columns;
			}
			string[] fieldValues = new string[idColumns.Count];
			for (int i=0; i<idColumns.Count; i++) {
				DataColumn c = (DataColumn)idColumns[i];
				fieldValues[i] = c.ColumnName+"="+
					Convert.ToString( row[c,
					row.RowState==DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current ] );
			}
			return String.Join(",", fieldValues);
		}
		
		protected IDictionary ComposeRecordIdInfo(DataRow r) {
			if (r.RowState==DataRowState.Added)
				return new DataRowDictionary(r);
			Hashtable idInfo = new Hashtable();
			foreach (DataColumn pkCol in r.Table.PrimaryKey)
				idInfo[pkCol.ColumnName] = r[pkCol, DataRowVersion.Original];
			return idInfo;
		}

		public void Update(DataSet ds, string sourceName) {
			if (Enabled)
				foreach (DataRow r in ds.Tables[sourceName].Rows) {
					// check for modifications
					if (r.RowState==DataRowState.Unchanged)
						continue;
					
					DalcRecordInfo recordInfo = new DalcRecordInfo(sourceName, ComposeRecordIdInfo(r), new DataRowDictionary(r) );
					DalcPermission recordPermission = new DalcPermission(
						ContextSubject, ConvertToDalcOperation(r.RowState), recordInfo);
					if (!PermissionChecker.Check(recordPermission))
						throw new SecurityException(
							String.Format("{0} operation is not allowed for {1}({2})",
							recordPermission.Operation.ToString(),
							sourceName,
							FormatRowIndentification(r) ) );
					
					// check field permissions for IComparable data columns
					if (r.HasVersion(DataRowVersion.Original) && ConvertToDalcOperation(r.RowState)==DalcOperation.Update )
						for (int i=0; i<r.Table.Columns.Count; i++) {
							object oldValue = r[r.Table.Columns[i], DataRowVersion.Original];
							object newValue = r[r.Table.Columns[i], DataRowVersion.Current];
							bool isFieldChanged = oldValue.GetType()!=newValue.GetType();

							if (!isFieldChanged)
								if(oldValue is IComparable)
									if ( ((IComparable)oldValue).CompareTo(newValue)!=0)
										isFieldChanged = true;
							
							if (isFieldChanged) {
								DalcRecordFieldInfo recordFieldInfo = new DalcRecordFieldInfo(
									recordInfo.SourceName, r.Table.Columns[i].ColumnName, recordInfo.UidFields, recordInfo.Fields );
								DalcPermission recordFieldPermission = new DalcPermission(
									ContextSubject, DalcOperation.Update, recordFieldInfo );
								if (!PermissionChecker.Check(recordFieldPermission))
									throw new SecurityException(
										String.Format("Update operation is not allowed for {0}.{1}({2})",
										recordFieldInfo.SourceName, recordFieldInfo.FieldName,
										FormatRowIndentification(r) ) );
							}
							
							
						}
					
				}
			Dalc.Update(ds, sourceName);
		}

		public int Update(IDictionary data, IQuery query) {
			if (Enabled) {
				IQuery modifiedQuery = AddPermissionCondition(DalcOperation.Update, query);
				return Dalc.Update(data, modifiedQuery);		
			} else {
				return Dalc.Update(data, query);	
			}
		}

		public void Insert(IDictionary data, string sourceName) {
			if (Enabled) {
				DalcPermission recordPermission = new DalcPermission(
					ContextSubject, DalcOperation.Create, new DalcRecordInfo(sourceName, data, data) );
				if (!PermissionChecker.Check(recordPermission))
					throw new SecurityException(
						String.Format("{0} operation is not allowed for {1}",
						recordPermission.Operation.ToString(),
						sourceName) );
			}
			Dalc.Insert(data, sourceName);		
		}

		public int Delete(IQuery query) {
			if (Enabled) {
				IQuery modifiedQuery = AddPermissionCondition(DalcOperation.Delete, query);
				return Dalc.Delete( modifiedQuery );
			} else {
				return Dalc.Delete(query);
			}
		}

		public bool LoadRecord(IDictionary data, IQuery query) {
			if (Enabled) {
				IQuery modifiedQuery = AddPermissionCondition(DalcOperation.Retrieve, query);
				return Dalc.LoadRecord( data, modifiedQuery);
			} else {
				return Dalc.LoadRecord(data, query);
			}
		}

		public int RecordsCount(string sourceName, IQueryNode conditions) {
			if (Enabled) {
				IQueryNode permissionCondition = DalcConditionComposer.Compose( ContextSubject, DalcOperation.Retrieve, sourceName );
				if (permissionCondition!=null) {
					QueryGroupNode groupAnd = new QueryGroupNode(GroupType.And);
					groupAnd.Nodes.Add( conditions );
					groupAnd.Nodes.Add( permissionCondition );
					conditions = groupAnd;
				}
			}
			return Dalc.RecordsCount(sourceName, conditions);
		}

	}
}
