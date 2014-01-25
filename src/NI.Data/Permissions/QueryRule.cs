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

		public string TableName { get; set; }

		public QTable[] ViewNames { get; set; }

		public DalcOperation Operation { get; set; }

		protected QueryNode RuleCondition;

		public QueryRule(string tableName, DalcOperation op, QueryNode ruleCondition) {
			TableName = tableName;
			Operation = op;
			RuleCondition = ruleCondition;
		}

		public QueryRule(string tableName, DalcOperation op, string relexCondition) {
			TableName = tableName;
			Operation = op;
			RuleCondition = (new RelExParser()).ParseCondition(relexCondition);
		}

		protected QTable FindViewName(string tableName) {
			if (ViewNames!=null)
				for (int i = 0; i < ViewNames.Length; i++) {
					if (ViewNames[i].Name == tableName)
						return ViewNames[i];
				}
			return null;
		}

		public virtual QueryNode ComposeCondition(PermissionContext context) {
			if ((Operation & context.Operation) != context.Operation)
				return null;
			var matchedTableName = TableName == context.TableName ? (QTable)TableName : FindViewName(context.TableName);
			if (matchedTableName==null)
				return null;

			var ruleCondition = RuleCondition;
			if (!String.IsNullOrEmpty( matchedTableName.Alias )) {
				Func<IQueryValue,IQueryValue> alignFieldPrefix = null;
				alignFieldPrefix = (n) => {
					if (n is QField) {
						var fld = (QField)n;
						if (fld.Prefix == TableName)
							return new QField(matchedTableName.Alias, fld.Name, fld.Expression);
					}
					if (n is Query) {
						var q = (Query)n;
						var qCopy = new Query(q);
						qCopy.Condition = DataHelper.MapQValue(qCopy.Condition, alignFieldPrefix);
					}
					return n;
				};
				ruleCondition = DataHelper.MapQValue(ruleCondition, alignFieldPrefix);
			}

			DataHelper.SetQueryVariables(ruleCondition, (v) => {
				SetVariable(v,context);
			});

			return ruleCondition;
		}


		protected virtual void SetVariable(QVar var, PermissionContext context) {
			var p = context.GetType().GetProperty(var.Name);
			var.Unset();
			if (p!=null) {
				var.Set( p.GetValue(context, null) );
			}
		}

	}
		
}
