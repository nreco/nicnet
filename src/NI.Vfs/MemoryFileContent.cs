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

namespace NI.Vfs
{
	/// <summary>
	/// The content of a in-memory file. 
	/// </summary>
	public class MemoryFileContent : IFileContent
	{
		protected MemoryFile MemoryFile;
		protected MemoryStream LastStream = null;
		protected DateTime _LastModifiedTime = DateTime.Now;
		
		protected byte[] ContentBytes = null;

		public MemoryFileContent(MemoryFile file)
		{
			MemoryFile = file;
		}

		public IFileObject File {
			get { return MemoryFile; }
		}

		public Stream GetStream(FileAccess access) {
			if (File.Type!=FileType.File)
				throw new FileSystemException("Not a file"); // TODO: more structured exception

			if (ContentBytes!=null && access==FileAccess.Read)
				return new MemoryStream(ContentBytes, false);

			LastStream = new MemoryFileStream(this);
			if (ContentBytes != null) {
				LastStream.Write(ContentBytes,0,ContentBytes.Length);
				LastStream.Seek(0, SeekOrigin.Begin);
			}
			return LastStream;
		}

		public long Size {
			get { 
				return ContentBytes!=null ? ContentBytes.Length : 0; 
			}
		}

		public DateTime LastModifiedTime {
			get { return _LastModifiedTime; }
			set { _LastModifiedTime = value; }
		}

		public void Close() {
			if (LastStream!=null) {
				LastStream.Close();
			}
		}

		internal class MemoryFileStream : MemoryStream {
			MemoryFileContent FileContent;

			public MemoryFileStream(MemoryFileContent fileContent) {
				FileContent = fileContent;
			}

			public override void Close() {
				base.Close();
				FileContent.ContentBytes = ToArray();
			}

		}

	}
}
