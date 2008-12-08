#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2008 NewtonIdeas
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
using System.Runtime.Serialization;

namespace NI.Data.Dalc
{
	
	[Serializable]
	public class QueryGroupNode : IQueryGroupNode, INamedQueryNode {
		
		private QueryNodeCollection _Nodes;
		private GroupType _Group;
		string _Name = null;

		public string Name {
			get { return _Name; }
			set { _Name = value; }
		}


	
		/// <summary>
		/// Nodes collection
		/// </summary>
		IEnumerable IQueryNode.Nodes { get { return _Nodes; } }
		
		public QueryNodeCollection Nodes { get { return _Nodes; } }
		
		/// <summary>
		/// Group type
		/// </summary>
		public GroupType Group { get { return _Group; } }
		
		public QueryGroupNode(GroupType type) {
			_Group = type;
			_Nodes = new QueryNodeCollection();
		}
		
		public QueryGroupNode(string name, GroupType type) : this(type) {
			Name = name;
		}

		public QueryGroupNode(IQueryGroupNode likeGroup)
			: this(likeGroup.Group) {
			if (likeGroup is INamedQueryNode)
				Name = ((INamedQueryNode)likeGroup).Name;
		}
		

		/// <summary>
		/// OR operator
		/// </summary>
		public static QueryGroupNode operator | (QueryGroupNode node1, IQueryNode node2) {
			
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
		public static QueryGroupNode operator & (QueryGroupNode node1, IQueryNode node2) {
			
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


	
}
