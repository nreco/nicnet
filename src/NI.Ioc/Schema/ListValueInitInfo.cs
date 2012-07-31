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
using System.Collections;

namespace NI.Ioc
{
	/// <summary>
	/// Value Info for list
	/// </summary>
	public class ListValueInitInfo : IValueInitInfo
	{
		public IValueInitInfo[] Values;
		bool isOnlyConstValues = false;
		IDictionary cachedTypedArrays = null;
		
		public ListValueInitInfo(IValueInitInfo[] values)
		{
			Values = values;
			isOnlyConstValues = true;
			for (int i=0;i<values.Length;i++)
				if (!(values[i] is ValueInitInfo))
					isOnlyConstValues = false;
		}
		
		public object GetInstance(IValueFactory factory, Type conversionType) {
			// try to find in consts cache
			if (isOnlyConstValues && cachedTypedArrays != null &&
				conversionType.IsArray && cachedTypedArrays.Contains(conversionType.GetElementType())) {
				return ((Array)cachedTypedArrays[conversionType.GetElementType()]).Clone();
			}
			
			// try to create instance of desired type
			Type elemType = typeof(object);
			if (conversionType.IsArray)
				elemType = conversionType.GetElementType();
			Array listArray = Array.CreateInstance(elemType,Values.Length);
			
			for (int i=0; i<Values.Length; i++) {
				IValueInitInfo value = Values[i];
				listArray.SetValue( value.GetInstance( factory, elemType), i );
			}
			
			// store in consts cache
			if (isOnlyConstValues && conversionType.IsArray) {
				if (cachedTypedArrays==null) cachedTypedArrays = new Hashtable();
				cachedTypedArrays[elemType] = listArray.Clone();
			}
			if (conversionType.IsArray)
				return listArray; // nothing to convert
			return factory.CreateInstance(new ValueInitInfo(listArray), conversionType);
		}
		
	}
}
