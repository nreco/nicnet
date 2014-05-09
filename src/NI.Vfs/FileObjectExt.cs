#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
 * Copyright 2008-2013 Vitalii Fedorchenko (changes and v.2)
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
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

namespace NI.Vfs {
	
	/// <summary>
	/// Extension methods for <see cref="NI.Vfs.IFileObject"/> interface
	/// </summary>
	public static class FileObjectExt {
		
		/// <summary>
		/// Reads the contents of the file into a byte array
		/// </summary>
		/// <param name="file">IFileObject instance</param>
		/// <returns>A byte array containing the contents of the file</returns>
		public static byte[] ReadAllBytes(this IFileObject file) {
			using (var fs = file.Content.GetStream(FileAccess.Read)) {
				var buf = new byte[fs.Length];
				fs.Read(buf, 0, buf.Length);
				return buf;
			}
		}

		/// <summary>
		/// Writes the specified byte array to the file. If the target file already exists, it is overwritten.
		/// </summary>
		/// <param name="file">IFileObject instance</param>
		/// <param name="bytes">The bytes to write to the file.</param>
		public static void WriteAllBytes(this IFileObject file, byte[] bytes) {
			if (file.Type == FileType.File) {
				file.Delete();
				file.CreateFile();
			}
			using (var fs = file.Content.GetStream(FileAccess.Write)) {
				fs.Write(bytes, 0, bytes.Length);
			}
		}

		/// <summary>
		/// Reads all lines of the text file
		/// </summary>
		/// <param name="file">IFileObject instance</param>
		/// <returns>A string containing all lines of the file.</returns>
		public static string ReadAllText(this IFileObject file) {
			using (var fs = file.Content.GetStream(FileAccess.Read)) {
				return new StreamReader(fs).ReadToEnd();
			}
		}

		/// <summary>
		/// Writes the specified string to the file. If the target file already exists, it is overwritten.
		/// </summary>
		/// <param name="file">IFileObject instance</param>
		/// <param name="text">The string to write to the file.</param>
		public static void WriteAllText(this IFileObject file, string text) {
			WriteAllBytes(file, Encoding.Default.GetBytes(text));
		}

	}
}
