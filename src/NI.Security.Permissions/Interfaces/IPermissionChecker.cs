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

namespace NI.Security.Permissions
{
	/// <summary>
	/// Generic permission checker interface
	/// </summary>
	public interface IPermissionChecker
	{
		/// <summary>
		/// Check permission
		/// </summary>
		/// <param name="permission">permission instance</param>
		/// <returns>true if permission is granted or false when denieded</returns>
		bool Check(Permission permission);

		/// <summary>
		/// Check permissions set at once
		/// </summary>
		/// <param name="permissions">permissions set</param>
		/// <returns>check results</returns>
		bool[] Check(Permission[] permissions);
	}
}
