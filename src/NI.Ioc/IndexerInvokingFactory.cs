#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas,  Vitalii Fedorchenko (v.2 changes)
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
using System.Reflection;

namespace NI.Ioc {

	/// <summary>
	/// Factory component which returns a value represented by specified object's indexer property.
	/// </summary>
	public class IndexerInvokingFactory : IFactoryComponent {
		/// <summary>
		/// Get or set target object
		/// </summary>
		public object TargetObject { get; set; }
		
		/// <summary>
		/// Get or set indexer arguments
		/// </summary>
		public object[] IndexerArgs { get; set; }

		public IndexerInvokingFactory() {
		}
		
		public object GetObject() {
			IndexerProxy indexer = new IndexerProxy(TargetObject);
			return indexer[IndexerArgs];
		}
		
		public Type GetObjectType() {
			IndexerProxy indexer = new IndexerProxy(TargetObject);
			return indexer[IndexerArgs]!=null ? indexer[IndexerArgs].GetType() : typeof(object);
		}		
		
		
	}
}
