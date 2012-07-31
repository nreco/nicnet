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
	/// A IFileSelector that selects files by explicit list of names 
	/// </summary>
	public class ListFileSelector : IFileSelector
	{
		protected string[] Names;
		StringComparison Comparision = StringComparison.CurrentCultureIgnoreCase;
	
		public ListFileSelector(params string[] names)
		{
			Names = new string[names.Length];
			// normalize file names
			for (int i=0; i<Names.Length; i++)
				Names[i] = names[i].Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar );
		}
		
		public bool IncludeFile(IFileObject file) {
			string normFileName = file.Name.Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar );
			string onlyFileName = Path.GetFileName(normFileName);
			for (int i = 0; i < Names.Length; i++) {
				bool isFullPath = Names[i].IndexOf(Path.DirectorySeparatorChar) >= 0;
				if (isFullPath && Names[i].Equals(normFileName, Comparision))
					return true;
				if (!isFullPath && Names[i].Equals(onlyFileName, Comparision))
					return true;
			}
			return false;
		}
		
		public bool TraverseDescendents(IFileObject file) {
			// TODO: more intellectual behaviour should be implemented here
			return true;
		}		
		
	}
}
