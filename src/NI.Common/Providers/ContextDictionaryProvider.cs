#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
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
using System.Collections.Specialized;
using System.Text;

namespace NI.Common.Providers {
	
	/// <summary>
	/// Default 'context provider' used in providers where context treated as IDictionary.
	/// </summary>
	public class ContextDictionaryProvider : IDictionaryProvider, IObjectProvider {
		string _ContextArgumentKey = "arg";

		public string ContextArgumentKey {
			get { return _ContextArgumentKey; }
			set { _ContextArgumentKey = value; }
		}
	
		static ContextDictionaryProvider _DefaultInstance = new ContextDictionaryProvider();
		public static ContextDictionaryProvider DefaultInstance {
			get {
				return _DefaultInstance;
			}
		}
		
		public ContextDictionaryProvider() { }
		
		public IDictionary GetDictionary(object contextObj) {
			if (contextObj is IDictionary) {
				return (IDictionary)contextObj;
			} else {
				IDictionary context = new ListDictionary();
				context[ContextArgumentKey] = contextObj;
				return context;
			}				
		}

		public object GetObject(object context) {
			return GetDictionary(context);
		}
	}
}
