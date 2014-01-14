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
using System.ComponentModel;
using System.Reflection;

namespace NI.Ioc
{
	/// <summary>
	/// Factory component which returns a value which is the result of instance method invocation.
	/// </summary>
	/// <example><code>
	/// &lt;component name="datetimenow-3days" type="NI.Ioc.MethodInvokingFactory,NI.Ioc" singleton="false" lazy-init="true"&gt;
	///		&lt;property name="TargetObject"&gt;&lt;ref name="dateTimeNow"/&gt;&lt;/property&gt;
	///		&lt;property name="TargetMethod"&gt;&lt;value&gt;AddDays&lt;/value&gt;&lt;/property&gt;
	///		&lt;property name="TargetMethodArgTypes"&gt;&lt;list&gt;&lt;entry&gt;&lt;type&gt;System.Double,Mscorlib&lt;/type&gt;&lt;/entry&gt;&lt;/list&gt;&lt;/property&gt;
	///		&lt;property name="TargetMethodArgs"&gt;&lt;list&gt;&lt;entry&gt;&lt;value&gt;-3&lt;/value&gt;&lt;/entry&gt;&lt;/list&gt;&lt;/property&gt;
	///	&lt;/component&gt; 
	/// </code></example>
	/// <inherit>NI.Ioc.BaseMethodInvokingFactory</inherit>
	public class MethodInvokingFactory : BaseMethodInvokingFactory, IFactoryComponent
	{
		object _TargetObject;
		string _TargetMethod;
		
		/// <summary>
		/// Get or set target object instance
		/// </summary>
		public object TargetObject {
			get { return _TargetObject; }
			set { _TargetObject = value; }
		}
		
		/// <summary>
		/// Get or set target method name to invoke
		/// </summary>
		public string TargetMethod {
			get { return _TargetMethod; }
			set { _TargetMethod = value; }
		}
		
		
		public MethodInvokingFactory()
		{
		}
		
		public object GetObject() {

			Type[] argTypes = ResolveMethodArgTypes();
			object[] argValues = PrepareMethodArgs(TargetMethodArgs, argTypes);
			
			MethodInfo mInfo = TargetObject.GetType().GetMethod(TargetMethod, argTypes);
			if (mInfo==null) throw new MissingMethodException( TargetObject.GetType().ToString(), TargetMethod);
			return mInfo.Invoke( TargetObject, argValues );
		}
		
		public Type GetObjectType() {
			MethodInfo mInfo = TargetObject.GetType().GetMethod(TargetMethod, ResolveMethodArgTypes());
			return mInfo.ReturnType;
		}
		
	}
}
