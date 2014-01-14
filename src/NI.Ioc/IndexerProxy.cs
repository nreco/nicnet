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
using System.Collections;
using System.Data;
using System.Linq;
using System.Reflection;

namespace NI.Ioc
{

	// Proxy-class that can be used for accessing indexer of an object
	internal class IndexerProxy {
		object IndexedObj;

		public IndexerProxy(object indexed_obj) {
			IndexedObj = indexed_obj;
		}

		public static implicit operator IndexerProxy(DataRow o) {
			return new IndexerProxy(o);
		}

		public static implicit operator IndexerProxy(Hashtable o) {
			return new IndexerProxy(o);
		}

		public object this[params object[] i] {
			get {
				//special handling of arrays
				if (IndexedObj is Array) {
					return ((Array)IndexedObj).GetValue(i.Select(ii => Convert.ToInt32(ii)).ToArray());
				}

				Type[] arg_types = new Type[i.Length];
				for (int k = 0; k < i.Length; k++)
					arg_types[k] = i[k].GetType();

				MethodInfo methodInfo = IndexedObj.GetType().GetMethod("get_Item", arg_types);
				if (methodInfo != null)
					return methodInfo.Invoke(IndexedObj, i);

				throw new NotImplementedException("Cannot find get indexer for such arguments");
			}
			set {
				//special handling of arrays
				if (IndexedObj is Array) {
					((Array)IndexedObj).SetValue(value, i.Select(ii => Convert.ToInt32(ii)).ToArray());
					return;
				}

				Type[] arg_types = new Type[i.Length + 1];
				object[] args = new object[i.Length + 1];
				for (int k = 0; k < i.Length; k++) {
					arg_types[k] = i[k].GetType();
					args[k] = i[k];
				}
				arg_types[i.Length] = value != null ? value.GetType() : typeof(object);
				args[i.Length] = value;

				MethodInfo methodInfo = IndexedObj.GetType().GetMethod("set_Item", arg_types);
				if (methodInfo != null)
					methodInfo.Invoke(IndexedObj, args);
				else
					throw new NotImplementedException("Cannot find set indexer for such arguments");
			}
		}

	}
	
	
}
