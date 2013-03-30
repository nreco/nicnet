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
	/// A file object implementation which uses in-memory file access
	/// </summary>
	public class MemoryFile : IFileObject
	{

		string _Name;
		FileType _Type = FileType.Imaginary;
		int _CopyBufferLength = 64*1024; //64kb

		protected MemoryFileSystem MemoryFs;
		
		protected MemoryFileContent FileContent = null;
		
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
		
		public string Name {
			get { return _Name; }
		}
		
		public FileType Type {
			get { return _Type; }
		}
		
		public IFileObject Parent {
			get { return MemoryFs.ResolveFile( Path.GetDirectoryName(Name) );  }
		}
			
		public MemoryFile(string name, FileType type, MemoryFileSystem memFilesystem) {
			_Name = name;
			MemoryFs = memFilesystem;
			_Type = type;
		}

		public virtual void Close() {
			if (FileContent!=null) {
				FileContent.Close();
				FileContent = null;
			}
		}
		
		
		public virtual void CopyFrom(IFileObject srcFile) {
			if (srcFile.Type==FileType.File) {
				using (Stream inputStream = srcFile.GetContent().InputStream) {
					CopyFrom( inputStream );
				}
			}
			if (srcFile.Type==FileType.Folder) {
				CopyFrom( srcFile.GetChildren() );
			}
		}
		
		protected virtual void CopyFrom(IFileObject[] srcEntries) {
			if (Type==FileType.File) Delete();
			if (Type==FileType.Imaginary) CreateFolder();
			
			foreach (IFileObject srcFile in srcEntries) {
				string destFileName = Path.Combine( Name, Path.GetFileName(srcFile.Name) );
				IFileObject destFile = MemoryFs.ResolveFile( destFileName );
				destFile.CopyFrom( srcFile );
			}
		}
		
		public virtual void CopyFrom(Stream inputStream) {
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
		
		}
		
		public virtual void CreateFile() {
			if (Type==FileType.Imaginary) {
				string fileParentLocalFolder = Path.GetDirectoryName(Name);
				if (fileParentLocalFolder.Length>0) {
					IFileObject parentFolder = MemoryFs.ResolveFile(fileParentLocalFolder);
					parentFolder.CreateFolder();
				}
			}		
			_Type = FileType.File;
			Close();
		}
		
		public virtual void CreateFolder() {
			_Type = FileType.Folder;
		}
		
		public virtual void Delete() {
			if (Type==FileType.Folder) {
				foreach (IFileObject memFile in MemoryFs.MemoryFiles)
					if (memFile.Exists() && memFile.Name.StartsWith( Name+Path.DirectorySeparatorChar.ToString() ) )
						memFile.Delete();
			}
			_Type = FileType.Imaginary;
		}
		
		public virtual bool Exists() {
			return Type!=FileType.Imaginary;
		}
		
		public virtual IFileObject[] GetChildren() {
			if (Type!=FileType.Folder)
				throw new FileSystemException(); // TODO: more structured exception
			
			ArrayList children = new ArrayList();
			foreach (IFileObject memFile in MemoryFs.MemoryFiles)
				if (memFile.Exists() && memFile.Name.Length>0 && Path.GetDirectoryName(memFile.Name)==Name )
					children.Add(memFile);
			return children.ToArray(typeof(IFileObject)) as IFileObject[];
		}
		
		public IFileContent GetContent() {
			if (FileContent==null)
				FileContent = new MemoryFileContent(this);
			return FileContent;
		}		
		
		public IFileObject[] FindFiles(IFileSelector selector) {
			if (Type!=FileType.Folder)
				throw new FileSystemException(); // TODO: more structured exception

			ArrayList resultList = new ArrayList();
			IFileObject[] children = GetChildren();
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
		
		/// <summary>
		/// <see cref="IFileObject.MoveTo"/>
		/// </summary>
		public virtual void MoveTo(IFileObject destFile) {
			// copy-delete
			destFile.CopyFrom(this);
			this.Delete();
		}		
		

		
		
	}
}
