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
using System.ComponentModel;

namespace NI.Common
{
	/// <summary>
	/// Component with automatic dependency resolving ability
	/// </summary>
	public class Component : System.ComponentModel.Component
	{
		IDependencyResolver _DependencyResolver = null;
		
		/// <summary>
		/// Get or set dependency resolver component
		/// </summary>
		[Dependency(Required=false)]
		public IDependencyResolver DependencyResolver {
			get { return _DependencyResolver; }
			set { _DependencyResolver = value; }
		}
		
		/// <summary>
		/// Get or Set component Site
		/// </summary>
		public override ISite Site {
			get { return base.Site; }
			set {
				base.Site = value;
				if (DependencyResolver!=null && value!=null)
					DependencyResolver.Resolve(this, value);
			}
		}		
	}
}
