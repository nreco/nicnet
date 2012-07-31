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
using System.Text;
using System.Reflection;

namespace NI.Common {
	
	/// <summary>
	/// Operator helper contains static methods that performs 'operator' for any type in runtime.
	/// </summary>
	public static class OperatorHelper {
		
		public static object  Add(object op1, object op2) {
			return Add(op1.GetType(), op1, op2);
		}

		public static object Add(Type operandsType, object op1, object op2) {
			// try to perform 'add' operation for 'primitive' types
			if (operandsType==typeof(byte)) return ((byte)op1)+((byte)op2);
			if (operandsType == typeof(sbyte)) return ((sbyte)op1) + ((sbyte)op2);
			if (operandsType == typeof(sbyte)) return ((sbyte)op1) + ((sbyte)op2);
			if (operandsType == typeof(Int16)) return ((Int16)op1) + ((Int16)op2);
			if (operandsType == typeof(UInt16)) return ((UInt16)op1) + ((UInt16)op2);
			if (operandsType == typeof(Int32)) return ((Int32)op1) + ((Int32)op2);
			if (operandsType == typeof(Int64)) return ((Int64)op1) + ((Int64)op2);
			if (operandsType == typeof(UInt64)) return ((UInt64)op1) + ((UInt64)op2);
			if (operandsType == typeof(double)) return ((double)op1) + ((double)op2);
			if (operandsType == typeof(float)) return ((float)op1) + ((float)op2);
			
			MethodInfo opAdditionMethodInfo = operandsType.GetMethod("op_Addition", BindingFlags.Static | BindingFlags.Public);
			if (opAdditionMethodInfo == null) {
				throw new Exception(String.Format("Addition operation (+) is not defined for type {0}", operandsType.ToString()));
			}
			return opAdditionMethodInfo.Invoke(null, new object[] { op1, op2 });		
		}

		public static object Multiply(object op1, object op2) {
			return Multiply(op1.GetType(), op1, op2);
		}

		public static object Multiply(Type operandsType, object op1, object op2) {
			// try to perform 'add' operation for 'primitive' types
			if (operandsType == typeof(byte)) return ((byte)op1) * ((byte)op2);
			if (operandsType == typeof(sbyte)) return ((sbyte)op1) * ((sbyte)op2);
			if (operandsType == typeof(sbyte)) return ((sbyte)op1) * ((sbyte)op2);
			if (operandsType == typeof(Int16)) return ((Int16)op1) * ((Int16)op2);
			if (operandsType == typeof(UInt16)) return ((UInt16)op1) * ((UInt16)op2);
			if (operandsType == typeof(Int32)) return ((Int32)op1) * ((Int32)op2);
			if (operandsType == typeof(Int64)) return ((Int64)op1) * ((Int64)op2);
			if (operandsType == typeof(UInt64)) return ((UInt64)op1) * ((UInt64)op2);
			if (operandsType == typeof(double)) return ((double)op1) * ((double)op2);
			if (operandsType == typeof(float)) return ((float)op1) * ((float)op2);

			MethodInfo opMultiplyMethodInfo = operandsType.GetMethod("op_Multiply", BindingFlags.Static | BindingFlags.Public);
			if (opMultiplyMethodInfo == null) {
				throw new Exception(String.Format("Multiply operation (*) is not defined for type {0}", operandsType.ToString()));
			}
			return opMultiplyMethodInfo.Invoke(null, new object[] { op1, op2 });
		}		
		
		
	}
	
}
