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

namespace NI.Winter
{
	/// <summary>
	/// ReplacingFactory factory component
	/// </summary>
	/// <example><code>
	/// &lt;component name="testEnabled" type="NI.Winter.StringFormatFactory,NI.Winter" singleton="true"&gt;
	///		&lt;property name="Format"&gt;&lt;value&gt;{0}/some&lt;/value&gt;&lt;/property&gt;	
	///		&lt;property name="Params"&gt;&lt;list&gt;&lt;/list&gt;&lt;/property&gt;	
	///	&lt;/component&gt;
	/// </code></example>
	public class StringFormatFactory : IFactoryComponent
	{
		string _Format;
		object[] _Params;

		public string Format {
			get { return _Format; }
			set { _Format = value; }
		}
		public object[] Params {
			get { return _Params; }
			set { _Params = value; }
		}

		public StringFormatFactory()
		{
		}
		
		public object GetObject() {
			return String.Format(Format,Params);
		}
		
		public Type GetObjectType() {
			return typeof(string);
		}
		
	}
}
