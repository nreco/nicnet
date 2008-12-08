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

namespace NI.Security.Permissions
{
	/// <summary>
	/// Represents one permission.
	/// </summary>
	public class Permission
	{
		object _Subject;
		object _Operation;
		object _Object;
		
		public object Subject { get { return _Subject; } }
		public object Operation { get { return _Operation; } }
		public object Object { get { return _Object; } }
		
		public Permission(object subj, object op, object obj)
		{
			_Subject = subj;
			_Operation = op;
			_Object = obj;
		}
	}
}
