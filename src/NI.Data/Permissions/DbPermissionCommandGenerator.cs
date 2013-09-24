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
using System.Threading;
using System.Security.Principal;
using System.Collections;
using System.Text.RegularExpressions;
using System.Linq;
using System.Data;


namespace NI.Data.Permissions
{
	/// <summary>
	/// </summary>
	public class DbPermissionCommandGenerator : NI.Data.DbCommandGenerator
	{

		public IDalcQueryRule[] Rules { get; set; }
		
		public DbPermissionCommandGenerator(IDbDalcFactory dbFactory, IDbDalcView[] views) : base(dbFactory,views) {

		}

		protected virtual PermissionContext CreatePermissionContext(string sourceName, DalcOperation op) {
			return new PermissionContext(sourceName, op);
		}

		protected QueryNode ApplyRuleConditions(QueryNode node, string sourceName, DalcOperation operation) {
			var resNode = new QueryGroupNode(GroupType.And);
			resNode.Nodes.Add(node);
			var context = CreatePermissionContext(sourceName, operation);
			for (int i = 0; i < Rules.Length; i++) {
				var r = Rules[i];
				if (r.IsMatch(context)) {
					var extraCondition = r.ComposeCondition(context);
					if (extraCondition != null)
						resNode.Nodes.Add(extraCondition);
				}
			}
			return resNode.Nodes.Count > 1 ? resNode : node;
		}

		protected virtual Query PrepareSelectQuery(Query q) {
			var withExtraConditions = ApplyRuleConditions(q.Condition, q.SourceName.Name, DalcOperation.Select);
			if (withExtraConditions != q.Condition) {
				var qClone = new Query(q);
				qClone.Condition = withExtraConditions;
				return qClone;
			}
			return q;
		}

		protected override QueryNode ComposeDeleteCondition(DataTable table, IDbSqlBuilder dbSqlBuilder) {
			var baseDelete = base.ComposeDeleteCondition(table, dbSqlBuilder);
			return ApplyRuleConditions(baseDelete, table.TableName, DalcOperation.Delete);
		}

		protected override QueryNode ComposeDeleteCondition(Query query) {
			var baseDelete = base.ComposeDeleteCondition(query);
			return ApplyRuleConditions(baseDelete, query.SourceName.Name, DalcOperation.Delete);
		}

		protected override QueryNode ComposeUpdateCondition(DataTable table, IDbSqlBuilder dbSqlBuilder) {
			var baseUpdate = base.ComposeUpdateCondition(table, dbSqlBuilder);
			return ApplyRuleConditions(baseUpdate, table.TableName, DalcOperation.Update);
		}

		protected override QueryNode ComposeUpdateCondition(Query query) {
			var baseUpdate = base.ComposeUpdateCondition(query);
			return ApplyRuleConditions( baseUpdate, query.SourceName.Name, DalcOperation.Update );
		}
		
		/*protected override IDictionary BuildSqlCommandContext(IDbCommand cmd, IDbDalcView dataView, Query query) {
			IDictionary context = base.BuildSqlCommandContext (cmd, dataView, query);
			// if origin does not specified, skip permission-conditions generation
			if (dataView.OriginSourceNames==null)
				return context;
			
			// if origin more than one, or alias specified - generate '<table-alias>-whereExpression' tokens
			Match m = SourceNameOriginsRegex.Match( String.Join(",",  dataView.OriginSourceNames.Select(s=>s.ToString() ) ) );
			for (int i=0; i<m.Groups["sourceName"].Captures.Count; i++) {
				string sourceName = m.Groups["sourceName"].Captures[i].Value;
				string alias = m.Groups["alias"].Captures[i].Value;
				string whereExpressionPrefix = alias.Length>0 ? alias : sourceName;

				QueryNode permissionCondition = DalcConditionComposer.Compose(ContextUser, DalcOperation.Retrieve, sourceName);
				IDbSqlBuilder dbSqlBuilder = DbFactory.CreateSqlBuilder( cmd );
				if (alias.Length > 0) {
					//var origFormatter = dbSqlBuilder.QueryFieldValueFormatter;
					//dbSqlBuilder.QueryFieldValueFormatter = (qFld) => {
					///var resFld = origFormatter != null ?
						//	origFormatter(qFld) : qFld.Name;

						//string[] parts = resFld.Split('.');
						//if (parts.Length > 1 && parts[0] == sourceName)
							//return alias + "." + parts[1];
						//return resFld;
					//};
				}

				context[whereExpressionPrefix+"-permissionWhereExpression"] = 
					IsolateWhereExpression( dbSqlBuilder.BuildExpression(permissionCondition) );
			}
			
			return context;
		}

		
		protected override string BuildWhereExpression(IDbSqlBuilder dbSqlBuilder, IDbDalcView dataView, Query query) {
			// if origin does not specified, skip permission-conditions generation
			if (dataView.OriginSourceNames==null)
				return base.BuildWhereExpression(dbSqlBuilder, dataView, query);

			// if origin more than one, or alias specified - skip permission-conditions generation
			Match m = SourceNameOriginsRegex.Match(String.Join(",", dataView.OriginSourceNames.Select(s => s.ToString())));
			if (!m.Success || m.Groups["sourceName"].Captures.Count>1)
				return base.BuildWhereExpression(dbSqlBuilder, dataView, query);

			// add one more field-formatter to the formatters chain
			// if source name alias specified
			if (m.Groups["alias"].Captures[0].Length > 0) {
				/var origFormatter = dbSqlBuilder.QueryFieldValueFormatter;
				dbSqlBuilder.QueryFieldValueFormatter = (qFld) => {
					var resFld = origFormatter != null ?
						origFormatter(qFld) : qFld.Name;

					string[] parts = resFld.Split('.');
					if (parts.Length > 1 && parts[0] == m.Groups["sourceName"].Captures[0].Value)
						return m.Groups["alias"].Captures[0].Value + "." + parts[1];
					return resFld;
				};/

			}
			
			// compose permission condition
			QueryNode permissionCondition = DalcConditionComposer.Compose(ContextUser, DalcOperation.Retrieve, m.Groups["sourceName"].Captures[0].Value);
			QueryNode condition = query.Condition;
			QueryGroupNode groupAnd = new QueryGroupNode(GroupType.And);
			if (condition != null)
				groupAnd.Nodes.Add(condition);
			if (permissionCondition != null)
				groupAnd.Nodes.Add( permissionCondition );
			
			return dbSqlBuilder.BuildExpression( groupAnd );
		}*/
		
		
		
		
	}
}
