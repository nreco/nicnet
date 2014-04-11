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
	/// Value initialization info represented by reference to another component definition
	/// </summary>
	public class RefValueInfo : IValueInitInfo
	{
		public IComponentInitInfo ComponentRef { get; private set; }
		
		public string ComponentMethod { get; private set; }

		public RefValueInfo(IComponentInitInfo componentRef) : this(componentRef, null) {
		}

		public RefValueInfo(IComponentInitInfo componentRef, string method) {
			ComponentRef = componentRef;
			ComponentMethod = method;
		}
		
		public object GetValue(IValueFactory factory, Type conversionType)
		{
			if (ComponentMethod!=null) {
				var targetInstance = factory.GetInstance(ComponentRef, typeof(object) );
				return factory.GetInstance( 
					new DelegateFactory(targetInstance, ComponentMethod).GetObject(),
					conversionType
				);
			} else {
				return factory.GetInstance(ComponentRef, conversionType);
			}
		}
	}
}
