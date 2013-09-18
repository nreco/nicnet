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

namespace NI.Data {
	
	[Serializable]
	public class QueryRawSqlNode : QueryNode {

		/// <summary>
		/// Nodes collection
		/// </summary>
		public override IList<QueryNode> Nodes { get { return new QueryNode[0]; } }
		
		public string SqlText {
			get; private set;
		}
	
		public QueryRawSqlNode(string sqlText) {
			SqlText = sqlText;
		}
	
		

		
	}


	
}
