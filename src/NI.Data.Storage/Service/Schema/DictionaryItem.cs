#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013 Vitalii Fedorchenko
 * Copyright 2014 NewtonIdeas
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
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Data;
using System.Threading.Tasks;
using System.Globalization;
using System.Security.Permissions;

namespace NI.Data.Storage.Service.Schema {
	
	[Serializable]
	public class DictionaryItem : ISerializable {
		public IDictionary<string,object> Data;

		public DictionaryItem() {
			Data = new Dictionary<string,object>();
		}

		public DictionaryItem(IDictionary<string,object> data) {
			Data = data;
		}

		//[SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		protected DictionaryItem(SerializationInfo info, StreamingContext context) {
			var e = info.GetEnumerator();
			Data = new Dictionary<string,object>();
			while (e.MoveNext()) {
				Data[e.Name] = e.Value;
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			foreach (var entry in Data) {
				if (entry.Value == null || DBNull.Value.Equals(entry.Value))
					continue;

				info.AddValue(entry.Key, entry.Value, entry.Value.GetType());
			}
		}

	}

}
