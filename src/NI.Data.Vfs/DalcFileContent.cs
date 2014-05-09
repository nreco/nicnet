#region License
/*
 * NIC.NET library
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
using System.Collections.Generic;
using System.Text;
using System.IO;

using NI.Vfs;

namespace NI.Data.Vfs {
    /// <summary>
    /// Dalc base file content implementation
    /// </summary>
    public class DalcFileContent: IFileContent {

        protected DalcFileObject _File;
        protected DalcFileStream fileStream = null;
		protected DateTime _LastModifiedTime = DateTime.Now;
		
		public DalcFileContent(DalcFileObject file) {
			_File = file;
		}

        /// <summary>
        /// Instance of file object
        /// </summary>
		public IFileObject File {
			get { return _File; }
		}

        /// <summary>
        /// Name (key) of content
        /// </summary>
        public string Name {
            get { return File.Name; }
        }

		public Stream GetStream(FileAccess access) {
			if (File.Type!=FileType.File)
				throw new FileSystemException(); // TODO: more structured exception
			ReopenStream();
			return fileStream;
		}

		public long Size {
            get { return fileStream.Length; }
		}

		public DateTime LastModifiedTime {
			get { return _LastModifiedTime; }
			set { _LastModifiedTime = value; }
		}

		protected void ReopenStream() {
			DalcFileStream newFileStream = new DalcFileStream(this);
			if (fileStream!=null) {
				fileStream.CloseMemoryStream();
				byte[] data = fileStream.ToArray();
				newFileStream.Write(data,0,data.Length);
				newFileStream.Seek(0, SeekOrigin.Begin);
			}
			fileStream = newFileStream;			
		}

		public void Close() {
			if (fileStream!=null) {
                _File.SaveContent(); //Save content in storage
				fileStream.CloseMemoryStream(); //Close memory(underlying) stream
			}
		}

    }
}
