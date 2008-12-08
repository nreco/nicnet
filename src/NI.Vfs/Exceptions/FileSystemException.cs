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

namespace NI.Vfs {

	/// <summary>
	/// File system exception
	/// </summary>
	public class FileSystemException : ApplicationException {
		
		IFileObject _File = null;
		
		public IFileObject File {
			get { return _File; }
		}
		
		public FileSystemException() : base("File system error") {
		}
		
		public FileSystemException(string message) : base (message) { }

		public FileSystemException(string message, IFileObject file) : base(message) { 
			_File = file;
		}

		public FileSystemException(string message, Exception innerException) : base (message, innerException) { }

		public FileSystemException(string message, IFileObject file, Exception innerException) : base(message, innerException) { 
			_File = file;
		}


	}
}
