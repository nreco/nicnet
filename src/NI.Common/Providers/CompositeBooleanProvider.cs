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
	/// Composite boolean provider (supports 'and' and 'or' composition types)
	/// </summary>
	public class CompositeBooleanProvider : IBooleanProvider, IObjectProvider {
		
		public enum GroupType { And, Or };
		
		IBooleanProvider[] _Conditions;
		GroupType _Group = GroupType.And;

		public IBooleanProvider[] Conditions {
			get { return _Conditions; }
			set { _Conditions = value; }
		}

		public GroupType Group {
			get { return _Group; }
			set { _Group = value; }
		}


		public bool GetBoolean(object contextObj) {
			if (Conditions.Length==0)
				return true;

			bool result = Conditions[0].GetBoolean(contextObj);
			for (int i=1; i<Conditions.Length; i++) {
				// optimizations
				// and:
				if (Group==GroupType.And)
					if (!result) return result;
				// or:
				if (Group==GroupType.Or)
					if (result) return result;
				
				if (Group==GroupType.And)
					result = result && Conditions[i].GetBoolean(contextObj);
				else
					result = result || Conditions[i].GetBoolean(contextObj);
			}
			return result;
		}

		public object GetObject(object context) {
			return GetBoolean(context);
		}

	}
}
