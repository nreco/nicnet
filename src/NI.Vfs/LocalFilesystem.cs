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
		IFileObjectEventsMediator _EventsMediator = null;
		
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
		
		public IFileObjectEventsMediator EventsMediator {
			get { return _EventsMediator; }
			set { _EventsMediator = value; }
		}

		public LocalFileSystem() {
		}
		
		public LocalFileSystem(string rootFolder) {
			RootFolder = rootFolder;
		}

		public IFileObject Root {
			get { return ResolveFile(""); }
		}
		
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
		

	}
}
