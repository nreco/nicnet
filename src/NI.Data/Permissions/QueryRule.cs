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
using System.Collections;
using System.Text;
using NI.Data.RelationalExpressions;

namespace NI.Data.Permissions {
	
	/// <summary>
	/// Generic query rule
	/// </summary>
	public class QueryRule {

		public string SourceName { get; set; }

		public QSource[] ViewNames { get; set; }

		public DalcOperation Operation { get; set; }

		protected QueryNode RuleCondition;

		public QueryRule(string sourceName, DalcOperation op, QueryNode ruleCondition) {
			SourceName = sourceName;
			Operation = op;
			RuleCondition = ruleCondition;
		}

		public QueryRule(string sourceName, DalcOperation op, string relexCondition) {
			SourceName = sourceName;
			Operation = op;
			RuleCondition = (new RelExParser()).ParseCondition(relexCondition);
		}

		protected QSource FindViewName(string sourceName) {
			if (ViewNames!=null)
				for (int i = 0; i < ViewNames.Length; i++) {
					if (ViewNames[i].Name == sourceName)
						return ViewNames[i];
				}
			return null;
		}



		public virtual QueryNode ComposeCondition(PermissionContext context) {
			if ((Operation & context.Operation) != context.Operation)
				return null;
			var matchedSourceName = SourceName == context.SourceName ? (QSource)SourceName : FindViewName(context.SourceName);
			if (matchedSourceName==null)
				return null;

			return RuleCondition;
		}

	}
		
}
