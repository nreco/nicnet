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
using System.Text;

namespace NI.Vfs
{
	/// <summary>
	/// A IFileSelector that selects files by given mask
	/// </summary>
	public class MaskFileSelector : IFileSelector
	{
		protected string Mask;
		protected Regex MaskRegex;
		protected string PathPrefix;

		static char[] wildcardChars = { '*', '?' }, pathDelimiters ={ '/', '\\' };

		public MaskFileSelector(string mask) {
			Mask = mask;
			string regEx = GetMaskRegex( mask );
			regEx = @"(?:^|\/|\\)" + regEx + "$";
			
			MaskRegex = new Regex(regEx, RegexOptions.Compiled|RegexOptions.ExplicitCapture);

			PathPrefix = GetMaskParentPath(mask);
			if (PathPrefix != null)
				PathPrefix = PathPrefix.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		}

		public static string GetMaskParentPath(string mask) {
			int wildcardIdx = mask.IndexOfAny(wildcardChars), slashIdx;
			if (wildcardIdx == -1) {
				slashIdx = mask.LastIndexOfAny(pathDelimiters);
				if (slashIdx != -1)
					return mask.Substring(0, slashIdx + 1);
			} else if (wildcardIdx > 1) {
				slashIdx = mask.LastIndexOfAny(pathDelimiters, wildcardIdx - 1);
				if (slashIdx != -1)
					return mask.Substring(0, slashIdx + 1);
			}
			return null;
		}

		protected virtual string GetMaskRegex(string mask) {
			var sb = new StringBuilder();
			for (int i = 0; i < mask.Length; i++) {
				switch (mask[i]) {
					case '\\':
						sb.Append(@"[\/\\]");
						break;
					case '/':
						sb.Append(@"[\/\\]");
						break;
					case '*':
						if ((i + 1) < mask.Length && mask[i + 1] == '*') {
							// handle **
							i++;
							sb.Append(".*?");
						} else {
							sb.Append(@"[^\/\\]*");
						}
						break;
					case '?':
						sb.Append(@"[^\/\\]");
						break;
					default:
						sb.AppendFormat("[{0}]", mask[i]);
						break;
				}
			}
			return sb.ToString();
		}

		public bool IncludeFile(IFileObject file) {
			return MaskRegex.IsMatch(file.Name);
		}
		
		public bool TraverseDescendents(IFileObject file) {
			if (PathPrefix == null)
				return true;
			string fname = file.Name.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar) +
				Path.DirectorySeparatorChar;
			if (PathPrefix.Length > fname.Length)
				return PathPrefix.StartsWith(fname, false, null);
			else
				return fname.StartsWith(PathPrefix, false, null);
		}
	}
}
