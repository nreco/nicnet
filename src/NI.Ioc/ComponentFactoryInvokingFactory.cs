#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas,  Vitalii Fedorchenko (v.2 changes)
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

namespace NI.Ioc
{
	/// <summary>
	/// Factory that returns specific component from owning component factory.
	/// </summary>
	public class ComponentFactoryInvokingFactory : IComponentFactoryAware, IFactoryComponent
	{
		
		/// <summary>
		/// Get or set component factory instance
		/// </summary>
		public IComponentFactory ComponentFactory { get; set; }
		
		/// <summary>
		/// Get or set component name to retrieve from component factory
		/// </summary>
		public string ServiceName { get; set; }
		
		public ComponentFactoryInvokingFactory()
		{
		}
		
		public object GetObject() {
			return ComponentFactory.GetComponent(ServiceName);
		}
		
		public Type GetObjectType() {
			return typeof(object);
		}
		
		
	}
}
