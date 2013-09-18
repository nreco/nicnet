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
	
	[Serializable]
	public class QueryGroupNode : QueryNode {
		
		private List<QueryNode> _Nodes;
		private GroupType _Group;

	
		/// <summary>
		/// Nodes collection
		/// </summary>
		public override IList<QueryNode> Nodes {
			get {
				return _Nodes;
			}
		}
		
		/// <summary>
		/// Group type
		/// </summary>
		public GroupType Group { get; set; }
		
		public QueryGroupNode(GroupType type) {
			Group = type;
			_Nodes = new List<QueryNode>();
		}
		
		public QueryGroupNode(QueryGroupNode likeGroup)
			: this(likeGroup.Group) { 
			Name = likeGroup.Name;
			_Nodes.AddRange(likeGroup.Nodes);
		}
		

		/// <summary>
		/// OR operator
		/// </summary>
		public static QueryGroupNode operator | (QueryGroupNode node1, QueryNode node2) {
			
			if ( node1.Group==GroupType.Or) {
				node1.Nodes.Add( node2 );
				return node1;
			}
			QueryGroupNode res = new QueryGroupNode(GroupType.Or);
			res.Nodes.Add(node1);
			res.Nodes.Add(node2);
			return res;
		}

		/// <summary>
		/// AND operator
		/// </summary>
		public static QueryGroupNode operator & (QueryGroupNode node1, QueryNode node2) {
			
			if ( node1.Group==GroupType.And) {
				node1.Nodes.Add( node2 );
				return node1;
			}
			QueryGroupNode res = new QueryGroupNode(GroupType.And);
			res.Nodes.Add(node1);
			res.Nodes.Add(node2);
			return res;
		}	

	}

	public enum GroupType {
		Or, And
	}	


	
}
