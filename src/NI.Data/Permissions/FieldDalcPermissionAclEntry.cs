#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
 * Copyright 2008-2013 Vitalii Fedorchenko (changes and v.2)
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
using System.Text.RegularExpressions;
using System.Collections;


namespace NI.Data.Permissions
{
	/// <summary>
	/// Field-oriented DalcPermission ACL entry.
	/// </summary>
	/*public class FieldDalcPermissionAclEntry : DalcPermissionAclEntry
	{
		string _MatchFieldName = ".*"; // means any
		string _FieldNameContextKey = "__field_name";
		
		/// <summary>
		/// Get or set context key for field name
		/// </summary>
		public string FieldNameContextKey {
			get { return _FieldNameContextKey; }
			set { _FieldNameContextKey = value; }
		}
		
		/// <summary>
		/// Get or set field name to match
		/// </summary>
		public string MatchFieldName {
			get { return _MatchFieldName; }
			set { _MatchFieldName = value; }			
		}
		
	
		public FieldDalcPermissionAclEntry()
		{
		}

		public override bool IsMatch(DalcPermission permission) {
			if (!(permission.Object is DalcRecordFieldInfo))
				return false;
			
			DalcRecordFieldInfo recordFieldInfo = (DalcRecordFieldInfo)permission.Object;
			if (!Regex.IsMatch(recordFieldInfo.FieldName, MatchFieldName))
				return false;
			
			return base.IsMatch(permission);
		}


		public override bool Check(DalcPermission permission) {
			if (!(permission.Object is DalcRecordFieldInfo))
				throw new ArgumentException("Expected DalcRecordFieldInfo instance", "permission");
			
			return base.Check(permission);
		}
		
		protected override IDictionary BuildContext(DalcPermission permission) {
			IDictionary context = base.BuildContext (permission);
			context[FieldNameContextKey] = ((DalcRecordFieldInfo)permission.Object).FieldName;
			return context;
		}



	}*/
}
