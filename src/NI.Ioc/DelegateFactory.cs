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
using System.Reflection;

using NI.Common;

namespace NI.Ioc
{
	/// <summary>
	/// Creates delegate of target object's method.
	/// </summary>
	public class DelegateFactory : IFactoryComponent
	{
		/// <summary>
		/// Get or set target object instance
		/// </summary>
		public object TargetObject { get; set; }
		
		/// <summary>
		/// Get or set target method name to invoke
		/// </summary>
		public string TargetMethod { get; set; }

		public Type DelegateType { get; set; }

		public DelegateFactory() {
		}
		
		public object GetObject() {
			return Delegate.CreateDelegate(DelegateType, TargetObject, TargetMethod);
		}
		
		public Type GetObjectType() {
			return DelegateType;
		}
		
	}
}
