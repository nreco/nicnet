#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013 Vitalii Fedorchenko
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
    /// Generic data class property
    /// </summary>
	public class Property {

		public string ID { get; set; }

		public long CompactID { get; set; }

		public string Name { get; set; }

		public PropertyDataType DataType { get; set; }

        public bool Multivalue { get; set; }

		public bool PrimaryKey { get; set; }

        public IEnumerable<Class> Classes {
			get {
				return Schema.FindPropertyClasses(ID);
			}
		}

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
