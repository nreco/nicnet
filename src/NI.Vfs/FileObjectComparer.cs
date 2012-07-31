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
using System.ComponentModel;
using System.IO;

namespace NI.Vfs
{

	public class FileObjectComparer : IComparer {
		ListSortDirection _SortDirection = ListSortDirection.Ascending;
		FileObjectField _Field;
			
		public ListSortDirection SortDirection {
			get { return _SortDirection; }
			set { _SortDirection = value; }
		}

		public enum FileObjectField {
			Name, LastModifiedTime, Size, Extension
		}
			
		public FileObjectField Field {
			get { return _Field; }
			set { _Field = value; }
		}
			
		public FileObjectComparer(FileObjectField fld) {
			Field = fld;
		}

		public FileObjectComparer(FileObjectField fld, ListSortDirection sortDirection) {
			SortDirection = sortDirection;
			Field = fld;
		}
			
		public int Compare(object x, object y) {
			if (x is IFileObject && y is IFileObject) {
				IFileObject xFile = (IFileObject)x;
				IFileObject yFile = (IFileObject)y;
					
				if (Field==FileObjectField.Name) {
					return SortDirection==ListSortDirection.Ascending ?
						xFile.Name.CompareTo(yFile.Name) : yFile.Name.CompareTo(xFile.Name);
				}
					
				if (Field==FileObjectField.LastModifiedTime) {
					return SortDirection==ListSortDirection.Ascending ?
						xFile.GetContent().LastModifiedTime.CompareTo(yFile.GetContent().LastModifiedTime) :
						yFile.GetContent().LastModifiedTime.CompareTo(xFile.GetContent().LastModifiedTime);
				}

				if (Field==FileObjectField.Size) {
					return SortDirection==ListSortDirection.Ascending ?
						xFile.GetContent().Size.CompareTo(yFile.GetContent().Size) :
						yFile.GetContent().Size.CompareTo(xFile.GetContent().Size);
				}

				if (Field==FileObjectField.Extension) {
					return SortDirection==ListSortDirection.Ascending ?
						Path.GetExtension(xFile.Name).CompareTo( Path.GetExtension(yFile.Name) ) :
						Path.GetExtension(yFile.Name).CompareTo( Path.GetExtension(xFile.Name) );
				}
					
			}
			return 0;
		}


	}		


}
