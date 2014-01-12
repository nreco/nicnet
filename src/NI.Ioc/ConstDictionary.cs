using System;
using System.Collections;
using System.Text;

namespace NI.Ioc {
	
	/// <summary>
	/// Special IDictionary implementation used for immutable dictionaries (constructed through IoC container)
	/// </summary>
	[Serializable]
    internal class ConstDictionary : IDictionary, IEnumerable {
		object[] ArrKeys;
		object[] ArrValues;
		object[] orderedKeys = null;
		int[] orderedIndexes = null;
		bool isOrdered = false;
		bool _IsReadOnly = true;
		IDictionary UnderlyingDictionary = null;
		
		public ConstDictionary(object[] keys, object[] values) {
			ArrKeys = keys;
			ArrValues = values;
			if (ArrKeys.Length!=ArrValues.Length)
				throw new ArgumentException("Amount of keys and values should be identical");
			if (ArrKeys.Length>32) { // good enough suggestion?
				isOrdered = true;
				orderedKeys = new object[ArrKeys.Length];
				orderedIndexes = new int[ArrKeys.Length];
				for (int i=0; i<ArrKeys.Length; i++) {
					orderedKeys[i] = ArrKeys[i];
					orderedIndexes[i] = i;
				}
				Array.Sort(orderedKeys, orderedIndexes);
			}
		}
		public ConstDictionary(object[] keys, object[] values, bool isReadOnly) : this(keys,values) {
			_IsReadOnly = isReadOnly;
		}
		
		public ConstDictionary(ConstDictionary copyFrom) {
			ArrKeys = (object[])copyFrom.ArrKeys.Clone();
			ArrValues = (object[])copyFrom.ArrValues.Clone();
			isOrdered = copyFrom.isOrdered;
			if (isOrdered) {
				orderedKeys = (object[])copyFrom.orderedKeys.Clone();
				orderedIndexes = (int[])copyFrom.orderedIndexes.Clone();
			}
		}
		public ConstDictionary(ConstDictionary copyFrom, bool isReadOnly) : this(copyFrom) {
			_IsReadOnly = isReadOnly;
		}
		
		protected void EnsureUnderlyingDictionary() {
			if (!IsReadOnly && UnderlyingDictionary == null)
				UnderlyingDictionary = new Hashtable(this);
		}
		
		public void Add(object key, object value) {
			if (IsReadOnly)
				throw new NotSupportedException("ConstDictionary is readonly.");
			else {
				EnsureUnderlyingDictionary();
				UnderlyingDictionary.Add(key,value);
			}
		}

		public void Clear() {
			if (IsReadOnly)
				throw new NotSupportedException("ConstDictionary is readonly.");
			else {
				EnsureUnderlyingDictionary();
				UnderlyingDictionary.Clear();
			}
		}

		public bool Contains(object key) {
			if (UnderlyingDictionary!=null)
				return UnderlyingDictionary.Contains(key);
			return isOrdered ? Array.BinarySearch(orderedKeys, key)>=0 : Array.IndexOf(ArrKeys,key)>=0;
		}

		public IDictionaryEnumerator GetEnumerator() {
			if (UnderlyingDictionary != null)
				return UnderlyingDictionary.GetEnumerator();		
			return CreateDictionaryEnumerator();
		}
		
		protected IDictionaryEnumerator CreateDictionaryEnumerator() {
			return new ConstDictionaryEnumerator(this);
		}

		public bool IsFixedSize {
			get { return _IsReadOnly; }
		}

		public bool IsReadOnly {
			get { return _IsReadOnly; }
		}

		public ICollection Keys {
			get { 
				if (UnderlyingDictionary!=null)
					return UnderlyingDictionary.Keys;
				return (ICollection)ArrKeys.Clone(); 
			}
		}

		public void Remove(object key) {
			if (IsReadOnly)
				throw new Exception("The method or operation is not implemented.");
			else {
				EnsureUnderlyingDictionary();
				UnderlyingDictionary.Remove(key);
			}
		}

		public ICollection Values {
			get {
				if (UnderlyingDictionary != null)
					return UnderlyingDictionary.Values;				
				return (ICollection)ArrValues.Clone(); 
			}
		}

		public object this[object key] {
			get {
				if (UnderlyingDictionary!=null)
					return UnderlyingDictionary[key];
				int idx = isOrdered ? Array.BinarySearch(orderedKeys, key) : Array.IndexOf(ArrKeys, key);
				if (isOrdered && idx>=0) idx = orderedIndexes[idx];
				return idx>=0 ? ArrValues[idx] : null;
			}
			set {
				if (IsReadOnly) 
					throw new NotSupportedException("ConstDictionary is readonly.");
				else {
					EnsureUnderlyingDictionary();
					UnderlyingDictionary[key] = value;
				}
			}
		}

		public void CopyTo(Array array, int index) {
			if (UnderlyingDictionary!=null) {
				UnderlyingDictionary.CopyTo(array,index);
			} else
				throw new Exception("The method or operation is not implemented.");
		}

		public int Count {
			get { 
				if (UnderlyingDictionary!=null)
					return UnderlyingDictionary.Count;
				return ArrKeys.Length; 
			}
		}

		public bool IsSynchronized {
			get { 
				if (UnderlyingDictionary!=null)
					return UnderlyingDictionary.IsSynchronized;
				return true; 
			}
		}

		public object SyncRoot {
			get { 
				if (UnderlyingDictionary!=null)
					return UnderlyingDictionary.SyncRoot;
				return this; 
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			if (UnderlyingDictionary!=null)
				return UnderlyingDictionary.GetEnumerator();
			return CreateDictionaryEnumerator();
		}
		
		internal class ConstDictionaryEnumerator : IEnumerator, IDictionaryEnumerator {
			ConstDictionary dictionary;
			int pos = -1;
			
			internal ConstDictionaryEnumerator(ConstDictionary constDictionary) {
				dictionary = constDictionary;
			}
			
			public object Current {
				get { return Entry; }
			}

			public bool MoveNext() {
				if (pos<-1)
					return false;
				pos++;
				if (pos>=dictionary.ArrKeys.Length)
					pos = -2;
				return pos>=0;
			}

			public void Reset() {
				pos = -1;
			}

			public DictionaryEntry Entry {
				get {
					if (pos < 0) throw new InvalidOperationException("Current is not available");
					return new DictionaryEntry(dictionary.ArrKeys[pos],dictionary.ArrValues[pos]);
				}
			}

			public object Key {
				get { 
					if (pos<0) throw new InvalidOperationException("Current is not available");
					return dictionary.ArrKeys[pos];
				}
			}

			public object Value {
				get {
					if (pos < 0) throw new InvalidOperationException("Current is not available");
					return dictionary.ArrValues[pos];
				}
			}
		}
	}
}
