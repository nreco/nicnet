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
using System.Collections;

using NI.Common;

namespace NI.Common.Collections
{
	/// <summary>
	/// Dictionary implementation composed from one or more
	/// underlying IDictionary instances.
	/// </summary>
    [Serializable]
	public class CompositeDictionary : IDictionary
	{
		public enum SetSatteliteBehaviourType {
			Ignore, OnlyOverwrite, AlwaysSet
		}
		
		IDictionary _MasterDictionary;
		IDictionary[] _SatelliteDictionaries;
		SetSatteliteBehaviourType _SetSatelliteBehaviour = SetSatteliteBehaviourType.Ignore;
		
		/// <summary>
		/// Get or set flag that specifies set operation behaviour for satellite dictionaries
		/// </summary>
		[Dependency(Required=false)]
		public SetSatteliteBehaviourType SetSatelliteBehaviour {
			get { return _SetSatelliteBehaviour; }
			set { _SetSatelliteBehaviour = value; }
		}
		
		/// <summary>
		/// Get or set master dictionary instance
		/// </summary>
		[Dependency]
		public IDictionary MasterDictionary {
			get { return _MasterDictionary; }
			set { _MasterDictionary = value; }
		}
		
		public IDictionary[] SatelliteDictionaries {
			get { return _SatelliteDictionaries; }
			set { _SatelliteDictionaries = value; }
		}
		
		public CompositeDictionary()
		{
		}
		
		public bool IsFixedSize { get { return MasterDictionary.IsFixedSize; } }

		public bool IsReadOnly { get { return MasterDictionary.IsReadOnly; } }

		public object this[object key] {
			get {
				// try to find in sattelities
				if (SatelliteDictionaries!=null)
					for (int i=0; i<SatelliteDictionaries.Length; i++)
						if (SatelliteDictionaries[i].Contains(key))
							return SatelliteDictionaries[i][key];
				
				return MasterDictionary[key];
			}
			set {
				MasterDictionary[key] = value;
				if (SatelliteDictionaries!=null && SetSatelliteBehaviour!=SetSatteliteBehaviourType.Ignore)
					for (int i=0; i<SatelliteDictionaries.Length; i++)
						if (SetSatelliteBehaviour==SetSatteliteBehaviourType.AlwaysSet || 
							SatelliteDictionaries[i].Contains(key))
							SatelliteDictionaries[i][key] = value;
			}
		}

		public ICollection Keys {
			get {
				ArrayList keys = new ArrayList( MasterDictionary.Keys);
				if (SatelliteDictionaries!=null)
					for (int i=0; i<SatelliteDictionaries.Length; i++)
						foreach (object key in SatelliteDictionaries[i].Keys)
							if (!keys.Contains(key))
								keys.Add(key);
				return keys;
			}
		}

		public ICollection Values {
			get {
				ArrayList values = new ArrayList(MasterDictionary.Values.Count);
				foreach (object key in Keys)
					values.Add( this[key] );
				return values;
			}
		}

		public void Add (object key, object value) {
			this[key] = value;
		}

		public void Clear () {
			MasterDictionary.Clear();
		}

		public bool Contains (object key) {
			if (SatelliteDictionaries!=null)
				for (int i=0; i<SatelliteDictionaries.Length; i++)
					if (SatelliteDictionaries[i].Contains(key))
						return true;
			return MasterDictionary.Contains(key);
		}

		public IDictionaryEnumerator GetEnumerator () {
			return new Enumerator(this);
		}
		
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public void Remove (object key) {
			MasterDictionary.Remove(key);
		}
			
		public bool IsSynchronized {
			get { return MasterDictionary.IsSynchronized; }
		}

		public int Count {
			get { return Keys.Count; }
		}

		public void CopyTo(Array array, int index) {
			// what to do ???
		}

		public object SyncRoot {
			get { return MasterDictionary.SyncRoot; }
		}
		
		private sealed class Enumerator : IDictionaryEnumerator {

			private IDictionary Dictionary;
			private IList Keys;
			private int pos;
			private int size;

			private object currentKey;
			private object currentValue;

			public Enumerator(IDictionary dictionary) {
				this.Dictionary = dictionary;
				Keys = dictionary.Keys as IList;
				Reset();
			}

			public void Reset () {
				pos = -1;
				size = Keys.Count;
				currentKey = null;
				currentValue = null;
			}

			public bool MoveNext () {
				if (pos < (size-1) ) {
					pos++;
					currentKey = Keys[pos];
					currentValue = Dictionary[currentKey];
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
