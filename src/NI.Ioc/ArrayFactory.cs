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
	/// ArrayFactory used for defining typed arrays inside Winter configuration.
	/// </summary>
	/// <example><code>
	/// &lt;component name="intArray" type="NI.Ioc.ArrayFactory,NI.Ioc" singleton="true"&gt;
	///		&lt;property name="ElementType"&gt;&lt;type&gt;System.Int32,mscorlib&lt;/type&gt;&lt;/property&gt;
	///		&lt;property name="Elements"&gt;&lt;list&gt;&lt;entry&gt;1&lt;/entry&gt;&lt;/list&gt;&lt;/property&gt;					
	///	&lt;/component&gt;
	/// </code></example>
	/// </example>
	public class ArrayFactory : IFactoryComponent
	{
		Type _ElementType;
		IEnumerable _Elements;
		
		/// <summary>
		/// Get or set array element type
		/// </summary>
		public Type ElementType {
			get { return _ElementType; }
			set { _ElementType = value; }
		}
		
		/// <summary>
		/// Get or set enumerations of array
		/// </summary>
		public IEnumerable Elements {
			get { return _Elements; }
			set { _Elements = value; }
		}
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		public ArrayFactory()
		{
		}

		public object GetObject() {
			ArrayList list = new ArrayList();
			foreach (object o in Elements)
				if (!(o is IConvertible))
					list.Add( o );
				else
					list.Add( Convert.ChangeType(o, ElementType) );
			return list.ToArray(ElementType);
		}

		public Type GetObjectType() {
			return Array.CreateInstance(ElementType, 0).GetType();
		}

	}
}
