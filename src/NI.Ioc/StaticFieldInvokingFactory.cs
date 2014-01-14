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
using System.Reflection;



namespace NI.Ioc {

	/// <summary>
	/// StaticFieldInvokingFactory used for defining instance as static field of some class.
	/// </summary>
	/// <example><code>
	/// &lt;component name="datetimenow" type="NI.Ioc.StaticFieldInvokingFactory,NI.Ioc" singleton="false" lazy-init="true"&gt;
	///		&lt;property name="TargetType"&gt;&lt;type&gt;System.DBNull,Mscorlib&lt;/type&gt;&lt;/property&gt;
	///		&lt;property name="TargetField"&gt;&lt;value&gt;Value&lt;/value&gt;&lt;/property&gt;
	/// &lt;/component&gt;
	/// </code></example>
	public class StaticFieldInvokingFactory : IFactoryComponent {
		Type _TargetType;
		string _TargetField;
	
		/// <summary>
		/// Get or set target type
		/// </summary>
		public Type TargetType {
			get { return _TargetType; }
			set { _TargetType = value; }
		}
		
		/// <summary>
		/// Get or set static target property name
		/// </summary>
		public string TargetField {
			get { return _TargetField; }
			set { _TargetField = value; }
		}

		public StaticFieldInvokingFactory() {
		}
		
		public object GetObject() {
			System.Reflection.FieldInfo fInfo = TargetType.GetField(TargetField, BindingFlags.Static | BindingFlags.Public);
			if (fInfo == null)
				throw  new MissingFieldException(TargetType.ToString(), TargetField);
			return fInfo.GetValue(null);
		}
		
		public Type GetObjectType() {
			System.Reflection.FieldInfo fInfo = TargetType.GetField(TargetField, BindingFlags.Static | BindingFlags.Public);
			if (fInfo == null)
				throw new MissingFieldException(TargetType.ToString(), TargetField);
			return fInfo.FieldType;
		}		
		
		
	}
}
