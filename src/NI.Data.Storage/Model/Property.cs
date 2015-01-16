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
    /// Describes storage object property
    /// </summary>
	public class Property {

		/// <summary>
		/// Property unique string identifier
		/// </summary>
		public string ID { get; set; }

		/// <summary>
		/// Internal property identifier represented by integer value
		/// </summary>
		public long CompactID { get; set; }

		/// <summary>
		/// Property human-readable name (label)
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Property data type descriptor
		/// </summary>
		public PropertyDataType DataType { get; set; }

		/// <summary>
		/// Determines if property is multivalue
		/// </summary>
        public bool Multivalue { get; set; }

		/// <summary>
		/// Determines if property represents object primary key
		/// </summary>
		public bool PrimaryKey { get; set; }

		/// <summary>
		/// List of classes with this property
		/// </summary>
        public IEnumerable<Class> Classes {
			get {
				return Schema.FindPropertyClasses(ID);
			}
		}

		/// <summary>
		/// Property schema instance
		/// </summary>
		public DataSchema Schema { get; internal set; }

		public Property() {
			PrimaryKey = false;
		}

        public Property(string id) {
            ID = id;
		}

		public ClassPropertyLocation GetLocation(Class dataClass) {
			return Schema.FindClassPropertyLocation(dataClass.ID, ID);
		}

		public override bool Equals(object obj) {
			if (obj is Property) {
				var p = (Property)obj;
				if (p.ID != null && this.ID != null)
					return this.ID == p.ID;
			}
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return String.IsNullOrEmpty(ID) ? base.GetHashCode() : ID.GetHashCode();
		}

		public static bool operator ==(Property a, Property b) {
			if ((object)a == null && (object)b == null) return true;
			if ((object)a != null && (object)b != null)
				return a.ID == b.ID;
			return false;
		}
		public static bool operator !=(Property a, Property b) {
			return !(a == b);
		}

	}


}
