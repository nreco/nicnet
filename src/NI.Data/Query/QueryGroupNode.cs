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
using System.Runtime.Serialization;

namespace NI.Data
{
	
	/// <summary>
	/// Represents group of nodes combined with logical OR/AND operator
	/// </summary>
	[Serializable]
	public class QueryGroupNode : QueryNode {
		
		private List<QueryNode> _Nodes;
	
		/// <summary>
		/// List of group child nodes
		/// </summary>
		public override IList<QueryNode> Nodes {
			get {
				return _Nodes;
			}
		}
		
		/// <summary>
		/// Logical operator type (<see cref="NI.Data.QueryGroupNodeType"/>)
		/// </summary>
		public QueryGroupNodeType GroupType { get; set; }
		
		/// <summary>
		/// Initializes a new instance of the QueryGroupNode with specified logical operator
		/// </summary>
		/// <param name="type">group logical operator (<see cref="NI.Data.QueryGroupNodeType"/>)</param>
		public QueryGroupNode(QueryGroupNodeType type) {
			GroupType = type;
			_Nodes = new List<QueryNode>();
		}
		
		/// <summary>
		/// Initializes a new instance of the QueryGroupNode that copies specified QueryGroupNode
		/// </summary>
		/// <param name="likeGroup">QueryGroupNode to copy from</param>
		public QueryGroupNode(QueryGroupNode likeGroup)
			: this(likeGroup.GroupType) { 
			Name = likeGroup.Name;
			_Nodes.AddRange(likeGroup.Nodes);
		}
		

		/// <summary>
		/// OR operator
		/// </summary>
		public static QueryGroupNode operator | (QueryGroupNode node1, QueryNode node2) {
			
			if ( node1.GroupType==QueryGroupNodeType.Or) {
				node1.Nodes.Add( node2 );
				return node1;
			}
			QueryGroupNode res = new QueryGroupNode(QueryGroupNodeType.Or);
			res.Nodes.Add(node1);
			res.Nodes.Add(node2);
			return res;
		}

		/// <summary>
		/// AND operator
		/// </summary>
		public static QueryGroupNode operator & (QueryGroupNode node1, QueryNode node2) {
			if ( node1.GroupType==QueryGroupNodeType.And) {
				node1.Nodes.Add( node2 );
				return node1;
			}
			QueryGroupNode res = new QueryGroupNode(QueryGroupNodeType.And);
			res.Nodes.Add(node1);
			res.Nodes.Add(node2);
			return res;
		}

		/// <summary>
		/// Compose AND group node with specified child nodes
		/// </summary>
		/// <param name="nodes">child nodes</param>
		/// <returns>QueryGroupNode of AND type</returns>
		public static QueryGroupNode And(params QueryNode[] nodes) {
			var andGrp = new QueryGroupNode(QueryGroupNodeType.And);
			andGrp._Nodes.AddRange(nodes);
			return andGrp;
		}

		/// <summary>
		/// Compose OR group node with specified child nodes
		/// </summary>
		/// <param name="nodes">child nodes</param>
		/// <returns>QueryGroupNode of OR type</returns>
		public static QueryGroupNode Or(params QueryNode[] nodes) {
			var orGrp = new QueryGroupNode(QueryGroupNodeType.Or);
			orGrp._Nodes.AddRange(nodes);
			return orGrp;
		}

	}

	/// <summary>
	/// Describes the group node types
	/// </summary>
	public enum QueryGroupNodeType {
		/// <summary>
		/// Logical OR group type
		/// </summary>
		Or, 

		/// <summary>
		/// Logical AND group type
		/// </summary>
		And
	}	


	
}
