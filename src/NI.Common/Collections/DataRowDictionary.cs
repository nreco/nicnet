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
using System.Data;
using System.Collections;

namespace NI.Common.Collections
{
	/// <summary>
	/// Dictionary proxy for DataRow.
	/// </summary>
	public class DataRowDictionary : IDictionary, ICollection
	{
		DataRow _Row;
		
		/// <summary>
		/// Get or set data row
		/// </summary>
		[Dependency]
		public DataRow Row {
			get { return _Row; }
			set { _Row = value; }
		}
	
		public DataRowDictionary() { }
	
		public DataRowDictionary(DataRow row) {
			Row = row;
		}
		
		
		public bool IsFixedSize { get { return true; } }

		public bool IsReadOnly { get { return true; } }

		public object this[object key] {
			get {
				string fieldName = key as string;
				if (key==null || !Row.Table.Columns.Contains(fieldName))
					return null;
				return Row[fieldName];
			}
			set {
				throw new NotSupportedException("DataRowDictionary is readonly");
			}
		}

		public ICollection Keys {
			get {
				string[] fieldNames = new string[Row.Table.Columns.Count];
				for (int i=0; i<fieldNames.Length; i++)
					fieldNames[i] = Row.Table.Columns[i].ColumnName;
				return fieldNames;
			}
		}

		public ICollection Values {
			get {
				return Row.ItemArray;
			}
		}

		public void Add (object key, object value) {
			throw new NotSupportedException("DataRowDictionary is readonly");
		}

		public void Clear () {
			throw new NotSupportedException("DataRowDictionary is readonly");
		}

		public bool Contains (object key) {
			string fieldName = key as string;
			if (fieldName==null) return false;
			return Row.Table.Columns.Contains(fieldName);
		}

		public IDictionaryEnumerator GetEnumerator () {
			return new Enumerator(Row);
		}
		
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public void Remove (object key) {
			throw new NotSupportedException("DataRowDictionary is readonly");
		}
			
		public bool IsSynchronized {
			get { return false; }
		}

		public int Count {
			get { return Row.Table.Columns.Count; }
		}

		public void CopyTo(Array array, int index) {
			// what to do ???
		}

		public object SyncRoot {
			get { return null; }
		}
		
		private sealed class Enumerator : IDictionaryEnumerator {

			private DataRow row;
			private int pos;
			private int size;

			private string currentKey;
			private Object currentValue;

			public Enumerator(DataRow row) {
				this.row = row;
				Reset();
			}

			public void Reset () {
				pos = -1;
				size = row.Table.Columns.Count;
				currentKey = null;
				currentValue = null;
			}

			public bool MoveNext () {
				if (pos < (size-1) ) {
					pos++;
					currentKey = row.Table.Columns[pos].ColumnName;
					currentValue = row[currentKey];
					return true;
				}
				currentKey = null;
				currentValue = null;
				return false;
			}

			public DictionaryEntry Entry {
				get {
					if (currentKey == null) throw new InvalidOperationException ();
					return new DictionaryEntry (currentKey, currentValue);
				}
			}

			public Object Key {
				get {
					if (currentKey == null) throw new InvalidOperationException ();
					return currentKey;
				}
			}

			public Object Value {
				get {
					if (currentKey == null) throw new InvalidOperationException ();
					return currentValue;
				}
			}

			public Object Current {
				get {
					if (currentKey == null) throw new InvalidOperationException ();
					return new DictionaryEntry (currentKey, currentValue);
				}
			}
		} 		
		

		

	}
}
