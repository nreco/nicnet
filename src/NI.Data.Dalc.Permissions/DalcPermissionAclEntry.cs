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
using System.Text.RegularExpressions;
using System.Collections;

using NI.Common.Providers;
using NI.Common;
using NI.Security.Permissions;

namespace NI.Data.Dalc.Permissions
{
	/// <summary>
	/// Field-oriented DalcPermission ACL entry.
	/// </summary>
	public class DalcPermissionAclEntry : IPermissionAclEntry
	{
		string _MatchSourceName = ".*"; // means any
		string _SourceNameContextKey = "__source_name";
		string _SubjectContextKey = "__subject";
		IBooleanProvider _BooleanProvider;
		DalcOperation _MatchOperation;
		
		/// <summary>
		/// Get or set DALC operation should be matched by this entry
		/// </summary>
		[Dependency]
		public DalcOperation MatchOperation {
			get { return _MatchOperation; }
			set { _MatchOperation = value; }
		}
				
		/// <summary>
		/// Get or set external boolean provider used in permission 'Check' operation
		/// </summary>
		[Dependency]
		public IBooleanProvider BooleanProvider {
			get { return _BooleanProvider; }
			set { _BooleanProvider = value; }
		}
		
		/// <summary>
		/// Get or set context key for source name
		/// </summary>
		[Dependency(Required=false)]
		public string SourceNameContextKey {
			get { return _SourceNameContextKey; }
			set { _SourceNameContextKey = value; }
		}
		
		/// <summary>
		/// Get or set context key for subject
		/// </summary>
		[Dependency(Required=false)]
		public string SubjectContextKey {
			get { return _SubjectContextKey; }
			set { _SubjectContextKey = value; }
		}		
		

		/// <summary>
		/// Get or set source name regex to match
		/// </summary>
		[Dependency]
		public string MatchSourceName {
			get { return _MatchSourceName; }
			set { _MatchSourceName = value; }
		}
		
		public DalcPermissionAclEntry()
		{
		}

		public virtual bool IsMatch(Permission permission) {
			if (!(permission is DalcPermission))
				return false;
			DalcPermission dalcPermission = (DalcPermission)permission;
			
			if (MatchOperation!=dalcPermission.Operation)
				return false;

			if (!Regex.IsMatch(dalcPermission.Object.SourceName, MatchSourceName))
				return false;

			return true;
		}

		public virtual bool Check(Permission permission) {
			if (!(permission is DalcPermission))
				throw new ArgumentException("DalcPermissionAclEntry can check only permissions with type 'DalcPermission'");
			
			IDictionary context = BuildContext( (DalcPermission)permission);
			return BooleanProvider.GetBoolean(context);
		}
		
		protected virtual IDictionary BuildContext(DalcPermission permission) {
			Hashtable context = new Hashtable(permission.Object.UidFields);
			context[SourceNameContextKey] = permission.Object.SourceName;
			context[SubjectContextKey] = permission.Subject;
			// extend context with record fields if they are provided
			if (permission.Object.Fields!=null)
				foreach (DictionaryEntry fldEntry in permission.Object.Fields)
					if (!context.ContainsKey(fldEntry.Key))
						context[fldEntry.Key] = fldEntry.Value;

			return context;
		}


	}
}
