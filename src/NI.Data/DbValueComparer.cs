#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2014 NewtonIdeas
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NI.Data {
	
	public class DbValueComparer : IComparer {
		
		static IComparer _Instance = new DbValueComparer();

		public static IComparer Instance { 
			get {
				return _Instance;
			} 
		}

		public int Compare(object a, object b) {
			if (a == null && b == null)
				return 0;
			if (a == null && b != null)
				return -1;
			if (a != null && b == null)
				return 1;

			if ((a is IList) && (b is IList)) {
				IList aList = (IList)a;
				IList bList = (IList)b;
				if (aList.Count < bList.Count)
					return -1;
				if (aList.Count > bList.Count)
					return +1;
				for (int i = 0; i < aList.Count; i++) {
					int r = Compare(aList[i], bList[i]);
					if (r != 0)
						return r;
				}
				// lists are equal
				return 0;
			}
			if (a is IComparable) {

				// try to convert b to a type (because standard impl of IComparable for simple types are stupid enough)
				try {
					object bConverted = Convert.ChangeType(b, a.GetType());
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
					object aConverted = Convert.ChangeType(a, b.GetType());
					return -((IComparable)b).CompareTo(aConverted);
				} catch {
				}
			}

			throw new Exception("Cannot compare");
		}

	}
}
