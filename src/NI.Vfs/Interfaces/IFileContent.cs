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
	/// Represents the data content of a file.
	/// </summary>
	public interface IFileContent
	{
		/// <summary>
		/// Returns the file which this is the content of.
		/// </summary>
		IFileObject File { get; }
		
		/// <summary>
		/// Returns a stream for reading or writing the file's content.
		/// </summary>
		/// <param name="access">file access type (read or write).</param>
		/// <remarks>Note that access type <see cref="FileAccess.ReadWrite"/> option may not be supported by some implementations.</remarks>
		/// <returns>An stream to read or write the file's content.</returns>
		Stream GetStream(FileAccess access);
		
		/// <summary>
		/// Determines the size of the file, in bytes.
		/// </summary>
		long Size { get; }
		
		/// <summary>
		/// Get or set the last-modified timestamp of the file.
		/// </summary>
		DateTime LastModifiedTime { get; set; }
		
		/// <summary>
		/// Closes all resources used by the content, including any open stream.
		/// </summary>
		void Close();
	}
}
