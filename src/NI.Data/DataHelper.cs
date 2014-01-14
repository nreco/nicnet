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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace NI.Data {
	
	public static class DataHelper {
		public static DataSet GetDataSetFromXml(string xml) {
			var ds = new DataSet();
			ds.ReadXml(new StringReader(xml));
			return ds;
		}

		public static void EnsureConnectionOpen(IDbConnection connection, Action a) {
			bool closeConn = false;
			if (connection.State != ConnectionState.Open) {
				connection.Open();
				closeConn = true;
			}
			try {
				a();
			} finally {
				if (closeConn)
					connection.Close();
			}
		}

		public static QueryNode MapQValue(QueryNode qNode, Func<IQueryValue,IQueryValue> mapFunc) {
			if (qNode is QueryGroupNode) {
				var group = new QueryGroupNode((QueryGroupNode)qNode);
				for (int i = 0; i < group.Nodes.Count; i++)
					group.Nodes[i] = MapQValue(group.Nodes[i], mapFunc);
				return group;
			}
			if (qNode is QueryConditionNode) {
				var origCndNode = (QueryConditionNode)qNode;
				var cndNode = new QueryConditionNode(origCndNode.Name,
						mapFunc(origCndNode.LValue),
						origCndNode.Condition,
						mapFunc(origCndNode.RValue));
				return cndNode;
			}
			if (qNode is QueryNegationNode) {
				var negNode = new QueryNegationNode((QueryNegationNode)qNode);
				for (int i = 0; i < negNode.Nodes.Count; i++)
					negNode.Nodes[i] = MapQValue(negNode.Nodes[i], mapFunc);
				return negNode;
			}
			return qNode;
		}

		public static void SetQueryVariables(QueryNode node, Action<QVar> setVar) {
			if (node is QueryConditionNode) {
				var cndNode = (QueryConditionNode)node;
				if (cndNode.LValue is QVar)
					setVar( (QVar) cndNode.LValue);
				if (cndNode.RValue is QVar)
					setVar( (QVar) cndNode.RValue);
			}
			if (node != null)
				foreach (var cNode in node.Nodes)
					SetQueryVariables(cNode, setVar);
		}

	}
}
