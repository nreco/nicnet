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
using System.Text.RegularExpressions;

using NI.Common;
using NI.Common.Providers;

namespace NI.Security.Permissions
{
	/// <summary>
	/// Generic permission ACL entry based on regexes applied over string representation
	/// of subject, operation and object.
	/// </summary>
	public class RegexPermissionAclEntry : IPermissionAclEntry
	{
		IBooleanProvider _BooleanProvider;
		string _MatchOperation;
		string _MatchObject;
		string _MatchSubject;
		
		string _SubjectContextKey = "subject";
		string _ObjectContextKey = "object";
		string _OperationContextKey = "operation";
		
		public IBooleanProvider BooleanProvider {
			get { return _BooleanProvider; }
			set { _BooleanProvider = value; }
		}
		
		/// <summary>
		/// Get or set operation regex that should be matched by this entry
		/// </summary>
		public string MatchSubject {
			get { return _MatchSubject; }
			set { _MatchSubject = value; }
		}		
		
		/// <summary>
		/// Get or set operation regex that should be matched by this entry
		/// </summary>
		public string MatchOperation {
			get { return _MatchOperation; }
			set { _MatchOperation = value; }
		}
		
		/// <summary>
		/// Get or set object regex that should be matched by this entry
		/// </summary>
		public string MatchObject {
			get { return _MatchObject; }
			set { _MatchObject = value; }
		}
		
		/// <summary>
		/// Get or set subject context key
		/// </summary>
		public string SubjectContextKey {
			get { return _SubjectContextKey; }
			set { _SubjectContextKey = value; }
		}

		/// <summary>
		/// Get or set operation context key
		/// </summary>
		public string OperationContextKey {
			get { return _OperationContextKey; }
			set { _OperationContextKey = value; }
		}
		
		/// <summary>
		/// Get or set object context key
		/// </summary>
		public string ObjectContextKey {
			get { return _ObjectContextKey; }
			set { _ObjectContextKey = value; }
		}		
		
	
		public RegexPermissionAclEntry()
		{
		}
		
		public virtual bool IsMatch(Permission permission) {

			if (!Regex.IsMatch( Convert.ToString(permission.Operation), MatchOperation))
				return false;
			
			if (!Regex.IsMatch( Convert.ToString(permission.Subject), MatchSubject))
				return false;

			if (!Regex.IsMatch( Convert.ToString(permission.Object), MatchObject))
				return false;
			
			return true;
		}
		
		public virtual bool Check(Permission permission) {
			return BooleanProvider.GetBoolean( BuildContext(permission) );
		}
		
		protected virtual IDictionary BuildContext(Permission permission) {
			ListDictionary context = new ListDictionary();
			
			context[SubjectContextKey] = Convert.ToString( permission.Subject );
			context[ObjectContextKey] = Convert.ToString( permission.Object );
			context[OperationContextKey] = Convert.ToString( permission.Operation );

			return context;
		}		
		
		
		
	}
}
