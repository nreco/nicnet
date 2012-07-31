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
using System.Reflection;

using NI.Common;

namespace NI.Ioc
{
	/// <summary>
	/// StaticMethodInvokingFactory used for defining instance as result of invoking static method
	/// </summary>
	/// <example><code>
	/// &lt;component name="falseInstance" type="NI.Ioc.StaticMethodInvokingFactory,NI.Ioc" singleton="false" lazy-init="true"&gt;
	///		&lt;property name="TargetType"&gt;&lt;type&gt;System.Boolean,Mscorlib&lt;/type&gt;&lt;/property&gt;
	///		&lt;property name="TargetMethod"&gt;&lt;value&gt;Parse&lt;/value&gt;&lt;/property&gt;
	///		&lt;property name="TargetMethodArgTypes"&gt;
	///			&lt;list&gt;
	///				&lt;entry&gt;&lt;type&gt;System.String,Mscorlib&lt;/type&gt;&lt;/entry&gt;		
	///			&lt;/list&gt;
	///		&lt;/property&gt;
	///		&lt;property name="TargetMethodArgs"&gt;
	///			&lt;list&gt;
	///				&lt;entry&gt;&lt;value&gt;False&lt;/value&gt;&lt;/entry&gt;		  
	///			&lt;/list&gt;
	///		&lt;/property&gt;
	///	&lt;/component&gt;
	/// </code></example>
	public class StaticMethodInvokingFactory : BaseMethodInvokingFactory, IFactoryComponent
	{
		Type _TargetType;
		string _TargetMethod;
	
		
		/// <summary>
		/// Get or set target type
		/// </summary>
		public Type TargetType {
			get { return _TargetType; }
			set { _TargetType = value; }
		}
		
		/// <summary>
		/// Get or set static target method name
		/// </summary>
		public string TargetMethod {
			get { return _TargetMethod; }
			set { _TargetMethod = value; }
		}
		

		
		
		public StaticMethodInvokingFactory()
		{
		}
		
		public object GetObject() {

			Type[] argTypes = ResolveMethodArgTypes();
			object[] argValues = PrepareMethodArgs(TargetMethodArgs, argTypes);
			
			MethodInfo mInfo = TargetType.GetMethod(TargetMethod, BindingFlags.Static|BindingFlags.Public, null, argTypes, null);
			if (mInfo==null)
				throw new MissingMethodException( TargetType.ToString(), TargetMethod);
			return mInfo.Invoke( null, BindingFlags.Static|BindingFlags.Public, null, argValues, null );
		}
		
		public Type GetObjectType() {
			MethodInfo mInfo = TargetType.GetMethod(TargetMethod, BindingFlags.Static | BindingFlags.Public, null, ResolveMethodArgTypes(), null);
			return mInfo.ReturnType;
		}		
		
		
	}
}
