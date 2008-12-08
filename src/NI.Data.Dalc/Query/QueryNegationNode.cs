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

namespace NI.Data.Dalc {
	
	[Serializable]
	public class QueryNegationNode : QueryNode, IQueryNegationNode {
		
		private IQueryNode _Node;
	
		/// <summary>
		/// Nodes collection
		/// </summary>
		public override IEnumerable Nodes { get { return new IQueryNode[] { _Node }; } }
		
		public IQueryNode ExpressionNode { get { return _Node; } }
		
		public QueryNegationNode(IQueryNode node) {
			_Node = node;
		}

		
	}


	
}
