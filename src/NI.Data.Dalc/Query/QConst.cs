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

namespace NI.Data.Dalc {
	
	/// <summary>
	/// IQueryConstantValue implementation
	/// </summary>
	[Serializable]
	public struct QConst : IQueryConstantValue {
		object _Value;
		TypeCode _Type;
		
		public object Value {
			get { return _Value; } 
		}
	
		public TypeCode Type { 
			get {
				return _Type!=TypeCode.Empty ? _Type : Convert.GetTypeCode(Value);
			} 
		}
	
		public QConst(object value) {
			_Value = value;
			_Type = TypeCode.Empty;
		}
		
		public QConst(object value, TypeCode type) {
			_Value = value;
			_Type = type;
		}
	
		public static explicit operator QConst(int value) {
			return new QConst(value);
		}

		public static explicit operator QConst(string value) {
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