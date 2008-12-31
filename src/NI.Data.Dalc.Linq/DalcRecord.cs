using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NI.Data.Dalc.Linq {
	
	/// <summary>
	/// Linq-friendly dalc record structure.
	/// </summary>
	[Serializable]
	public class DalcRecord {
		IDictionary RecordData;

		public DalcRecord(IDictionary data) {
			RecordData = data;
		}

		public DalcValue this[string fldName] {
			get {
				return new DalcValue(this[fldName]);
			}
			set {
				RecordData[fldName] = value.Value;
			}
		}

	}

	/// <summary>
	/// Linq-friendly DALC value boxing structure.
	/// </summary>
	[Serializable]
	public struct DalcValue : IComparable {
		object _Value;

		public object Value {
			get { return _Value; }
			set { _Value = value; }
		}

		public DalcValue(object o) {
			_Value = o;
		}

		public override int GetHashCode() {
			return Value!=null ? Value.GetHashCode() : DBNull.Value.GetHashCode();
		}
		public override bool Equals(object obj) {
			return Value!=null ? Value.Equals(obj) : obj==null;
		}
		public int CompareTo(object obj) {
			if (Value is IComparable)
				return ((IComparable)Value).CompareTo(obj);
			if (Value == null && obj == null)
				return 0;
			throw new NotImplementedException();
		}

		public bool Like(object o) {
			// this is marker method for linq
			throw new NotImplementedException();
		}

		public static bool operator ==(DalcValue o1, object o2) {
			return o1.Equals(o2);
		}
		public static bool operator !=(DalcValue o1, object o2) {
			return !o2.Equals(o1); ;
		}
		
		static int Compare(object o1, object o2) {
			if (o1 is IComparable)
				return ((IComparable)o1).CompareTo(o2);
			if (o2 is IComparable)
				return -((IComparable)o2).CompareTo(o1);
			throw new NotSupportedException();
		}
		public static bool operator >(DalcValue o1, object o2) {
			return Compare(o1.Value, o2) > 0;
		}
		public static bool operator <(DalcValue o1, object o2) {
			return Compare(o1.Value, o2) < 0;
		}
		public static bool operator >=(DalcValue o1, object o2) {
			return Compare(o1.Value, o2) >= 0;
		}
		public static bool operator <=(DalcValue o1, object o2) {
			return Compare(o1.Value, o2) <= 0;
		}
		public static implicit operator DalcValue(string o) {
			return new DalcValue(o);
		}
		public static implicit operator DalcValue(int o) {
			return new DalcValue(o);
		}
		public static implicit operator DalcValue(DBNull o) {
			return new DalcValue(o);
		}
		public static implicit operator DalcValue(decimal o) {
			return new DalcValue(o);
		}
		public static implicit operator DalcValue(float o) {
			return new DalcValue(o);
		}
		public static implicit operator DalcValue(bool o) {
			return new DalcValue(o);
		}
		public static implicit operator DalcValue(byte o) {
			return new DalcValue(o);
		}
		public static implicit operator DalcValue(long o) {
			return new DalcValue(o);
		}
		public static implicit operator DalcValue(DateTime o) {
			return new DalcValue(o);
		}
	}

}
