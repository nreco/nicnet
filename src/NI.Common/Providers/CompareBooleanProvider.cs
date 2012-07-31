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

namespace NI.Common.Providers {
	
	/// <summary>
	/// Comparision result provider (binary operation)
	/// </summary>
	public class CompareBooleanProvider : IBooleanProvider, IObjectProvider {

		public enum OperatorType {
			Equal = 1,
			LessThan = 2,
			GreaterThan = 4
		}

		IObjectProvider _LeftObjectProvider;
		IObjectProvider _RightObjectProvider;
		OperatorType _Operator;

		public OperatorType Operator {
			get { return _Operator; }
			set { _Operator = value; }
		}

		public IObjectProvider LeftObjectProvider {
			get { return _LeftObjectProvider; }
			set { _LeftObjectProvider = value; }
		}

		public IObjectProvider RightObjectProvider {
			get { return _RightObjectProvider; }
			set { _RightObjectProvider = value; }
		}

		public int Compare(object a, object b) {
			if (a==null && b==null)
				return 0;
			if (a==null && b!=null)
				return -1;
			if (a!=null && b==null)
				return 1;

			if ((a is IList) && (b is IList)) {
				IList aList = (IList)a;
				IList bList = (IList)b;
				if (aList.Count<bList.Count) return -1;
				if (aList.Count>bList.Count) return +1;
				for (int i=0; i<aList.Count; i++) {
					int r = Compare(aList[i], bList[i]);
					if (r!=0)
						return r;
				}
				// lists are equal
				return 0;
			}
			if (a is IComparable) {

				// try to convert b to a type (because standard impl of IComparable for simple types are stupid enough)
				try {
					object bConverted = Convert.ChangeType(b, a.GetType() );
					return ((IComparable)a).CompareTo(bConverted);
				} catch {
				}

				// try to compare without any conversions
				try {
					return ((IComparable)a).CompareTo(b);
				} catch { }
				
				
			}
			if (b is IComparable) {
				// try to compare without any conversions
				try {
					return -((IComparable)b).CompareTo(a);
				} catch { }

				// try to convert a to b type
				try {
					object aConverted = Convert.ChangeType(a, b.GetType() );
					return -((IComparable)b).CompareTo(aConverted);
				} catch {
				}
			}

			throw new Exception("Cannot compare");
		}

		public bool GetBoolean(object contextObj) {
			object leftObject = LeftObjectProvider.GetObject(contextObj);
			object rightObject = RightObjectProvider.GetObject(contextObj);

			int r = Compare(leftObject, rightObject);
			if (r<0 && (Operator & OperatorType.LessThan)==OperatorType.LessThan)
				return true;
			if (r==0 && (Operator & OperatorType.Equal)==OperatorType.Equal)
				return true;
			if (r>0 && (Operator & OperatorType.GreaterThan)==OperatorType.GreaterThan)
				return true;
			return false;
		}


		public object GetObject(object context) {
			return GetBoolean(context);
		}

	}

}
