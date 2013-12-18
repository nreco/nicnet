#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
 * Copyright 2008-2013 Vitalii Fedorchenko (changes and v.2)
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

namespace NI.Data {
	
	/// <summary>
	/// Represents query constant value
	/// </summary>
	[Serializable]
	public class QConst : IQueryValue {
		object _Value;
		TypeCode _Type;
		
		/// <summary>
		/// Get constant value object
		/// </summary>
		public virtual object Value {
			get { return _Value; }
		}
		
		/// <summary>
		/// Get TypeCode of constant value
		/// </summary>
		public TypeCode Type { 
			get {
				return _Type!=TypeCode.Empty ? _Type : Convert.GetTypeCode(Value);
			} 
		}
		
		/// <summary>
		/// Initializes a new instance of the QConst with specified value object
		/// </summary>
		/// <param name="value">object of constant value</param>
		public QConst(object value) {
			_Value = value;
			_Type = TypeCode.Empty;
		}

		/// <summary>
		/// Initializes a new instance of the QConst with specified value object and explicit type
		/// </summary>
		/// <param name="value">object of constant value</param>
		public QConst(object value, TypeCode type) {
			_Value = value;
			_Type = type;
		}
		
		public static explicit operator QConst(int value) {
			return new QConst(value);
		}

		public static explicit operator QConst(float value) {
			return new QConst(value);
		}

		public static explicit operator QConst(string value) {
			return new QConst(value);
		}

		public static explicit operator QConst(decimal value) {
			return new QConst(value);
		}

		public static explicit operator QConst(Array value) {
			return new QConst(value);
		}

		public static explicit operator QConst(double value) {
			return new QConst(value);
		}
		
		public static explicit operator QConst(DateTime value) {
			return new QConst(value);
		}

		public static explicit operator QConst(bool value) {
			return new QConst(value);
		}

	}
}
