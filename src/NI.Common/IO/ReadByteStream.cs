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

namespace NI.Common.IO
{
	/// <summary>
	/// Stream wrapper that uses only 'ReadByte' method when reading from stream.
	/// </summary>
	public class ReadByteStream : FilterStream
	{
		public ReadByteStream(Stream stream) : base(stream)
		{
		}
		
		public override long Length {
			get { throw new NotSupportedException(); }
		}

		
		public override int Read(byte[] b, int off, int len) {
			if (b == null) {
				throw new NullReferenceException();
			} else if ((off < 0) || (off > b.Length) || (len < 0) ||
					((off + len) > b.Length) || ((off + len) < 0)) {
				throw new IndexOutOfRangeException();
			} else if (len == 0) {
				return 0;
			}

			int c = ReadByte();
			if (c == -1) {
				return -1;
			}
			b[off] = (byte)c;

			int i = 1;

			for (; i < len ; i++) {
				c = ReadByte();
				if (c == -1) {
					break;
				}
				if (b != null) {
					b[off + i] = (byte)c;
				}
			}
			
			return i;
		} 
		
	}
}
