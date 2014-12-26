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
	/// Describes class property value location
	/// </summary>
	public class ClassPropertyLocation {
		public Class Class { get; private set; }
		public Property Property { get; private set; }
		public PropertyValueLocationType Location { get; private set; }
		public string TableColumnName { get; private set; }

		protected ClassPropertyLocation(Class dataClass, Property p, PropertyValueLocationType location) {
			Class = dataClass;
			Property = p;
			Location = location;
		}

		/// <summary>
		/// Initializes new class property located in value table
		/// </summary>
		public ClassPropertyLocation(Class dataClass, Property p) : this(dataClass,p,PropertyValueLocationType.ValueTable) {
		}

		/// <summary>
		/// Initializes new class property located in object's table column
		/// </summary>
		public ClassPropertyLocation(Class dataClass, Property p, string columnName) : this (dataClass,p,PropertyValueLocationType.TableColumn) {
			if (String.IsNullOrEmpty(columnName))
				throw new ArgumentNullException("columnName");
			TableColumnName = columnName;
		}

		public override string ToString() {
			return String.Format("Class property {0}.{1} location={2}", Class.ID, Property.ID, Location);
		}
	}

	public enum PropertyValueLocationType {
		ValueTable,
		TableColumn,
		Derived
	}

	public class DerivedClassPropertyLocation : ClassPropertyLocation {

		public ClassPropertyLocation DerivedFrom { get; private set; }

		public string DeriveType { get; private set; }

		public DerivedClassPropertyLocation(Class dataClass, Property p, string deriveType, ClassPropertyLocation derivedFrom)
			: base(dataClass, p, PropertyValueLocationType.Derived) {
			if (derivedFrom.Class!=dataClass)
				throw new NotSupportedException("Property can be derived from property of the same class");
			DerivedFrom = derivedFrom;
			DeriveType = deriveType;
		}

		public override string ToString() {
			return String.Format("{0} deriveType={1}", base.ToString(), DeriveType);
		}

	}


}
