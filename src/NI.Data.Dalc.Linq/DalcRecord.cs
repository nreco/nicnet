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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace NI.Data.Dalc.Linq {
	
	/// <summary>
	/// Linq-friendly dalc record structure.
	/// </summary>
	[Serializable]
	public class DalcRecord : ICustomTypeDescriptor {
		public IDictionary Data;

		public DalcRecord(IDictionary data) {
			Data = data;
		}

		public DalcValue this[string fldName] {
			get {
				return new DalcValue(Data[fldName]);
			}
			set {
				Data[fldName] = value.Value;
			}
		}

		#region ICustomTypeDescriptor

		AttributeCollection ICustomTypeDescriptor.GetAttributes() {
			return new AttributeCollection(null);
		}

		string ICustomTypeDescriptor.GetClassName() {
			return null;
		}

		string ICustomTypeDescriptor.GetComponentName() {
			return null;
		}

		TypeConverter ICustomTypeDescriptor.GetConverter() {
			return null;
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
			return null;
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
			return null;
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
			return null;
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
			return new EventDescriptorCollection(null);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
			return new EventDescriptorCollection(null);
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
			var props = new List<PropertyDescriptor>();
			foreach (DictionaryEntry varEntry in Data)
				props.Add(new DalcRecordPropertyDescriptor(varEntry.Key.ToString()));
			return new PropertyDescriptorCollection(props.ToArray());
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
			return ((ICustomTypeDescriptor)this).GetProperties(null);
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
			return this;
		}

		#endregion
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
		public override string ToString() {
			if (Value != null)
				return Value.ToString();
			return base.ToString();
		}

		public int CompareTo(object obj) {
			if (Value is IComparable)
				return ((IComparable)Value).CompareTo(obj);
			if (Value == null && obj == null)
				return 0;
			throw new NotImplementedException();
		}

		public bool Like(string o) {
			// this is marker method for linq
			throw new NotImplementedException();
		}

		public bool In(IEnumerable o) {
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

	internal class DalcRecordPropertyDescriptor : PropertyDescriptor {
		string Name;

		public DalcRecordPropertyDescriptor(string name)
			: base(name, null) {
			Name = name;
		}

		public override bool CanResetValue(object component) {
			return true;
		}

		public override Type ComponentType {
			get { return typeof(DalcRecord); }
		}

		public override object GetValue(object component) {
			return ((DalcRecord)component)[Name].Value;
		}

		public override bool IsReadOnly {
			get { return false; }
		}

		public override Type PropertyType {
			get { return typeof(object); }
		}

		public override void ResetValue(object component) {
			((DalcRecord)component)[Name] = new DalcValue(null);
		}

		public override void SetValue(object component, object value) {
			((DalcRecord)component)[Name] = new DalcValue(value);
		}

		public override bool ShouldSerializeValue(object component) {
			return false;
		}
	}

}
