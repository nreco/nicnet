#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013-2014 Vitalii Fedorchenko
 * Copyright 2014 NewtonIdeas
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NI.Data.Storage.Model {
	
	/// <summary>
	/// Represents data object that may be persisted in the <see cref="IObjectContainerStorage"/>
	/// </summary>
	public class ObjectContainer : IEnumerable<KeyValuePair<Property,object>> {
		public long? ID { get; set; }
		
		Class ObjectClass;

		IDictionary<Property, object> Values;

		public ObjectContainer(Class objClass, long? id = null) {
			Values = new Dictionary<Property,object>();
			ObjectClass = objClass;

			ID = id;
		}

		public object this[Property p] {
			get {
				if (p.PrimaryKey)
					return ID.HasValue ? (object)ID.Value : null;
				return Values.ContainsKey(p) ? Values[p] : null;
			}
			set {
				if (!ObjectClass.HasProperty(p))
					throw new ArgumentException( String.Format("Invalid property (PID={0}) for class (CID={1})", p.ID, ObjectClass.ID) );
				if (p.PrimaryKey) {
					ID = value==null? (long?)null : (long?)Convert.ToInt64(value);
				} else {
					Values[p] = value==null || DBNull.Value.Equals(value) ? null : p.DataType.ConvertToValueType( value );
				}
			}
		}

		public object this[string propId] {
			get { 
				var p = ObjectClass.Schema.FindPropertyByID(propId);
				if (p == null)
					throw new ArgumentException(String.Format("Property with ID={0} doesn't exist", propId));
				return this[p];
			}
			set {
				var p = ObjectClass.Schema.FindPropertyByID(propId);
				if (p == null)
					throw new ArgumentException(String.Format("Property with ID={0} doesn't exist", propId));
				this[p] = value;
			}
		}

		public Class GetClass() {
			return ObjectClass;
		}

		public override string ToString() {
			var c = GetClass();
			return String.Format( "{0}({1}):{2}", c.Name, c.ID, ID.HasValue ? ID.Value.ToString() : "new"+base.GetHashCode().ToString() );
		}

		public override int GetHashCode() {
			if (ID.HasValue)
				return ToString().GetHashCode();
			return base.GetHashCode();
		}

		public override bool Equals(object obj) {
			if (obj is ObjectContainer) {
				var objCont = (ObjectContainer)obj;
				if (objCont.GetClass() == GetClass() && objCont.ID.HasValue && ID.HasValue && objCont.ID.Value == ID.Value )
					return true;
			}
			return base.Equals(obj);
		}

		public IEnumerator<KeyValuePair<Property, object>> GetEnumerator() {
			return Values.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return ((System.Collections.IEnumerable)Values).GetEnumerator();
		}
	}

}
