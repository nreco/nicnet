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
using System.Collections.Generic;
using System.ComponentModel;

namespace NI.Data
{
	/// <summary>
	/// Sql expression builder (default implementation).
	/// </summary>
	public class SqlBuilder : ISqlBuilder
	{
		
		public SqlBuilder()
		{
		}
		
		
		public virtual string BuildExpression(QueryNode node) {
			if (node==null) return null;

			if (node is QueryRawSqlNode)
				return ((QueryRawSqlNode)node).SqlText;
			if (node is QueryGroupNode)
				return BuildGroup( (QueryGroupNode)node );
			if (node is QueryConditionNode)
				return BuildCondition( (QueryConditionNode)node );
			if (node is QueryNegationNode)
				return BuildNegation( (QueryNegationNode)node );
			
			throw new ArgumentException("Cannot build node with such type", node.GetType().ToString() );
		}
		
		protected virtual string BuildGroup(QueryGroupNode node) {
			// do not render empty group
			if (node.Nodes==null) return null;
			
			// if group contains only one node ignore group node...
			var subNodes = new List<string>();
			foreach (QueryNode childNode in node.Nodes) {
				string childNodeExpression = BuildExpression( childNode );
				if (childNodeExpression!=null)
					subNodes.Add( "("+childNodeExpression+")" ); 
			}
			
			// if only one child node just ignore group node
			if (subNodes.Count==1) {
				string childNodeExpression = subNodes[0];
				return childNodeExpression.Substring(1, childNodeExpression.Length-2);
			}
			
			// do not render empty group
			if (subNodes.Count==0) return null;
			
			return String.Join(
				" "+node.GroupType.ToString()+" ",
				subNodes.ToArray() );
		}
		
		protected virtual string BuildNegation(QueryNegationNode node) {
			if (node.Nodes.Count==0) return null;
			string expression = BuildExpression(node.Nodes[0]);
			if (expression==null) return null;
			return String.Format("NOT({0})", expression);
		}

		protected virtual string BuildCondition(QueryConditionNode node) {
			Conditions condition = node.Condition & (
				Conditions.Equal | Conditions.GreaterThan |
				Conditions.In | Conditions.LessThan |
				Conditions.Like | Conditions.Null
				);
			string lvalue = BuildValue( node.LValue);
			string rvalue = BuildValue( node.RValue);
			string res = null;
			
			switch (condition) {
				case Conditions.GreaterThan:
					res = String.Format("{0}>{1}", lvalue, rvalue );
					break;
				case Conditions.LessThan:
					res = String.Format("{0}<{1}", lvalue, rvalue );
					break;
				case (Conditions.LessThan | Conditions.Equal):
					res = String.Format("{0}<={1}", lvalue, rvalue );
					break;
				case (Conditions.GreaterThan | Conditions.Equal):
					res = String.Format("{0}>={1}", lvalue, rvalue );
					break;
				case Conditions.Equal:
					res = String.Format("{0}{2}{1}", lvalue, rvalue, (node.Condition & Conditions.Not)!=0 ? "<>" : "=" );
					break;
				case Conditions.Like:
					res = String.Format("{0} LIKE {1}", lvalue, rvalue );
					break;
				case Conditions.In:
					res = String.Format("{0} IN ({1})", lvalue, rvalue );
					break;
				case Conditions.Null:
					res = String.Format("{0} IS {1} NULL", lvalue, (node.Condition & Conditions.Not)!=0 ? "NOT" : "" );
					break;
				default:
					throw new ArgumentException("Invalid conditions set", condition.ToString() );
			}
				
			if ( (node.Condition & Conditions.Not)!=0
				&& (condition & Conditions.Null)==0
				&& (condition & Conditions.Equal)==0 )
				return String.Format("NOT ({0})", res);
			return res;
		}
		
		public virtual string BuildValue(IQueryValue value) {
			if (value==null) return null;
			
			if (value is QField)
				return BuildValue( (QField)value );
			
			if (value is QConst)
				return BuildValue( (QConst)value );
			
			if (value is QRawSql)
				return ((QRawSql)value).SqlText;

			throw new ArgumentException("Invalid query value", value.GetType().ToString() );
		}

		public virtual string BuildSort(QSort value) {
			var sortFld = BuildValue( (IQueryValue) value.Field);
			return String.Format("{0} {1}", sortFld, value.SortDirection == ListSortDirection.Ascending ? QSort.Asc : QSort.Desc);
		}

		protected virtual string BuildValue(QConst value) {
			object constValue = value.Value;
				
			// special processing for arrays
			if (constValue is IList)
				return BuildValue( (IList)constValue );
			if (constValue is string)
				return BuildValue( (string)constValue );
									
			return Convert.ToString(constValue, System.Globalization.CultureInfo.InvariantCulture);
		}
		
		protected virtual string BuildValue(IList list) {
			string[] paramNames = new string[list.Count];
			for (int i=0; i<list.Count; i++)
				paramNames[i] = BuildValue( new QConst(list[i]) );
			return String.Join(",", paramNames);
		}
		
		protected virtual string BuildValue(string str) {
			return "'"+str.Replace(@"'", @"\'")+"'";
		}
		
		protected virtual string BuildValue(QField fieldValue) {
			if (!String.IsNullOrEmpty(fieldValue.Expression))
				return fieldValue.Expression;
			return String.IsNullOrEmpty(fieldValue.Prefix) ? fieldValue.Name : fieldValue.Prefix + "." + fieldValue.Name;
		}

		
		
		
	}
}
