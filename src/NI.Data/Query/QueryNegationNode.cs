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
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NI.Data {
	
	/// <summary>
	/// Represents logical negation operator
	/// </summary>
	[DebuggerDisplay("{Nodes}")]
	[Serializable]
	public class QueryNegationNode : QueryNode {
		
		private QueryNode[] SingleNodeList;
	
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public override IList<QueryNode> Nodes {
			get { return SingleNodeList; } 
		}
		
		/// <summary>
		/// Initializes a new instance of the QueryNegationNode that wraps specified node  
		/// </summary>
		/// <param name="node">condition node to negate</param>
		public QueryNegationNode(QueryNode node) {
			SingleNodeList = new QueryNode[] { node };
		}

		public QueryNegationNode(QueryNegationNode copyNode) {
			SingleNodeList = new QueryNode[] { copyNode.Nodes[0] };
			Name = copyNode.Name;
		}

		
	}


	
}
