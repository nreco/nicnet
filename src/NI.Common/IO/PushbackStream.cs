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
using System.IO;

namespace NI.Common.IO {

	public class PushbackStream : FilterStream {

		protected byte[] buf;

		protected int pos;

		private void EnsureOpen() {
			if (UnderlyingStream == null)
				throw new IOException("Stream closed");
		}

		public PushbackStream(Stream inStream, int size) : base(inStream) {
			if (size <= 0) {
				throw new ArgumentException("size <= 0");
			}
			this.buf = new byte[size];
			this.pos = size;
		}

		public PushbackStream(Stream inStream) : this(inStream, 1) {
		}

		public override int ReadByte() {
			EnsureOpen();
			if (pos < buf.Length) {
				return buf[pos++] & 0xff;
			}
			return base.ReadByte();
		}

		public override int Read(byte[] b, int off, int len) {
			EnsureOpen();
			if ((off < 0) || (off > b.Length) || (len < 0) || ((off + len) > b.Length) || ((off + len) < 0)) {
				throw new IndexOutOfRangeException();
			}

			if (len == 0)
				return 0;

			int avail = buf.Length - pos;
			if (avail > 0) {
				if (len < avail) {
					avail = len;
				}
				Array.Copy(buf, pos, b, off, avail);
			
				pos += avail;
				off += avail;
				len -= avail;
			}
			if (len > 0) {
				len = base.Read(b, off, len);
				if (len == -1) {
					return avail == 0 ? -1 : avail;
				}
				return avail + len;
			}
			return avail;
		}

		public void Unread(int b) {
			EnsureOpen();
			if (pos == 0) {
				throw new IOException("Push back buffer is full");
			}
			buf[--pos] = (byte)b;
		}

		public void Unread(byte[] b, int off, int len) {
			EnsureOpen();
			if (len > pos) {
				throw new IOException("Push back buffer is full");
			}
			pos -= len;
			Array.Copy(b, off, buf, pos, len);
		}

		public void Unread(byte[] b) {
			Unread(b, 0, b.Length);
		}

		public override bool CanSeek {
			get { return false; }
		}


		/*public int Available {
			EnsureOpen();
			return (buf.Length - pos) + super.available();
		}*/
		
		public override long Seek(long offset, SeekOrigin origin) {
			throw new NotSupportedException();
		}

		public override void Close() {
			if (UnderlyingStream == null) return;
			UnderlyingStream.Close();
			UnderlyingStream = null;
			buf = null;
		}

	}
	
} 