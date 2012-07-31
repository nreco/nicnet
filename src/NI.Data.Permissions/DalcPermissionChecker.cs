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
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Text.RegularExpressions;

using NI.Common;
using NI.Data;
using NI.Security.Permissions;

namespace NI.Data.Permissions
{
	/// <summary>
	/// DALC-specific permission checker
	/// </summary>
	public class DalcPermissionChecker : PermissionChecker
	{
		IDalc _OriginalDalc;
		IDalcConditionComposer _DalcConditionComposer;
		RecordExistsMethodName _RecordExistsMethod = RecordExistsMethodName.RecordsCount;


		public enum RecordExistsMethodName {
			RecordsCount = 0,
			LoadRecord = 1
		}

		/// <summary>
		/// Get or set record exists check type
		/// </summary>
		public RecordExistsMethodName RecordExistsMethod {
			get { return _RecordExistsMethod; }
			set { _RecordExistsMethod = value; }
		}

		/// <summary>
		/// Get or set original (not permission proxy!) DALC component
		/// </summary>
		public IDalc OriginalDalc {
			get { return _OriginalDalc; }
			set { _OriginalDalc = value; }
		}
		
		/// <summary>
		/// Get or set condition composer component
		/// </summary>
		public IDalcConditionComposer DalcConditionComposer {
			get { return _DalcConditionComposer; }
			set { _DalcConditionComposer = value; }
		}
		

		public override bool Check(Permission permission) {
			if (permission is DalcPermission) {
				
				// load record fields if they wasn't provided
				DalcPermission dalcPermission = (DalcPermission)permission;
				if (dalcPermission.Object.Fields==null) {
					dalcPermission.Object.Fields = new Hashtable();
					Query q = new Query(dalcPermission.Object.SourceName);
					QueryGroupNode groupAnd = new QueryGroupNode(GroupType.And);
					foreach (DictionaryEntry entry in dalcPermission.Object.UidFields)
						groupAnd.Nodes.Add( (QField)entry.Key.ToString() == new QConst(entry.Value) );					
					q.Root = groupAnd;
					OriginalDalc.LoadRecord(dalcPermission.Object.Fields, q);
				}

				if (!CheckDalcDenyPermissions( (DalcPermission)permission ))
					return false;
			}
				
			return base.Check(permission);
		}

		public override bool[] Check(Permission[] permissions) {
			Hashtable massGroup = new Hashtable(permissions.Length);
			bool[] results = new bool[permissions.Length];

			for (int i=0; i<permissions.Length; i++) 
				if (permissions[i] is DalcPermission) {
					DalcPermission dalcPermission = (DalcPermission)permissions[i];
					if (dalcPermission.Operation!=DalcOperation.Create) {
						MassOperationParams opParams = new MassOperationParams(dalcPermission.Subject, dalcPermission.Operation, dalcPermission.Object.SourceName);
						if (!massGroup.ContainsKey(opParams))
							massGroup[ opParams ] = new ArrayList();
						IList groupList = (IList)massGroup[opParams];
						groupList.Add(dalcPermission);
					} else
						results[i] = Check(permissions[i]);
				}
			
			// check group permissions
			foreach (DictionaryEntry massGroupEntry in massGroup) {
				MassOperationParams opParams = (MassOperationParams)massGroupEntry.Key;
				IList groupList = (IList)massGroupEntry.Value;
				
				QueryGroupNode queryGroupAnd = new QueryGroupNode(GroupType.And);
				IQueryNode permissionCondition = DalcConditionComposer.Compose(
					opParams.Subject, opParams.Operation, opParams.SourceName);
				if (permissionCondition!=null)
					queryGroupAnd.Nodes.Add( permissionCondition );
				// compose UIDs
				QueryGroupNode uidsGroupOr = new QueryGroupNode(GroupType.Or);
				queryGroupAnd.Nodes.Add(uidsGroupOr);
				ArrayList fieldsToExtract = new ArrayList();
				foreach (DalcPermission dalcPermission in groupList) {
					QueryGroupNode uidGroupAnd = new QueryGroupNode(GroupType.And);
					foreach (DictionaryEntry entry in dalcPermission.Object.UidFields) {
						uidGroupAnd.Nodes.Add( (QField)entry.Key.ToString() == new QConst(entry.Value) );
						if (!fieldsToExtract.Contains(entry.Key.ToString())) {
							fieldsToExtract.Add( entry.Key.ToString() );
						}
					}
					uidsGroupOr.Nodes.Add(uidGroupAnd);
				}

				DataSet ds = new DataSet();
				Query query = new Query(opParams.SourceName, queryGroupAnd);
				query.Fields = (string[])fieldsToExtract.ToArray(typeof(string));
				OriginalDalc.Load(ds, query);

				// process results
				foreach (DalcPermission dalcPermission in groupList) {
					int resultIdx = Array.IndexOf( permissions, dalcPermission);
					// find key in loaded dataset
					if (FindDataRow(ds.Tables[opParams.SourceName], dalcPermission.Object.UidFields )==null)
						// permission denieded
						results[resultIdx] = false;
					else {
						// check also generic permissions
						results[resultIdx] = base.Check(dalcPermission);
					}
				}
			}

			return results;
		}

		protected DataRow FindDataRow(DataTable tbl, IDictionary matchFields) {
			for (int i=0; i<tbl.Rows.Count; i++) {
				bool equals = true;
				foreach (DictionaryEntry fld in matchFields) {
					string fldName = fld.Key.ToString();
					// TBD: more adequate comparision
					if (tbl.Rows[i][fldName].ToString()!=fld.Value.ToString()) {
						equals = false;
						break;
					}
				}
				if (equals)
					return tbl.Rows[i];
			}
			return null;
		}
		
		protected virtual bool CheckDalcDenyPermissions(DalcPermission permission) {
			bool isFieldPermission = permission.Object is DalcRecordFieldInfo;
			if (permission.Operation!=DalcOperation.Create && !isFieldPermission) {
				// check 'deny' constraints defined by DalcConditionComposer
				QueryGroupNode groupAnd = new QueryGroupNode(GroupType.And);
				IQueryNode permissionCondition = DalcConditionComposer.Compose(
					permission.Subject, permission.Operation, permission.Object.SourceName);
				if (permissionCondition!=null)
					groupAnd.Nodes.Add( permissionCondition );
				
				foreach (DictionaryEntry entry in permission.Object.UidFields)
					groupAnd.Nodes.Add( (QField)entry.Key.ToString() == new QConst(entry.Value) );
				
				if (!RecordExists(permission.Object.SourceName, groupAnd))
					// deny by conditions
					return false;
			
			}
			return true;
		}

		protected bool RecordExists(string sourcename, IQueryNode condition) {
			if (RecordExistsMethod==RecordExistsMethodName.RecordsCount)
				return OriginalDalc.RecordsCount(sourcename, condition)>0;
			ListDictionary rInfo = new ListDictionary();
			Query q = new Query(sourcename, condition);
			q.Fields = new string[] {"1"};
			return OriginalDalc.LoadRecord(rInfo, q);
		}

		internal class MassOperationParams {
			public object Subject;
			public DalcOperation Operation;
			public string SourceName;
			int hashCode;

			internal MassOperationParams(object subject, DalcOperation op, string sourcename) {
				Subject = subject;
				Operation = op;
				SourceName = sourcename;
				hashCode = String.Format("{0}#{1}#{2}", subject, op, sourcename).GetHashCode();
			}

			public override int GetHashCode() {
				return hashCode;
			}

			public override bool Equals(object obj) {
				if (obj is MassOperationParams) {
					MassOperationParams opParams = (MassOperationParams)obj;
					return 
						Subject==opParams.Subject &&
						Operation==opParams.Operation &&
						SourceName==opParams.SourceName;
				}

				return base.Equals(obj);
			}
		}


	}
}
