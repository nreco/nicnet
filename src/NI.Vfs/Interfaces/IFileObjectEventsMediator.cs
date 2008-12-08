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
using System.Data;
using System.Data.Common;
using System.IO;

namespace NI.Vfs
{
	/// <summary>
	/// File object events mediator interface.
	/// </summary>
	public interface IFileObjectEventsMediator
	{
		/// <summary>
		/// Occurs during file creation.
		/// </summary>
		event FileObjectEventHandler FileCreating;
		/// <summary>
		/// Occurs during folder creation.
		/// </summary>
		event FileObjectEventHandler FolderCreating;		
		/// <summary>
		/// Occurs during file or folder creation.
		/// </summary>
		event FileObjectEventHandler FileCreated;

		/// <summary>
		/// Occurs during file or folder deletion.
		/// </summary>
		event FileObjectEventHandler FileDeleting;
		/// <summary>
		/// Occurs during file or folder deletion.
		/// </summary>
		event FileObjectEventHandler FileDeleted;

		/// <summary>
		/// Occurs during file or folder moving.
		/// </summary>
		event FileObjectMoveEventHandler FileMoving;
		/// <summary>
		/// Occurs during file or folder moving.
		/// </summary>
		event FileObjectMoveEventHandler FileMoved;

		/// <summary>
		/// Occurs during file or folder copy.
		/// </summary>
		event FileObjectEventHandler FileCopying;
		/// <summary>
		/// Occurs during file or folder copy.
		/// </summary>
		event FileObjectEventHandler FileCopied;

		/// <summary>
		/// Occurs during file opening.
		/// </summary>
		event FileObjectOpenEventHandler FileOpening;

		/// <summary>
		/// Occurs during any file error between 'before' and 'after' events.
		/// </summary>
		event FileObjectErrorEventHandler FileError;
		
		/// <summary>
		/// Raise FileCreating event
		/// </summary>
		void OnFileCreating(FileObjectEventArgs e);
		/// <summary>
		/// Raise FolderCreating event
		/// </summary>
		void OnFolderCreating(FileObjectEventArgs e);
		/// <summary>
		/// Raise FileCreated event
		/// </summary>
		void OnFileCreated(FileObjectEventArgs e);
		
		
		/// <summary>
		/// Raise FileDeleting event
		/// </summary>
		void OnFileDeleting(FileObjectEventArgs e);
		/// <summary>
		/// Raise FileDeleted event
		/// </summary>
		void OnFileDeleted(FileObjectEventArgs e);

		/// <summary>
		/// Raise FileMoving event
		/// </summary>
		void OnFileMoving(FileObjectMoveEventArgs e);
		/// <summary>
		/// Raise FileMoved event
		/// </summary>
		void OnFileMoved(FileObjectMoveEventArgs e);

		/// <summary>
		/// Raise FileCopying event
		/// </summary>
		void OnFileCopying(FileObjectEventArgs e);
		/// <summary>
		/// Raise FileCopied event
		/// </summary>
		void OnFileCopied(FileObjectEventArgs e);

		/// <summary>
		/// Raise FileOpening event
		/// </summary>
		void OnFileOpening(FileObjectOpenEventArgs e);
		/// <summary>
		/// Raise FileError event
		/// </summary>
		void OnFileError(FileObjectErrorEventArgs e);
	}

	[Flags]
	public enum FileObjectEvents {
		None = 0,
		FileCreating = 1,
		FolderCreating = 2,
		Created = 4,
		Deleting = 8,
		Deleted = 16,
		Moving = 32,
		Moved = 64,
		Copying = 128,
		Copied = 256,
		FileOpening = 512,
		FileError = 1024
	}

	public delegate void FileObjectEventHandler(object sender, FileObjectEventArgs e);
	public delegate void FileObjectMoveEventHandler(object sender, FileObjectMoveEventArgs e);
	public delegate void FileObjectOpenEventHandler(object sender, FileObjectOpenEventArgs e);
	public delegate void FileObjectErrorEventHandler(object sender, FileObjectErrorEventArgs e);

	public class FileObjectEventArgs : EventArgs {
		IFileObject _File;
		IFileSystem _FileSystem;
		FileObjectEvents _Event = FileObjectEvents.None;
		
		public IFileObject File {
			get { return _File; }
		}
		public IFileSystem FileSystem {
			get { return _FileSystem; }
		}
		
		public FileObjectEventArgs(IFileSystem fs, IFileObject fileObj) {
			_FileSystem = fs;
			_File = fileObj;
		}
		
	}
	
	public class FileObjectMoveEventArgs : FileObjectEventArgs {
		IFileObject _Destination;
		
		public IFileObject Destination {
			get { return _Destination; }
		}
		
		public FileObjectMoveEventArgs(IFileSystem fs, IFileObject file, IFileObject dest) : base(fs,file) {
			_Destination = dest;
		}
	}

	public class FileObjectOpenEventArgs : FileObjectEventArgs {
		FileAccess _OpenType;

		public FileAccess OpenType {
			get { return _OpenType; }
		}

		public FileObjectOpenEventArgs(IFileSystem fs, IFileObject file, FileAccess accessType)
			: base(fs, file) {
			_OpenType = accessType;
		}
	}

	public class FileObjectErrorEventArgs : FileObjectEventArgs {
		Exception _FileException;

		public Exception FileException {
			get { return _FileException; }
		}

		public FileObjectErrorEventArgs(IFileSystem fs, IFileObject file, Exception ex)
			: base(fs, file) {
			_FileException = ex;
		}
	}
	
	
	
}
