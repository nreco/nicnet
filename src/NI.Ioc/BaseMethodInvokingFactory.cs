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

	public abstract class BaseMethodInvokingFactory
	{
		/// <summary>
		/// Get or set target method args values
		/// </summary>
		public object[] TargetMethodArgs { get; set; }
		
		/// <summary>
		/// Get or set target method args types
		/// </summary>
		public Type[] TargetMethodArgTypes { get; set; }		
			
		public BaseMethodInvokingFactory()
		{
		}

		protected Type[] ResolveMethodArgTypes() {
			if (TargetMethodArgTypes != null)
				return TargetMethodArgTypes;
			int argsCount = TargetMethodArgs != null ? TargetMethodArgs.Length : 0;
			Type[] argTypes = new Type[argsCount];
			for (int i = 0; i < argTypes.Length; i++)
				argTypes[i] = TargetMethodArgs[i] != null ? TargetMethodArgs[i].GetType() : typeof(object);
			return argTypes;
		}

		protected object[] PrepareMethodArgs(object[] args, Type[] argTypes) {
			object[] argValues = args!=null ? new object[args.Length] : new object[0];
			for (int i=0; i<argValues.Length; i++)
				argValues[i] = argTypes[i].IsInstanceOfType(args[i]) ? args[i] : Convert.ChangeType(args[i], argTypes[i]);
			return argValues;
		}
		
	}
}
