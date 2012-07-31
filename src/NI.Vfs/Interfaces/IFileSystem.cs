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
	/// </summary>
	public interface IFileSystem
	{
		/// <summary>
		/// Returns the root file of this file system. 
		/// </summary>
		IFileObject Root { get; }
		
		/// <summary>
		/// Finds a file in this file system. 
		/// </summary>
		IFileObject ResolveFile(string name);
	}
}
