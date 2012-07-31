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
	public enum FileType {
		Imaginary = 0,
		File = 1, 
		Folder = 2 
	}
	
	/// <summary>
	/// Represents a file, and is used to access the content and structure of the file. 
	/// </summary>
	public interface IFileObject
	{
		/// <summary>
		/// Returns the name of this file.
		/// </summary>
		string Name { get; }
		
		/// <summary>
		/// Returns this file's type. 
		/// </summary>
		FileType Type { get; }
		
		/// <summary>
		/// Returns the folder that contains this file.
		/// </summary>
		IFileObject Parent { get; }
		
		/// <summary>
		/// Closes this file, and its content. This method is a hint to the
		/// implementation that it can release any resources asociated with the file. 
		/// </summary>
		void Close();
		
		/// <summary>
		/// Copies another file, and all its descendents, to this file
		/// </summary>
		/// <remarks>
		/// Copies another file, and all its descendents, to this file. 
		/// If this file does not exist, it is created. Its parent folder is also created, if necessary.
		/// If this file does exist, it is deleted first. 
		/// This method is not transactional. If it fails and throws an exception, this file will potentially only be partially copied. 
		/// </remarks>
		/// <param name="srcFile">The source file to copy</param>
		void CopyFrom(IFileObject srcFile);

		/// <summary>
		/// Copies data to this file
		/// </summary>
		/// <remarks>
		/// If this file does not exist, it is created.
		/// If this file does exist, it is deleted first.
		/// </remarks>
		/// <param name="data">data input stream</param>
		void CopyFrom(Stream inputStream);
		
		/// <summary>
		/// Creates this file, if it does not exist. Also creates any ancestor folders
		/// which do not exist. This method does nothing if the file already exists and
		/// is a file. 
		/// </summary>
		void CreateFile();
		
		/// <summary>
		/// Creates this folder, if it does not exist. Also creates any ancestor folders
		/// which do not exist. This method does nothing if the folder already exists.
		/// </summary>
		void CreateFolder();
		
		/// <summary>
		/// Deletes this file.
		/// </summary>
		void Delete();
		
		/// <summary>
		/// Determines if this file exists.
		/// </summary>
		bool Exists();

		/// <summary>
		/// Lists the children of this file. 
		/// </summary>
		/// <returns>
		/// An array containing the children of this file. The array is unordered. If the
		/// file does not have any children, a zero-length array is returned. This method
		/// never returns null.
		/// </returns>
		IFileObject[] GetChildren();
		
		/// <summary>
		/// Returns this file's content.
		/// </summary>
		IFileContent GetContent();
		
		/// <summary>
		/// Finds the set of matching descendents of this file, in depthwise order.
		/// </summary>
		IFileObject[] FindFiles(IFileSelector selector);
		
		/// <summary>
		/// Move this file.
		/// </summary>
		void MoveTo(IFileObject destFile);

		
	}
}
