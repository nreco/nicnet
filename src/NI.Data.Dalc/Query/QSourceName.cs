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
using System.Text.RegularExpressions;

namespace NI.Data.Dalc
{
	/// <summary>
	/// Query source name structure
	/// </summary>
	public struct QSourceName
	{
		string _Name;
		string _Alias;

		public string Name {
			get { return _Name; }
			set { _Name = value; }
		}

		public string Alias {
			get { return _Alias; }
			set { _Alias = value; }
		}

		public QSourceName(string sourceName) {
			int dotIdx = sourceName.LastIndexOf('.'); // allow dot in table name (alias for this case is required), like dbo.users.u
			if (dotIdx >= 0) {
				_Name = sourceName.Substring(0, dotIdx);
				_Alias = sourceName.Substring(dotIdx+1);
			}
			else {
				_Name = sourceName;
				_Alias = null;
			}

		}

		public static explicit operator QSourceName(string sourcename) {
			return new QSourceName(sourcename);
		}
		
		public override string ToString() {
			return String.IsNullOrEmpty(Alias) ? Name : Name+"."+Alias;
		}
		
		
	}
}
