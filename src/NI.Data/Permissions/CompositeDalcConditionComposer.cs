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
using System.Security.Principal;
using NI.Data;

namespace NI.Data.Permissions
{
	/// <summary>
	/// Composite IDalcConditionComposer implementation.
	/// </summary>
	public class CompositeDalcConditionComposer : IDalcConditionComposer
	{
		IDalcConditionComposer[] _ConditionComposers;

		public IDalcConditionComposer[] ConditionComposers { get; set; }
	
		public CompositeDalcConditionComposer() {
		}

		public QueryNode Compose(IPrincipal user, DalcOperation operation, string sourceName) {
			QueryGroupNode groupAnd = new QueryGroupNode(GroupType.And);
			for (int i=0; i<ConditionComposers.Length; i++)
				groupAnd.Nodes.Add( ConditionComposers[i].Compose(user, operation, sourceName) );
			return groupAnd;
		}

	}
}
