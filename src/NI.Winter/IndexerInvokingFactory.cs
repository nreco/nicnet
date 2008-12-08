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
using System.Reflection;

using NI.Common;

namespace NI.Winter {

	/// <summary>
	/// Indexer invoking factory used for defining instance by indexer of another object.
	/// </summary>
	public class IndexerInvokingFactory : Component, IFactoryComponent {
		object _TargetObject;
		object[] _IndexerArgs;
	
		/// <summary>
		/// Get or set target object
		/// </summary>
		[Dependency]
		public object TargetObject {
			get { return _TargetObject; }
			set { _TargetObject = value; }
		}
		
		/// <summary>
		/// Get or set indexer arguments
		/// </summary>
		[Dependency]
		public object[] IndexerArgs {
			get { return _IndexerArgs; }
			set { _IndexerArgs = value; }
		}

		
		
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
