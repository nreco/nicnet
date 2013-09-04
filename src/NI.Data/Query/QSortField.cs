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
using System.Linq;
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

		/// <summary>
		/// Sort field
		/// </summary>
		public QField Field { get; private set; }
		
		/// <summary>
		/// Sort direction
		/// </summary>
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

			Field = new QField( sortDirectionFound ? sortFld.Substring(0, lastSpaceIdx).TrimEnd() : sortFld );
			if (Field.Name == String.Empty)
				throw new ArgumentException("Invalid sort field");
		}
		public QSortField(string sortFldName, ListSortDirection direction) {
			Field = (QField)sortFldName;
			SortDirection = direction;
		}
		
		public override string ToString() {
			return String.Format("{0} {1}", Field.ToString(), SortDirection==ListSortDirection.Ascending ? Asc : Desc );
		}

		public static implicit operator QSortField(string value) {
			return new QSortField(value);
		}
		public static implicit operator string(QSortField value) {
			return value.ToString();
		}

	}
}
