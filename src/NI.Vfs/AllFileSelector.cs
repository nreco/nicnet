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
	/// A IFileSelector that selects all files
	/// </summary>
	public class AllFileSelector : IFileSelector
	{
		FileType FilterType = FileType.Imaginary;
		
		public AllFileSelector()
		{
		}
		
		public AllFileSelector(FileType filterType) {
			FilterType = filterType;
		}
		
		public bool IncludeFile(IFileObject file) {
			return FilterType == FileType.Imaginary ? true : file.Type == FilterType;
		}
		
		public bool TraverseDescendents(IFileObject file) {
			return true;
		}		
		
	}
}
