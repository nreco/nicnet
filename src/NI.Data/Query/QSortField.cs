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
using System.ComponentModel;

namespace NI.Data
{
	/// <summary>
	/// QSortField
	/// </summary>
	public class QSortField : IQueryFieldValue
	{
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
			sortFld = sortFld.Trim();
			int lastSpaceIdx = sortFld.LastIndexOf(' ');
			string lastWord = lastSpaceIdx != -1 ? sortFld.Substring(lastSpaceIdx + 1).ToLower() : null;
			bool sortDirectionFound = true;

			if (lastWord == Asc || lastWord == "ascending")
				_SortDirection = ListSortDirection.Ascending;
			else if (lastWord == Desc || lastWord == "descending")
				_SortDirection = ListSortDirection.Descending;
			else
				sortDirectionFound = false;

			_Name = sortDirectionFound ? sortFld.Substring(0, lastSpaceIdx).TrimEnd() : sortFld;
			if (_Name == String.Empty)
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
