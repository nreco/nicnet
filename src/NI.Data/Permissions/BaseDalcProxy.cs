#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
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
using System.Data;
using System.Collections;
using System.Security;
using System.Security.Principal;
using System.Threading;

using NI.Common;
using NI.Common.Collections;

namespace NI.Data.Permissions {
	/// <summary>
	/// Permission-checking DALC proxy.
	/// </summary>
	public abstract class BaseDalcProxy : IDalc {
		
		IDalcConditionComposer _DalcConditionComposer;
		IDalcPermissionChecker _PermissionChecker;
		bool _Enabled = true;
		
		public bool Enabled {
			get { return _Enabled; }
			set { _Enabled = value; }
		}
		
		/// <summary>
		/// Get or set underlying DALC component
		/// </summary>
		protected abstract IDalc Dalc { get; }
		
		/// <summary>
		/// Get or set DALC condition composer component
		/// </summary>
		public IDalcConditionComposer DalcConditionComposer {
			get { return _DalcConditionComposer; }
			set { _DalcConditionComposer = value; }
		}

		/// <summary>
		/// Get or set DALC permission checker component
		/// </summary>
		public IDalcPermissionChecker PermissionChecker {
			get { return _PermissionChecker; }
			set { _PermissionChecker = value; }
		}

		protected IPrincipal ContextUser {
			get {
				return Thread.CurrentPrincipal;
			}
		}		
				
		public BaseDalcProxy() {
		}
		
		protected IQuery AddPermissionCondition(DalcOperation operation, IQuery query) {
			QSourceName qSourceName = (QSourceName)query.SourceName;
			IQueryNode permissionCondition = DalcConditionComposer.Compose(ContextUser, operation, qSourceName.Name);
			if (permissionCondition!=null) {
				Query modifiedQuery = new Query(query);
				QueryGroupNode newRoot = new QueryGroupNode(GroupType.And);
				newRoot.Nodes.Add( modifiedQuery.Root );
				newRoot.Nodes.Add( PreparePermissionCondition( permissionCondition, qSourceName) );
				modifiedQuery.Root = newRoot;
				return modifiedQuery;
			}
			return query;
		}

		protected IQueryNode PreparePermissionCondition(IQueryNode permissionCondition, QSourceName qSourceName) {
			// check if alias is used
			if (!String.IsNullOrEmpty(qSourceName.Alias))
				FixConditionFieldNames(permissionCondition, qSourceName);
			return permissionCondition;
		}
		protected void FixConditionFieldNames(IQueryNode node, QSourceName qSourceName) {
			if (node is QueryConditionNode) {
				QueryConditionNode condNode = (QueryConditionNode)node;
				condNode.LValue = FixConditionFieldNames(condNode.LValue, qSourceName);
			}
			if (node is IQueryGroupNode) {
				IQueryGroupNode grpNode = (IQueryGroupNode)node;
				foreach (IQueryNode grpChildNode in grpNode.Nodes)
					FixConditionFieldNames(grpChildNode, qSourceName);
			}
		}
		protected IQueryValue FixConditionFieldNames(IQueryValue qValue, QSourceName qSourceName) {
			if (qValue is IQueryFieldValue) {
				IQueryFieldValue qFld = (IQueryFieldValue)qValue;
				int dotIdx = qFld.Name.IndexOf('.');
				if (dotIdx >= 0) {
					string prefix = qFld.Name.Substring(0, dotIdx);
					string suffix = qFld.Name.Substring(dotIdx);
					if (prefix == qSourceName.Name)
						return new QField(qSourceName.Alias + suffix);
				}
			}
			if (qValue is IQuery) {
				IQuery q = (IQuery)qValue;
				FixConditionFieldNames(q.Root, qSourceName);
			}
			return qValue;
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
						ContextUser, ConvertToDalcOperation(r.RowState), recordInfo);
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
									ContextUser, DalcOperation.Update, recordFieldInfo );
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
					ContextUser, DalcOperation.Create, new DalcRecordInfo(sourceName, data, data) );
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
				IQueryNode permissionCondition = DalcConditionComposer.Compose( ContextUser, DalcOperation.Retrieve, sourceName );
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
