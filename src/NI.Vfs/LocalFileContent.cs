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
	/// The content of a local file. 
	/// </summary>
	public class LocalFileContent : IFileContent
	{
		protected LocalFile LocalFile;
		protected LocalFileSystem LocalFs;
		protected FileStream CurrentFileStream = null;
		protected FileInfo LocalFileInfo;
		

		internal LocalFileContent(LocalFile file, LocalFileSystem localFs)
		{
			LocalFile = file;
			LocalFs = localFs;
			try {
				LocalFileInfo = new FileInfo(LocalFile.LocalName);
			} catch (Exception ex) {
				throw new Exception(
					String.Format("Cannot retrieve file info ({0}): {1}", LocalFile.LocalName, ex.Message), ex);
			}
		}
		

		/// <summary>
		/// <see cref="IFileContent.File"/>
		/// </summary
		public IFileObject File {
			get { return LocalFile; }
		}
		
		/// <summary>
		/// <see cref="IFileContent.GetStream"/>
		/// </summary>
		public Stream GetStream(FileAccess access) {
			if (File.Type!=FileType.File)
				throw new FileSystemException(File.Name + " is not a file!"); // TODO: more structured exception
				
			if (CurrentFileStream!=null) CurrentFileStream.Close();
			// raise open event
			LocalFs.OnFileOpen(this.File, access);

			CurrentFileStream = new FileStream(LocalFile.LocalName, FileMode.OpenOrCreate, access, LocalFs.InputFileShare);
			return CurrentFileStream;
		}

		/// <summary>
		/// <see cref="IFileContent.Size"/>
		/// </summary>
		public long Size {
			get {
				if (File.Type!=FileType.File)
					return 0;
				return LocalFileInfo.Length;
			}
		}

		/// <summary>
		/// <see cref="IFileContent.LastModifiedTime"/>
		/// </summary>
		public DateTime LastModifiedTime {
			get { return LocalFileInfo.LastWriteTime; }
			set { LocalFileInfo.LastWriteTime = value; }
		}

		/// <summary>
		/// <see cref="IFileContent.Close"/>
		/// </summary>
		public void Close() {
			if (CurrentFileStream!=null) {
				CurrentFileStream.Close();
				CurrentFileStream = null;
			}
		}

	}
}
