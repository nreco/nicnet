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
using System.Text.RegularExpressions;

using NI.Common;
using NI.Common.Expressions;

namespace NI.Security.Permissions
{
	/// <summary>
	/// Check permission expression resolver.
	/// </summary>
	public class CheckPermissionExprResolver : IExpressionResolver
	{
		static readonly Regex ParamsRegex = new Regex(@"^(?<subject>[^,]+)\s*[,]\s*(?<operation>[^,]+)\s*[,]\s*(?<object>[^,]+)$",
			RegexOptions.IgnoreCase|RegexOptions.Singleline|RegexOptions.Compiled);
	
		IPermissionChecker _PermissionChecker;
		
		/// <summary>
		/// Get or set permission checker component
		/// </summary>
		public IPermissionChecker PermissionChecker {
			get { return _PermissionChecker; }
			set { _PermissionChecker = value; }
		}
	
		public CheckPermissionExprResolver()
		{
		}
		
		protected virtual Permission ComposePermission(string input) {
			Match m = ParamsRegex.Match(input);
			if (!m.Success)
				throw new ApplicationException("Invalid check permission expression: " + input);

			string subjectValue = m.Groups["subject"].Value;
			string operationValue = m.Groups["operation"].Value;
			string objectValue = m.Groups["object"].Value;

			return new Permission(subjectValue, operationValue, objectValue);
		}
		
		public object Evaluate(IDictionary context, string expression) {
			Permission permission = ComposePermission(expression);
			return PermissionChecker.Check( permission );
		}
		
	}
	
}
