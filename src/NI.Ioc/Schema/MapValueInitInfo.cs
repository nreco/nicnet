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

namespace NI.Ioc {

	/// <summary>
	/// Value Info for map
	/// </summary>
	public class MapValueInitInfo : IValueInitInfo {
		public MapEntryInfo[] Values;
		bool isOnlyConstValues = false;
		ConstDictionary cachedConstDictionary = null;
		
		public MapValueInitInfo(MapEntryInfo[] values) {
			Values = values;
			isOnlyConstValues = true;
			for (int i=0; i<Values.Length; i++)
				if (!(Values[i].Value is ValueInitInfo))
					isOnlyConstValues = false;
			
		}
		
		public object GetValue(IValueFactory factory, Type conversionType) {
			// try to find in cache
			if (isOnlyConstValues && cachedConstDictionary!=null) {
				return CastMap( new ConstDictionary(cachedConstDictionary,false), factory, conversionType);
			}
				
			// try to create instance of desired type
			string[] keys = new string[Values.Length];
			object[] values = new object[Values.Length];
			for (int i=0; i<Values.Length; i++) {
				keys[i] = Values[i].Key;
				values[i] = Values[i].Value.GetValue(factory, typeof(object) );
			}
			ConstDictionary result = new ConstDictionary(keys,values,false);
			// cache
			if (isOnlyConstValues)
				cachedConstDictionary = new ConstDictionary(result);
			return CastMap( result, factory, conversionType );
		}
		
		protected object CastMap(IDictionary map, IValueFactory factory, Type conversionType) {
			if (conversionType==typeof(Hashtable))
				return new Hashtable(map); // for compatibility
			if (conversionType==typeof(IDictionary))
				return map;
			// finally try to use value typecast mechanizm
			return factory.GetInstance( map, conversionType );
		}
		
	}
	
	
	/// <summary>
	/// </summary>
	public class MapEntryInfo {
		string _Key;
		IValueInitInfo _Value;
	
		public string Key { get { return _Key; } }
		
		public IValueInitInfo Value { get { return _Value; } }
	
		public MapEntryInfo(string key, IValueInitInfo value) {
			_Key = key;
			_Value = value;
		}
		
		
	}	
	
}
