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
using System.Reflection;
using System.Text;

using NI.Common.Providers;

namespace NI.Common.Caching
{
	/// <summary>
	/// Unique cache key provider.
	/// </summary>
	public class UniqueCacheKeyProvider : IStringProvider {
	
		public UniqueCacheKeyProvider() {
			
		}
		
		public virtual string GetString(object obj) {
			if (obj==null)
				return String.Empty;
			
			// find appropriate method
			System.Reflection.MethodInfo methodInfo = this.GetType().GetMethod(
				"GenerateString",
				System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic,
				null, 
				new Type[] { obj.GetType() }, null);
			if (methodInfo!=null)
				return Convert.ToString( methodInfo.Invoke(this, new object[] {obj} ) );
			
			return obj.ToString();
		}
		
		protected string GenerateString(object obj) {
			return obj.ToString();
		}

		protected string GenerateString(IList obj) {
			StringBuilder resultStr = new StringBuilder();
			for (int i=0; i<obj.Count; i++) {
				object v = obj[i];
				if (v!=null) {
					string str = GetString(v);
					resultStr.Append(i.ToString()+"#"+str.Length+"#");
					resultStr.Append(str);
				} else {
					resultStr.Append(i.ToString()+"#0#null");
				}
			}
			return resultStr.ToString();
		}
		
		protected string GenerateString(IDictionary dictionary) {
			object[] keys = new object[dictionary.Keys.Count];
			object[] values = new object[dictionary.Values.Count];
			dictionary.Keys.CopyTo(keys, 0);
			dictionary.Values.CopyTo(values, 0);
			string[] strKeys = new string[keys.Length];
			for (int i=0; i<keys.Length; i++) {
				strKeys[i] = GetString( keys[i] );
			}
			Array.Sort(strKeys, values);
			
			StringBuilder resultStr = new StringBuilder();
			for (int i=0; i<strKeys.Length; i++) {
				resultStr.Append( strKeys[i].Length );
				resultStr.Append( strKeys[i] );
				string strValue = GetString( values[i] ); 
				resultStr.Append( strValue.Length );
				resultStr.Append( strValue );
			}
			return resultStr.ToString();
		}
		
		
	}
}
