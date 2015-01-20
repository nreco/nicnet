#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
 * Copyright 2008-2014 Vitalii Fedorchenko (changes and v.2)
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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Data;


namespace NI.Data.Permissions
{
	/// <summary>
	/// Extends DbCommandGenerator with permission rules
	/// </summary>
	public class DbPermissionCommandGenerator : NI.Data.DbCommandGenerator
	{
		
		/// <summary>
		/// Get or set list of permission rules
		/// </summary>
		public IQueryRule[] Rules { get; set; }

		/// <summary>
		/// Get or set external permission context delegate
		/// </summary>
		public Func<string, DalcOperation, PermissionContext> GetPermissionContext { get; set; }

		public DbPermissionCommandGenerator(IDbProviderFactory dbFactory, IDbDalcView[] views) : base(dbFactory,views) {
			Rules = new IQueryRule[0];
		}

		public DbPermissionCommandGenerator(IDbProviderFactory dbFactory, IDbDalcView[] views, IQueryRule[] rules)
			: base(dbFactory, views) {
			Rules = rules;
		}

		protected virtual PermissionContext CreatePermissionContext(string tableName, DalcOperation op) {
			if (GetPermissionContext!=null)
				return GetPermissionContext(tableName, op);
			return new PermissionContext(tableName, op);
		}

		protected QueryNode ApplyRuleConditions(QueryNode node, string tableName, DalcOperation operation) {
			var resNode = new QueryGroupNode(QueryGroupNodeType.And);
			resNode.Nodes.Add(node);
			var context = CreatePermissionContext(tableName, operation);
			for (int i = 0; i < Rules.Length; i++) {
				var r = Rules[i];
				var extraCondition = r.ComposeCondition(context);
				if (extraCondition != null)
					resNode.Nodes.Add(extraCondition);
			}
			return resNode.Nodes.Count > 1 ? resNode : node;
		}

		protected override Query PrepareSelectQuery(Query q) {
			var withExtraConditions = ApplyRuleConditions(q.Condition, q.Table.Name, DalcOperation.Select);
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
			return ApplyRuleConditions(baseDelete, query.Table.Name, DalcOperation.Delete);
		}

		protected override QueryNode ComposeUpdateCondition(DataTable table, IDbSqlBuilder dbSqlBuilder) {
			var baseUpdate = base.ComposeUpdateCondition(table, dbSqlBuilder);
			return ApplyRuleConditions(baseUpdate, table.TableName, DalcOperation.Update);
		}

		protected override QueryNode ComposeUpdateCondition(Query query) {
			var baseUpdate = base.ComposeUpdateCondition(query);
			return ApplyRuleConditions( baseUpdate, query.Table.Name, DalcOperation.Update);
		}

		
	}
}
