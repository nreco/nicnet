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
using System.Collections;

using NI.Common;
using NI.Common.Providers;

namespace NI.Security.Permissions
{
	/// <summary>
	/// </summary>
	public class GroupListProvider : IObjectListProvider
	{
		IBooleanProvider[] _Groups = new IBooleanProvider[0];
		
		/// <summary>
		/// Get or set group list
		/// </summary>
		[Dependency]
		public IBooleanProvider[] Groups {
			get { return _Groups; }
			set { _Groups = value; }
		}
		
		public GroupListProvider()
		{
		}

		public IList GetObjectList(object obj) {
			ArrayList groups = new ArrayList();
			groups.Add(obj);
			foreach (IBooleanProvider objGroup in Groups)
				if (objGroup.GetBoolean(obj))
					groups.Add(objGroup);
			return groups;
		}

	}
}
