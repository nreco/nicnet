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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace NI.Common {

	/// <summary>
	/// Generic implementation of IBinarySerializer interface based on underlying .NET IFormatter component.
	/// </summary>
	public class BinarySerializer : IBinarySerializer {

		IFormatter _BinaryFormatter;
		
		/// <summary>
		/// Get or set underyling IFormatter component
		/// </summary>
		public IFormatter BinaryFormatter {
			get {
				if (_BinaryFormatter==null)
					_BinaryFormatter = new BinaryFormatter();
				return _BinaryFormatter;
			}
			set { _BinaryFormatter = value; }
		}

		/// <summary>
		/// Initializes a new instance of the BinarySerializer class.
		/// </summary>
		public BinarySerializer() {
		}

		/// <summary>
		/// <see cref="IStringSerializer.ToByteArray"/>
		/// </summary>
		public byte[] ToByteArray(object obj) {
			MemoryStream mem = new MemoryStream();
			BinaryFormatter.Serialize(mem, obj);
			return mem.GetBuffer();
		}

		/// <summary>
		/// <see cref="IStringSerializer.FromByteArray"/>
		/// </summary>
		public object FromByteArray(byte[] buffer) {
			MemoryStream mem = new MemoryStream(buffer);
			return BinaryFormatter.Deserialize(mem);
		}
		

		

	}
}