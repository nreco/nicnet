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

namespace NI.Vfs
{
	/// <summary>
	/// The content of a in-memory file. 
	/// </summary>
	public class MemoryFileContent : IFileContent
	{
		protected MemoryFile MemoryFile;
		protected MemoryStream MemFileStream = null;
		protected DateTime _LastModifiedTime = DateTime.Now;
		
		public MemoryFileContent(MemoryFile file)
		{
			MemoryFile = file;
		}

		public IFileObject File {
			get { return MemoryFile; }
		}

		public Stream InputStream {
			get {
				if (File.Type!=FileType.File)
					throw new FileSystemException(); // TODO: more structured exception
				ReopenMemoryStream();
				return MemFileStream;
			}
		}

		public Stream OutputStream {
			get {
				if (File.Type!=FileType.File)
					throw new FileSystemException(); // TODO: more structured exception
				ReopenMemoryStream();
				return MemFileStream;
			}
		}

		public long Size {
			get { return MemFileStream.Length; }
		}

		public DateTime LastModifiedTime {
			get { return _LastModifiedTime; }
			set { _LastModifiedTime = value; }
		}

		protected void ReopenMemoryStream() {
			MemoryStream newMemFileStream = new MemoryStream();
			if (MemFileStream!=null) {
				MemFileStream.Close();
				byte[] data = MemFileStream.ToArray();
				newMemFileStream.Write(data,0,data.Length);
				newMemFileStream.Seek(0, SeekOrigin.Begin);
			}
			MemFileStream = newMemFileStream;			
		}

		public void Close() {
			if (MemFileStream!=null) {
				MemFileStream.Close();
			}
		}

	}
}
