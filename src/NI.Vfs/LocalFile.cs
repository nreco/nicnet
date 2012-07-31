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
using System.Collections;

namespace NI.Vfs
{
	/// <summary>
	/// A file object implementation which uses direct file access. 
	/// </summary>
	public class LocalFile : IFileObject
	{
		string _Name;
		FileType _Type = FileType.Imaginary;
		int _CopyBufferLength = 64*1024; //64kb

		protected LocalFileSystem LocalFs;
		protected LocalFileContent FileContent = null;
		
		/// <summary>
		/// Get or set buffer length used when copying file
		/// </summary>
		public int CopyBufferLength {
			get { return _CopyBufferLength; }
			set {
				if (value<=0) throw new ArgumentOutOfRangeException();
				_CopyBufferLength = value;
			}
		}
		
		public string LocalName {
			get { 
				return Path.Combine(LocalFs.RootFolder,Name); 
			}
		}
		
		public string Name {
			get { return _Name; }
		}
		
		public FileType Type {
			get { return _Type; }
		}
		
		public IFileObject Parent {
			get { return LocalFs.ResolveFile( Path.GetDirectoryName(Name) );  }
		}
			
		public LocalFile(string name, LocalFileSystem localFilesystem) {
			_Name = name;
			LocalFs = localFilesystem;
			
			if (File.Exists(LocalName))
				_Type = FileType.File;
			else if (Directory.Exists(LocalName))
				_Type = FileType.Folder;
			
		}

		public LocalFile(string name, FileType fileType, LocalFileSystem localFilesystem) {
			_Name = name;
			LocalFs = localFilesystem;
			_Type = fileType;
		}

		/// <summary>
		/// <see cref="IFileObject.Close"/>
		/// </summary>
		public virtual void Close() {
			if (FileContent!=null) {
				FileContent.Close();
				FileContent = null;
			}
		}
		
		/// <summary>
		/// <see cref="IFileObject.CopyFrom"/>
		/// </summary>		
		public virtual void CopyFrom(IFileObject srcFile) {
			// raise 'before copy'
			if (LocalFs.EventsMediator!=null)
				LocalFs.EventsMediator.OnFileCopying(new FileObjectEventArgs(LocalFs,this));
				
			try {
				if (srcFile.Type==FileType.File) {
					using (Stream inputStream = srcFile.GetContent().InputStream) {
						CopyFrom( inputStream );
					}
				}
				if (srcFile.Type==FileType.Folder) {
					CopyFrom( srcFile.GetChildren() );
				}
				
			} catch (Exception ex) {
				// raise 'error'
				if (LocalFs.EventsMediator != null)
					LocalFs.EventsMediator.OnFileError(new FileObjectErrorEventArgs(LocalFs, this,ex));
				throw new FileSystemException(ex.Message, this, ex);
			}

			// raise 'after copy'
			if (LocalFs.EventsMediator != null)
				LocalFs.EventsMediator.OnFileCopied(new FileObjectEventArgs(LocalFs, this));
		}
		
		/// <summary>
		/// <see cref="IFileObject.CopyFrom"/>
		/// </summary>
		protected virtual void CopyFrom(IFileObject[] srcEntries) {
			if (Type==FileType.File) Delete();
			if (Type==FileType.Imaginary) CreateFolder();
			
			foreach (IFileObject srcFile in srcEntries) {
				string destFileName = Path.Combine( Name, Path.GetFileName(srcFile.Name) );
				IFileObject destFile = LocalFs.ResolveFile( destFileName );
				destFile.CopyFrom( srcFile );
			}
		}
		
		/// <summary>
		/// <see cref="IFileObject.CopyFrom"/>
		/// </summary>
		public virtual void CopyFrom(Stream inputStream) {
			// raise 'before copy'
			if (LocalFs.EventsMediator != null)
				LocalFs.EventsMediator.OnFileCopying(new FileObjectEventArgs(LocalFs, this));			
			
			try {
				if (Type!=FileType.Imaginary) Delete();
				CreateFile();
				Stream outputStream = GetContent().OutputStream;
				
				byte[] buf = new byte[CopyBufferLength];
				try {
					int bytesRead = 0;
					do {
						bytesRead = inputStream.Read(buf, 0, CopyBufferLength);
						if (bytesRead>0)
							outputStream.Write(buf, 0, bytesRead);
					} while (bytesRead>0);
				} finally {
					outputStream.Close();
				}
			} catch (Exception ex) {
				// raise 'error'
				if (LocalFs.EventsMediator != null)
					LocalFs.EventsMediator.OnFileError(new FileObjectErrorEventArgs(LocalFs, this, ex));
				throw new FileSystemException(ex.Message, this, ex);
			}

			// raise 'after copy'
			if (LocalFs.EventsMediator != null)
				LocalFs.EventsMediator.OnFileCopied(new FileObjectEventArgs(LocalFs, this));
		
		}
		
		/// <summary>
		/// <see cref="IFileObject.MoveTo"/>
		/// </summary>
		public virtual void MoveTo(IFileObject destFile) {
			// raise 'before move'
			if (LocalFs.EventsMediator != null)
				LocalFs.EventsMediator.OnFileMoving(new FileObjectMoveEventArgs(LocalFs, this, destFile));			
			
			try {		
				if (destFile is LocalFile) {
					// delete destination file
					if (destFile.Type!=FileType.Imaginary)
						destFile.Delete();
					
					// use fast API move functions...
					if (Type==FileType.File)
						File.Move(LocalName, ((LocalFile)destFile).LocalName);
					if (Type==FileType.Folder)
						Directory.Move(LocalName, ((LocalFile)destFile).LocalName);
					
				} else {
					// copy-delete
					destFile.CopyFrom(this);
					this.Delete();
				}
			} catch (Exception ex) {
				// raise 'error'
				if (LocalFs.EventsMediator != null)
					LocalFs.EventsMediator.OnFileError(new FileObjectErrorEventArgs(LocalFs, this, ex));
				throw new FileSystemException(ex.Message, this, ex);
			}

			// raise 'after move'
			if (LocalFs.EventsMediator != null)
				LocalFs.EventsMediator.OnFileMoved(new FileObjectMoveEventArgs(LocalFs, this,destFile));
		}
		
		/// <summary>
		/// Creates this file, if it does not exist. Also creates any ancestor folders
		/// which do not exist. This method throws IOException if this file or folder already exists        
		/// </summary>		
		public virtual void CreateFile() {
			if (LocalFs.ReadOnly)
				throw new InvalidOperationException("This instance of file system is read only");
			
			// raise 'before create'
			if (LocalFs.EventsMediator != null)
				LocalFs.EventsMediator.OnFileCreating(new FileObjectEventArgs(LocalFs, this));			
			
			try {

				if (Type != FileType.Imaginary)
					throw new IOException("Cannot create a file when that file already exists");

				string fileParentLocalFolder = Path.GetDirectoryName(LocalName);
				if (!Directory.Exists(fileParentLocalFolder))
					Directory.CreateDirectory(fileParentLocalFolder);
					
				FileStream fs = File.Create(LocalName);
				fs.Close();

				_Type = FileType.File;
			} catch (Exception ex) {
				// raise 'error'
				if (LocalFs.EventsMediator != null)
					LocalFs.EventsMediator.OnFileError(new FileObjectErrorEventArgs(LocalFs, this, ex));
				throw new FileSystemException(ex.Message, this, ex);
			}

			// raise 'after create'
			if (LocalFs.EventsMediator != null)
				LocalFs.EventsMediator.OnFileCreated(new FileObjectEventArgs(LocalFs, this));
		}
		
		/// <summary>
		/// <see cref="IFileObject.CreateFolder"/>
		/// </summary>			
		public virtual void CreateFolder() {
			if (LocalFs.ReadOnly)
				throw new InvalidOperationException("This instance of file system is read only");
			
			// raise 'before create'
			if (LocalFs.EventsMediator != null)
				LocalFs.EventsMediator.OnFolderCreating(new FileObjectEventArgs(LocalFs, this));			
			
			try {			
				if (Type==FileType.File) Delete();
				Directory.CreateDirectory(LocalName);

				_Type = FileType.Folder;
			} catch (Exception ex) {
				// raise 'error'
				if (LocalFs.EventsMediator != null)
					LocalFs.EventsMediator.OnFileError(new FileObjectErrorEventArgs(LocalFs, this, ex));
				throw new FileSystemException(ex.Message, this, ex);
			}

			// raise 'after create'
			if (LocalFs.EventsMediator != null)
				LocalFs.EventsMediator.OnFileCreated(new FileObjectEventArgs(LocalFs, this));			
		}
		
		/// <summary>
		/// <see cref="IFileObject.Delete"/>
		/// </summary>		
		public virtual void Delete() {
			if (LocalFs.ReadOnly)
				throw new InvalidOperationException("This instance of file system is read only");
			
			// raise 'before delete'
			if (LocalFs.EventsMediator != null)
				LocalFs.EventsMediator.OnFileDeleting(new FileObjectEventArgs(LocalFs, this));			
			
			try {			
				switch (Type) {
					case FileType.File: File.Delete(LocalName);
						break;
					case FileType.Folder: Directory.Delete(LocalName,true);
						break;
				}
				_Type = FileType.Imaginary;
			} catch (Exception ex) {
				// raise 'error'
				if (LocalFs.EventsMediator != null)
					LocalFs.EventsMediator.OnFileError(new FileObjectErrorEventArgs(LocalFs, this, ex));
				throw new FileSystemException(ex.Message, this, ex);
			}

			// raise 'after delete'
			if (LocalFs.EventsMediator != null)
				LocalFs.EventsMediator.OnFileDeleted(new FileObjectEventArgs(LocalFs, this));				
		}
		
		/// <summary>
		/// <see cref="IFileObject.Exists"/>
		/// </summary>		
		public virtual bool Exists() {
			return Type!=FileType.Imaginary;
		}
		
		public virtual IFileObject[] GetChildren() {
			if (Type!=FileType.Folder)
				throw new FileSystemException("Folder does not exist"); // TODO: more structured exception
			
			ChildrenList childrenList = GetChildrenList();
			string[] fileEntries = childrenList.FileEntries;
			string[] directoryEntries = childrenList.DirectoryEntries;

			IFileObject[] children = new IFileObject[fileEntries.Length+directoryEntries.Length];
			for (int i=0; i<directoryEntries.Length; i++)
				children[i] = LocalFs.ResolveFile( Path.Combine(Name, Path.GetFileName(directoryEntries[i])  ), FileType.Folder );
			for (int i=0; i<fileEntries.Length; i++)
				children[directoryEntries.Length+i] = LocalFs.ResolveFile( Path.Combine(Name, Path.GetFileName(fileEntries[i])  ), FileType.File );
			return children;
		}
		
		/// <summary>
		/// <see cref="IFileObject.GetContent"/>
		/// </summary>		
		public IFileContent GetContent() {
			if (FileContent==null)
				FileContent = new LocalFileContent(this, LocalFs);
			return FileContent;
		}
		
		/// <summary>
		/// <see cref="IFileObject.FindFiles"/>
		/// </summary>		
		public IFileObject[] FindFiles(IFileSelector selector) {
			if (Type!=FileType.Folder)
				throw new FileSystemException("Cannot search files in non-folder"); // TODO: more structured exception

			IFileObject[] children = GetChildren();
			ArrayList resultList = new ArrayList(children.Length);
			foreach (IFileObject file in children) {
				if (selector.IncludeFile(file)) resultList.Add(file);
				if (file.Type==FileType.Folder)
					if (selector.TraverseDescendents(file)) {
						IFileObject[] foundFiles = file.FindFiles(selector);
						resultList.AddRange( foundFiles );
					}
			}
			return (IFileObject[])resultList.ToArray(typeof(IFileObject));
		}

		protected ChildrenList GetChildrenList() {
			ChildrenList childrenList = null;
			string cacheKey = LocalName+"|ChildrenList";
			if (LocalFs.Cache!=null)
				childrenList = LocalFs.Cache.Get(cacheKey) as ChildrenList;
			// invalidate cache entry if folder was modified
			if (childrenList!=null && Directory.GetLastWriteTime(LocalName)>childrenList.Timestamp)
				childrenList = null;
			
			if (childrenList==null)
				childrenList = new ChildrenList( Directory.GetFiles(LocalName), Directory.GetDirectories(LocalName) );
			
			if (LocalFs.Cache!=null)
				LocalFs.Cache.Put(cacheKey, childrenList);

			return childrenList;
		}

		protected class ChildrenList {
			public string[] FileEntries;
			public string[] DirectoryEntries;
			public DateTime Timestamp;

			public ChildrenList(string[] fileEntries, string[] dirEntries) {
				FileEntries = fileEntries;
				DirectoryEntries = dirEntries;
				Timestamp = DateTime.Now;
			}

		}
		
		
	}
}
