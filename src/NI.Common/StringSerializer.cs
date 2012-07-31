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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;

namespace NI.Common
{
	/// <summary>
	/// Generic implementation of IStringSerializer interface based on underlying .NET IFormatter component.
	/// </summary>
	public class StringSerializer : IStringSerializer
	{
		IFormatter _StringFormatter;
		
		/// <summary>
		/// Get or set underyling IFormatter component
		/// </summary>
		public IFormatter StringFormatter {
			get {
				if (_StringFormatter!=null) {
					_StringFormatter = new Base64FormatterProxy( new BinaryFormatter() );
				}
				return _StringFormatter;
			}
			set { _StringFormatter = value; }
		}		
		
		/// <summary>
		/// Initializes a new instance of the StringSerializer class.
		/// </summary>
		public StringSerializer() {
		}

		public StringSerializer(IFormatter stringFormatter) {
		}		

		/// <summary>
		/// <see cref="IStringSerializer.ToString"/>
		/// </summary>
		public string ToString(object obj) {
			MemoryStream mem = new MemoryStream();
			StringFormatter.Serialize(mem, obj);
			mem.Position = 0;
			StreamReader strRdr = new StreamReader(mem);
			return strRdr.ReadToEnd();
		}
		
		/// <summary>
		/// <see cref="IStringSerializer.FromString"/>
		/// </summary>
		public object FromString(string data) {
			MemoryStream mem = new MemoryStream();
			StreamWriter strWr = new StreamWriter(mem);
			strWr.Write(data);
			strWr.Flush();
			mem.Position = 0;
			return StringFormatter.Deserialize(mem);
		}
		
		public class Base64FormatterProxy : IFormatter {
			IFormatter UnderlyingFormatter;
			
			public Base64FormatterProxy(IFormatter formatter) {
				UnderlyingFormatter = formatter;
			}
			
			public SerializationBinder Binder {
				get { return UnderlyingFormatter.Binder; }
				set { UnderlyingFormatter.Binder = value; }
			}

			public StreamingContext Context {
				get { return UnderlyingFormatter.Context; }
				set { UnderlyingFormatter.Context = value; }
			}

			public object Deserialize(Stream serializationStream) {
				StreamReader streamRdr = new StreamReader(serializationStream);
				string base64str = streamRdr.ReadToEnd();
				byte[] buf = Convert.FromBase64String(base64str);
				MemoryStream memStream = new MemoryStream(buf);
				return UnderlyingFormatter.Deserialize( memStream );
			}

			public void Serialize(Stream serializationStream, object graph) {
				MemoryStream memStream = new MemoryStream();
				UnderlyingFormatter.Serialize(memStream, graph);
				byte[] buf = memStream.GetBuffer();
                StreamWriter wr = new StreamWriter(serializationStream);
				wr.Write( Convert.ToBase64String(buf) );
			}

			public ISurrogateSelector SurrogateSelector {
				get { return UnderlyingFormatter.SurrogateSelector; }
				set { UnderlyingFormatter.SurrogateSelector = value; }
			}
		}
}
}
