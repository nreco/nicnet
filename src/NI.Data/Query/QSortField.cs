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
	public class QSortField : IQueryValue
	{
		public const string Asc = "asc";
		public const string Desc = "desc";

		public string Name { get; private set; }
		public ListSortDirection SortDirection { get; private set; }
				
		public QSortField(string sortFld) {
			SortDirection = ListSortDirection.Ascending;

			sortFld = sortFld.Trim();
			int lastSpaceIdx = sortFld.LastIndexOf(' ');
			string lastWord = lastSpaceIdx != -1 ? sortFld.Substring(lastSpaceIdx + 1).ToLower() : null;
			bool sortDirectionFound = true;

			if (lastWord == Asc || lastWord == "ascending")
				SortDirection = ListSortDirection.Ascending;
			else if (lastWord == Desc || lastWord == "descending")
				SortDirection = ListSortDirection.Descending;
			else
				sortDirectionFound = false;

			Name = sortDirectionFound ? sortFld.Substring(0, lastSpaceIdx).TrimEnd() : sortFld;
			if (Name == String.Empty)
				throw new ArgumentException("Invalid sort field");
		}
		public QSortField(string sortFldName, ListSortDirection direction) {
			Name = sortFldName;
			SortDirection = direction;
		}
		
		public static explicit operator QSortField(string sortFld) {
			return new QSortField(sortFld);
		}
		
		public override string ToString() {
			return String.Format("{0} {1}", Name, SortDirection==ListSortDirection.Ascending ? Asc : Desc );
		}
		
	}
}
