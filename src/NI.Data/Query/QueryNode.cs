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

namespace NI.Data
{
	/// <summary>
	/// Represents abstract query node that contains child nodes.
	/// </summary>
	[Serializable]
	public abstract class QueryNode
	{
		public abstract IList<QueryNode> Nodes { get; }

		public string Name { get; set; }	
	
		internal QueryNode() {
		}

		/// <summary>
		/// OR operator
		/// </summary>
		public static QueryGroupNode operator | (QueryNode node1, QueryNode node2) {
			QueryGroupNode res = new QueryGroupNode(QueryGroupNodeType.Or);
			res.Nodes.Add(node1);
			res.Nodes.Add(node2);
			return res;
		}

		/// <summary>
		/// AND operator
		/// </summary>
		public static QueryGroupNode operator & (QueryNode node1, QueryNode node2) {
			QueryGroupNode res = new QueryGroupNode(QueryGroupNodeType.And);
			res.Nodes.Add(node1);
			res.Nodes.Add(node2);
			return res;
		}



	}
}
