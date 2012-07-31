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
using System.Text.RegularExpressions;
using System.IO;

namespace NI.Vfs
{
	/// <summary>
	/// A IFileSelector that selects files by regular expression
	/// </summary>
	public class RegexFileSelector : IFileSelector
	{
		protected string FilenameRegexPattern;
		protected Regex FilenameRegex;
		
		public RegexFileSelector(string fileNameRegex) {
			FilenameRegexPattern = fileNameRegex;
			FilenameRegex = new Regex(FilenameRegexPattern, RegexOptions.Compiled|RegexOptions.Singleline);
		}
		
		public bool IncludeFile(IFileObject file) {
			return FilenameRegex.IsMatch(file.Name);
		}
		
		public bool TraverseDescendents(IFileObject file) {
			// TODO: more intellectual behaviour should be implemented here
			return true;
		}
	}
}
