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
using System.IO;

namespace NI.Vfs
{
	/// <summary>
	/// A IFileSelector that selects files by explicit list of extensions
	/// </summary>
	public class ExtensionFileSelector : IFileSelector
	{
		protected string[] Extensions;
		const string ExtensionSeparator = ".";
		StringComparison Comparision = StringComparison.CurrentCultureIgnoreCase;

		public ExtensionFileSelector(params string[] extensions) {
			Extensions = new string[extensions.Length];
			// normalize extensions
			for (int i = 0; i < Extensions.Length; i++)
				Extensions[i] = extensions[i].StartsWith(ExtensionSeparator) ? extensions[i] : "." + extensions[i];
		}
		
		public bool IncludeFile(IFileObject file) {
			string ext = Path.GetExtension( file.Name );
			for (int i = 0; i < Extensions.Length; i++)
				if (Extensions[i].Equals(ext, Comparision))
					return true;
			return false;
		}
		
		public bool TraverseDescendents(IFileObject file) {
			return true;
		}		
		
	}
}
