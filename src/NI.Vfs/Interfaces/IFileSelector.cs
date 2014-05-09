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

namespace NI.Vfs
{
	/// <summary>
	/// This interface is used to select files when traversing a file hierarchy.
	/// </summary>
	public interface IFileSelector
	{
		/// <summary>
		/// Determines if a file or folder should be selected.  
		/// This method is called in depthwise order (that is, it is called for the children of a folder before it is called for the folder itself).
		/// </summary>
		/// <param name="file">the file or folder to select</param>
		/// <returns>true if the file should be selected.</returns>
		bool IncludeFile(IFileObject file);

		/// <summary>
		/// Determines whether a folder should be traversed.  If this method returns true <see cref="IFileSelector.IncludeFile"/>
		/// is called for each of the children of the folder, and each of the child folders is recursively traversed.
		/// </summary>
		/// <param name="file">the file or folder to select.</param>
		/// <returns>true if the folder should be traversed.</returns>
		bool TraverseDescendents(IFileObject file);
	}
}
