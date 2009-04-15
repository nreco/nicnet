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

namespace NI.Common.IO {

	/// <summary>
	/// A <code>FilterInputStream</code> contains
	/// some other input stream, which it uses as
	/// its  basic source of data, possibly transforming
	/// the data along the way or providing  additional
	/// functionality. The class <code>FilterInputStream</code>
	/// itself simply overrides all  methods of
	/// <code>InputStream</code> with versions that
	/// pass all requests to the contained  input
	/// stream. Subclasses of <code>FilterInputStream</code>
	/// may further override some of  these methods
	/// and may also provide additional methods
	/// and fields. 	
	/// </summary>
	public class FilterStream : Stream {

		protected Stream stream;

		public FilterStream(Stream stream) {
			this.stream = stream;
		}

		protected virtual Stream UnderlyingStream {
			get { return stream; }
			set { stream = value; }
		}


		public override bool CanRead {
			get { return UnderlyingStream.CanRead; }
		}
		
		public override bool CanWrite {
			get { return UnderlyingStream.CanWrite; }
		}
		
		public override bool CanSeek {
			get { return UnderlyingStream.CanSeek;  }
		}
		
		public override void WriteByte(byte value) {
			UnderlyingStream.WriteByte(value);
		}
		
		public override void Write(byte[] buffer, int offset, int count) {
			UnderlyingStream.Write(buffer, offset, count);
		}
		
		public override int Read(byte[] buffer, int offset, int count) {
			return UnderlyingStream.Read(buffer, offset, count);
		}

		public override int ReadByte() {
			return UnderlyingStream.ReadByte();
		}
		
		public override void Flush() {
			UnderlyingStream.Flush();
		}

		
		public override void Close() {
			UnderlyingStream.Close();
		}
		
		public override long Length {
			get { return UnderlyingStream.Length; }
		}

		public override long Position {
			get { return UnderlyingStream.Position; }
			set { UnderlyingStream.Position = value; }
		}

		public override long Seek(long offset, SeekOrigin origin) {
			return UnderlyingStream.Seek(offset, origin);
		}
		
		public override void SetLength(long value) {
			UnderlyingStream.SetLength(value);
		}

	}


}