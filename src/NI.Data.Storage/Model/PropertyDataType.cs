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
using System.Globalization;

namespace NI.Data.Storage.Model {
	
	public class PropertyDataType {
		public string ID { get; set; }
		public Type ValueType { get; set; }

		internal PropertyDataType() {

		}

		public bool IsEmpty(object val) {
			if (ValueType==typeof(string) && Convert.ToString(val) == System.String.Empty)
				return true;
			return val==null;
		}

		public object ConvertToValueType(object from) {
			switch (ID) {
				case "integer":
					return Convert.ToInt64(from, CultureInfo.InvariantCulture);
				case "decimal":
					return Convert.ToDecimal(from, CultureInfo.InvariantCulture);
				case "string":
					return Convert.ToString(from, CultureInfo.InvariantCulture);
				case "datetime":
					try {
						return Convert.ToDateTime(from);
					} catch (Exception ex) {
						return Convert.ToDateTime(from, CultureInfo.InvariantCulture);
					}
				case "date":
					try {
						return Convert.ToDateTime(from).Date;
					} catch (Exception ex) {
						return Convert.ToDateTime(from, CultureInfo.InvariantCulture).Date;
					}
				case "boolean":
					return from is bool ? ((bool)from) : Convert.ToBoolean(from, CultureInfo.InvariantCulture);
			}
			return from;
		}

		public static PropertyDataType FindByID(string id) {
			foreach (var dt in KnownDataTypes)
				if (dt.ID==id)
					return dt;
			return null;
		}

		public static readonly PropertyDataType String = new PropertyDataType() { ID = "string", ValueType = typeof(string) };
		public static readonly PropertyDataType Decimal = new PropertyDataType() { ID = "decimal", ValueType = typeof(decimal) };
		public static readonly PropertyDataType Integer = new PropertyDataType() { ID = "integer", ValueType = typeof(long) };
		public static readonly PropertyDataType DateTime = new PropertyDataType() { ID = "datetime", ValueType = typeof(DateTime) };
		public static readonly PropertyDataType Date = new PropertyDataType() { ID = "date", ValueType = typeof(DateTime) };
		public static readonly PropertyDataType Boolean = new PropertyDataType() { ID = "boolean", ValueType = typeof(bool) };

		public static readonly IEnumerable<PropertyDataType> KnownDataTypes = new[] {String, Integer, Decimal, DateTime, Date, Boolean};
	}

}
