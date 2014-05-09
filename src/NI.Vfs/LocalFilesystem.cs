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
	/// Local file system component
	/// </summary>
	public class LocalFileSystem : IFileSystem
	{
		string _RootFolder;
		bool _ReadOnly = false;
		FileShare _InputFileShare = FileShare.Read;
		FileShare _OutputFileShare = FileShare.Read;
		int _CopyBufferLength = 64 * 1024; //64kb

		/// <summary>
		/// Get or set root folder for this instance of local filesystem
		/// </summary>
		public string RootFolder {
			get { return _RootFolder; }
			set { _RootFolder = value; }
		}
		
		/// <summary>
		/// Get or set read only mode flag
		/// </summary>
		/// <remarks>
		/// When ReadOnly is true file system will allow only 'read' operations.
		/// </remarks>
		public bool ReadOnly {
			get { return _ReadOnly; }
			set { _ReadOnly = value; }
		}
		
		public FileShare InputFileShare {
			get { return _InputFileShare; }
			set { _InputFileShare = value; }
		}

		public FileShare OutputFileShare {
			get { return _OutputFileShare; }
			set { _OutputFileShare = value; }
		}

		/// <summary>
		/// Get or set buffer length used when copying file
		/// </summary>
		public int CopyBufferLength {
			get { return _CopyBufferLength; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException();
				_CopyBufferLength = value;
			}
		}

		public LocalFileSystem() {
		}
		
		public LocalFileSystem(string rootFolder) {
			RootFolder = rootFolder;
		}

		public IFileObject Root {
			get { return ResolveFile(""); }
		}

		/// <summary>
		/// Occurs before file or folder creation.
		/// </summary>
		public event EventHandler<FileObjectCreateEventArgs> Creating;

		/// <summary>
		/// Occurs after file or folder creation.
		/// </summary>
		public event EventHandler<FileObjectCreateEventArgs> Created;

		/// <summary>
		/// Occurs before file or folder deletion.
		/// </summary>
		public event EventHandler<FileObjectEventArgs> Deleting;

		/// <summary>
		/// Occurs after file or folder deletion.
		/// </summary>
		public event EventHandler<FileObjectEventArgs> Deleted;

		/// <summary>
		/// Occurs before file or folder moving.
		/// </summary>
		public event EventHandler<FileObjectMoveEventArgs> Moving;

		/// <summary>
		/// Occurs after file or folder moving.
		/// </summary>
		public event EventHandler<FileObjectMoveEventArgs> Moved;

		/// <summary>
		/// Occurs before file or folder copy from another <see cref="IFileObject"/>.
		/// </summary>
		public event EventHandler<FileObjectCopyEventArgs> Copying;

		/// <summary>
		/// Occurs after file or folder copy from another <see cref="IFileObject"/>.
		/// </summary>
		public event EventHandler<FileObjectCopyEventArgs> Copied;

		/// <summary>
		/// Occurs during file stream opening.
		/// </summary>
		public event EventHandler<FileObjectOpenEventArgs> Open;

		/// <summary>
		/// Occurs when any file error.
		/// </summary>
		public event EventHandler<FileObjectErrorEventArgs> Error;
		
		public IFileObject ResolveFile(string name) {
			LocalFile file = new LocalFile( NormalizeLocalName(name), this);
			return file;
		}

		public IFileObject ResolveFile(string name, FileType type) {
			return new LocalFile( NormalizeLocalName(name), type, this);
		}

		protected string NormalizeLocalName(string name) {
			if (name.Length>0) {
				if (name.Contains("..")) {
					var fakeRootPath = "f:\\";
					var fakeAbsolutePath = Path.GetFullPath(Path.Combine(fakeRootPath, name));
					name = fakeAbsolutePath.Substring(Path.GetPathRoot(fakeAbsolutePath).Length);
				}

				// normalize file name
				name = Path.Combine( Path.GetDirectoryName(name), Path.GetFileName(name) );
			}
			return name;
		}


		internal void OnFileCreating(IFileObject f, FileType type) {
			if (Creating != null)
				Creating(this, new FileObjectCreateEventArgs(f, type) );
		}

		internal void OnFileCreated(IFileObject f, FileType type) {
			if (Created != null)
				Created(this, new FileObjectCreateEventArgs(f, type));
		}

		internal void OnFileDeleting(IFileObject f) {
			if (Deleting != null)
				Deleting(this, new FileObjectEventArgs(f));
		}

		internal void OnFileDeleted(IFileObject f) {
			if (Deleted != null)
				Deleted(this, new FileObjectEventArgs(f));
		}

		internal void OnFileMoving(IFileObject f, IFileObject dest) {
			if (Moving != null)
				Moving(this, new FileObjectMoveEventArgs(f, dest));
		}

		internal void OnFileMoved(IFileObject f, IFileObject dest) {
			if (Moved != null)
				Moved(this, new FileObjectMoveEventArgs(f, dest));
		}

		internal void OnFileCopying(IFileObject f, IFileObject src) {
			if (Copying != null)
				Copying(this, new FileObjectCopyEventArgs(f, src) );
		}

		internal void OnFileCopied(IFileObject f, IFileObject src) {
			if (Copied != null)
				Copied(this, new FileObjectCopyEventArgs(f, src));
		}

		internal void OnFileOpen(IFileObject f, FileAccess access) {
			if (Open != null)
				Open(this, new FileObjectOpenEventArgs(f, access));
		}

		internal void OnFileError(IFileObject f, Exception ex) {
			if (Error != null)
				Error(this, new FileObjectErrorEventArgs(f, ex) );

		}


	}
}
