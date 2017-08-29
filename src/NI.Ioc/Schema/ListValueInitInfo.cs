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
using System.Collections;
using System.Collections.Generic;

namespace NI.Ioc
{
	/// <summary>
	/// List value initialization info
	/// </summary>
	public class ListValueInitInfo : IValueInitInfo
	{
		public IValueInitInfo[] Values;
		bool isOnlyConstValues = false;
		IDictionary<Type,Array> cachedTypedArrays = null;
		private object cachedTypedArraysSyncObject = new object();
		
		public ListValueInitInfo(IValueInitInfo[] values)
		{
			Values = values;
			isOnlyConstValues = true;
			for (int i=0;i<values.Length;i++)
				if (!(values[i] is ValueInitInfo))
					isOnlyConstValues = false;
		}
		
		public object GetValue(IValueFactory factory, Type conversionType) {
			lock (cachedTypedArraysSyncObject)
			{
				// try to find in consts cache
				if (isOnlyConstValues && cachedTypedArrays != null &&
					conversionType.IsArray && cachedTypedArrays.ContainsKey(conversionType.GetElementType())) {
					return cachedTypedArrays[conversionType.GetElementType()].Clone();
				}
			
				// try to create instance of desired type
				Type elemType = typeof(object);
				if (conversionType.IsArray)
					elemType = conversionType.GetElementType();
				Array listArray = Array.CreateInstance(elemType,Values.Length);
			
				for (int i=0; i<Values.Length; i++) {
					IValueInitInfo value = Values[i];
					listArray.SetValue( value.GetValue( factory, elemType), i );
				}
			
				// store in consts cache
				if (isOnlyConstValues && conversionType.IsArray) {
					if (cachedTypedArrays==null) cachedTypedArrays = new Dictionary<Type,Array>();
					cachedTypedArrays[elemType] = (Array)listArray.Clone();
				}
				if (conversionType.IsArray)
					return listArray; // nothing to convert
				return factory.GetInstance(listArray, conversionType);
			}
		}
		
	}
}
