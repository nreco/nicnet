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
		protected FileStream InputFileStream = null;
		protected FileStream OutputFileStream = null;
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
		/// <see cref="IFileContent.InputStream"/>
		/// </summary>
		public Stream InputStream {
			get {
				if (File.Type!=FileType.File)
					throw new FileSystemException(File.Name + " is not a file!"); // TODO: more structured exception
				
				if (InputFileStream!=null) InputFileStream.Close();
				// raise open event
				if (LocalFs.EventsMediator!=null)
					LocalFs.EventsMediator.OnFileOpening(new FileObjectOpenEventArgs(LocalFs,File,FileAccess.Read));
				
				InputFileStream = new FileStream(LocalFile.LocalName, FileMode.OpenOrCreate, FileAccess.Read, LocalFs.InputFileShare);
				return InputFileStream;
			}
		}

		/// <summary>
		/// <see cref="IFileContent.OutputStream"/>
		/// </summary>
		public Stream OutputStream {
			get {
				if (File.Type!=FileType.File)
					throw new FileSystemException(); // TODO: more structured exception
				
				if (OutputFileStream!=null) OutputFileStream.Close();

				// raise open event
				if (LocalFs.EventsMediator != null)
					LocalFs.EventsMediator.OnFileOpening(new FileObjectOpenEventArgs(LocalFs, File, FileAccess.Write));
				
				OutputFileStream = new FileStream(LocalFile.LocalName, FileMode.OpenOrCreate, FileAccess.Write, LocalFs.OutputFileShare);
				return OutputFileStream;
			}
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
			if (InputFileStream!=null) {
				InputFileStream.Close();
				InputFileStream = null;
			}
			
			if (OutputFileStream!=null) {
				OutputFileStream.Close();
				InputFileStream = null;
			}
		}

	}
}
