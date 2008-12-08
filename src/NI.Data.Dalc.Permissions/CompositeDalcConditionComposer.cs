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
using NI.Data.Dalc;

namespace NI.Data.Dalc.Permissions
{
	/// <summary>
	/// Composite IDalcConditionComposer implementation.
	/// </summary>
	public class CompositeDalcConditionComposer : IDalcConditionComposer
	{
		IDalcConditionComposer[] _ConditionComposers;
		
		public IDalcConditionComposer[] ConditionComposers {
			get { return _ConditionComposers; }
			set { _ConditionComposers = value; }
		}
	
		public CompositeDalcConditionComposer() {
		}

		public IQueryNode Compose(object subject, DalcOperation operation, string sourceName) {
			QueryGroupNode groupAnd = new QueryGroupNode(GroupType.And);
			for (int i=0; i<ConditionComposers.Length; i++)
				groupAnd.Nodes.Add( ConditionComposers[i].Compose(subject, operation, sourceName) );
			return groupAnd;
		}

	}
}
