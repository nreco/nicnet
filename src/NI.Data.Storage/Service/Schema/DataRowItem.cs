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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Data;
using System.Threading.Tasks;
using System.Globalization;

namespace NI.Data.Storage.Service.Schema {

	[Serializable]
	public class DataRowItem : ISerializable {
		DataRow DataRow;

		public DataRowItem(DataRow r) {
			DataRow = r;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			foreach (DataColumn c in DataRow.Table.Columns) {
				if (DataRow.IsNull(c)) { 
					info.AddValue(c.ColumnName, null, c.DataType);
				} else { 
					var val = DataRow[c];
					info.AddValue(c.ColumnName, val, c.DataType);
				}
			}
		}

	}

}
