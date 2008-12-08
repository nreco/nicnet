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

namespace NI.Data.Dalc
{
	/// <summary>
	/// Base class for query nodes.
	/// </summary>
	[Serializable]
	public abstract class QueryNode : IQueryNode, INamedQueryNode
	{
		string _Name = null;
		
		public abstract IEnumerable Nodes { get; }
		
		public string Name {
			get { return _Name; }
			set { _Name = value; }
		}	
	
		public QueryNode()
		{
		}
		
		/// <summary>
		/// OR operator
		/// </summary>
		public static QueryGroupNode operator | (QueryNode node1, IQueryNode node2) {
			QueryGroupNode res = new QueryGroupNode(GroupType.Or);
			res.Nodes.Add(node1);
			res.Nodes.Add(node2);
			return res;
		}

		/// <summary>
		/// AND operator
		/// </summary>
		public static QueryGroupNode operator & (QueryNode node1, IQueryNode node2) {
			QueryGroupNode res = new QueryGroupNode(GroupType.And);
			res.Nodes.Add(node1);
			res.Nodes.Add(node2);
			return res;
		}



	}
}
