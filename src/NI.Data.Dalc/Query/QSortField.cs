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
	/// QSortField
	/// </summary>
	public class QSortField : IQueryFieldValue
	{
		static Regex SortFieldRegex = new Regex(@"\s*(?<fldName>[^\s]*)\s*(?<order>asc|desc|ascending|descending){0,1}", RegexOptions.IgnoreCase);
		
		public const string Asc = "asc";
		public const string Desc = "desc";
	
		string _Name;
		ListSortDirection _SortDirection = ListSortDirection.Ascending;
		
		public string Name {
			get { return _Name; }
			set { _Name = value; }
		}
		
		public ListSortDirection SortDirection {
			get { return _SortDirection; }
			set { _SortDirection = value; }
		}
				
		public QSortField(string sortFld) {
			Match match = SortFieldRegex.Match(sortFld);
			if (match.Success) {
				_Name = match.Groups["fldName"].Value;
				if (match.Groups["order"].Value.ToLower().StartsWith(Asc))
					_SortDirection = ListSortDirection.Ascending;
				if (match.Groups["order"].Value.ToLower().StartsWith(Desc))
					_SortDirection = ListSortDirection.Descending;
			} else
				throw new ArgumentException("Invalid sort field");
		}
		
		public static explicit operator QSortField(string sortFld) {
			return new QSortField(sortFld);
		}
		
		public override string ToString() {
			return String.Format("{0} {1}", Name, SortDirection==ListSortDirection.Ascending ? Asc : Desc );
		}
		
		
	}
}
