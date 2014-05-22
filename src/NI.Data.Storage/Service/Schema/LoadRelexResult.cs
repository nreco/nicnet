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
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Data;
using System.ComponentModel;
using System.Threading.Tasks;

namespace NI.Data.Storage.Service.Schema {

	[DataContract(Name = "result")]
	public class LoadRelexResult {

		[DataMember(Name = "data")]
		public DataRowItemList Data { get; set; }

		[DataMember(Name = "totalcount", EmitDefaultValue=true)]
		[DefaultValue(null)]
		public int? TotalCount { get; set; }

		public LoadRelexResult() {
		}

	}

	[CollectionDataContract(ItemName = "row")]
	public class DataRowItemList : List<DataRowItem> { }


}
