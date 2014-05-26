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
using System.Runtime.Serialization;
using System.Data;
using System.ServiceModel;
using System.Globalization;

using NI.Data.Storage.Model;
using NI.Data;
using NI.Data.RelationalExpressions;
using NI.Data.Storage.Service.Schema;

namespace NI.Data.Storage.Service.Actions {
	
	public class LoadRelex {
		
		DataSchema Schema;
		IDalc StorageDalc;

		public LoadRelex(DataSchema schema, IDalc storageDalc) {
			Schema = schema;
			StorageDalc = storageDalc;
		}

		public LoadRelexResult Execute(string relex, bool totalcount) {
			var res = new LoadRelexResult();
			var relexParser = new RelExParser();
			var q = relexParser.Parse(relex);

			if (totalcount) {
				res.TotalCount = StorageDalc.RecordsCount( q );
			}

			var ds = new DataSet();
			var tbl = StorageDalc.Load(q, ds);
			
			var data = new RowList();
			foreach (DataRow r in tbl.Rows) {
				data.Add( new DataRowItem(r) );
			}
			res.Data = data;
			return res;
		}

	}


}
