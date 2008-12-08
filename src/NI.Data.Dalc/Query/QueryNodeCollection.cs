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


	[Serializable]
	public class QueryNodeCollection : CollectionBase {
		
		public QueryNodeCollection() {
		}
		
		public void Add(IQueryNode item) {
			if (item==null) return;
			List.Add(item);
		}
		
		
		/*public DataConditionNode Find(string field_name) {
			for (int i=0; i<Items.Count; i++) {
				if (Items[i] is DataConditionNode)
					if ( ((DataConditionNode)Items[i]).Field == Filter.Prefix+field_name )
						return (DataConditionNode)Items[i];
				if (Items[i] is GroupConditionNode) {
					DataConditionNode res = ((GroupConditionNode)Items[i]).Find(field_name);
					if (res!=null) return res;
				}
					
			}
			return null;
		}*/	
		
		public IQueryNode this[int index] {
			get { return (IQueryNode) this.List[index]; }
			set { List[index] = value; }
		}
		
	}


}
