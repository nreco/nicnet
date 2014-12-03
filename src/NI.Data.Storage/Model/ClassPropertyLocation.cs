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
	/// Describes class property 
	/// </summary>
	public class ClassPropertyLocation {
		public Class Class { get; private set; }
		public Property Property { get; private set; }
		public PropertyValueLocationMode Location { get; private set; }
		public string ColumnName { get; private set; }

		public ClassPropertyLocation(Class dataClass, Property p, PropertyValueLocationMode locationMode, string columnName) {
			Class = dataClass;
			Property = p;
			Location = locationMode;
			ColumnName = columnName;
		}
	}

	public enum PropertyValueLocationMode {
		ValueTable,
		TableColumn
	}

}
