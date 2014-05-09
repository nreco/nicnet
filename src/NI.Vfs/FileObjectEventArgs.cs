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
using System.Data;
using System.Data.Common;
using System.IO;

namespace NI.Vfs
{

	/// <summary>
	/// File event arguments
	/// </summary>
	public class FileObjectEventArgs : EventArgs {

		/// <summary>
		/// Context <see cref="IFileObject"/> instance
		/// </summary>
		public IFileObject File { get; private set; }
		
		public FileObjectEventArgs(IFileObject fileObj) {
			File = fileObj;
		}
		
	}

	/// <summary>
	/// File create event arguments
	/// </summary>
	public class FileObjectCreateEventArgs : FileObjectEventArgs {

		/// <summary>
		/// Type of the file being created
		/// </summary>
		public FileType Type { get; private set; }

		public FileObjectCreateEventArgs(IFileObject file, FileType type)
			: base(file) {
			Type = type;
		}
	}
	
	/// <summary>
	/// File move event arguments
	/// </summary>
	public class FileObjectMoveEventArgs : FileObjectEventArgs {
		
		/// <summary>
		/// Represents move destination <see cref="IFileObject"/> instance
		/// </summary>
		public IFileObject Destination { get; private set; }
		
		public FileObjectMoveEventArgs(IFileObject file, IFileObject dest) : base(file) {
			Destination = dest;
		}
	}

	/// <summary>
	/// File copy event arguments
	/// </summary>
	public class FileObjectCopyEventArgs : FileObjectEventArgs {

		/// <summary>
		/// Represents copy from source <see cref="IFileObject"/> instance
		/// </summary>
		public IFileObject Source { get; private set; }

		public FileObjectCopyEventArgs(IFileObject file, IFileObject src)
			: base(file) {
			Source = src;
		}
	}

	/// <summary>
	/// File open event arguments
	/// </summary>
	public class FileObjectOpenEventArgs : FileObjectEventArgs {

		public FileAccess Access { get; private set; }

		public FileObjectOpenEventArgs(IFileObject file, FileAccess accessType)
			: base(file) {
			Access = accessType;
		}
	}

	public class FileObjectErrorEventArgs : FileObjectEventArgs {

		public Exception FileException { get; private set; }

		public FileObjectErrorEventArgs(IFileObject file, Exception ex)
			: base(file) {
			FileException = ex;
		}
	}
	
	
	
}
