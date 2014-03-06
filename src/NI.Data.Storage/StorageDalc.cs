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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Threading.Tasks;

using NI.Data.Storage.Model;
using NI.Data;

namespace NI.Data.Storage
{
    public class StorageDalc : IDalc {


		public Regex MatchClassSourceNameRegex { get; set; }

        protected OntologyDalcPersister OntologyPersister { get; set; }
		protected ISqlDalc UnderlyingDalc { get; set; }
		protected Ontology StorageOntology { get; set; }

        public StorageDalc(ISqlDalc dalc, OntologyDalcPersister ontologyPersister) {
			UnderlyingDalc = dalc;
			OntologyPersister = ontologyPersister;
			StorageOntology = OntologyPersister.GetOntology();
			MatchClassSourceNameRegex = new Regex("^class_(?<classid>[a-zA-Z0-9_-]+)_objects$", RegexOptions.Singleline);
        }

		protected Class MatchClassBySourceName(string sourceName) {
			var m = MatchClassSourceNameRegex.Match(sourceName);
			if (m.Success) {
				var classId = m.Groups["classid"].Value;
				return StorageOntology.FindClassByID(classId);
			}
			return null;
		}


		public void ExecuteReader(Query q, Action<IDataReader> handler) {
			var ds = new DataSet();
			Load(q, ds);
			handler( new DataTableReader(ds.Tables[q.Table.Name]) );
		}

		public int Delete(Query query) {
			var srcName = new QTable(query.Table.Name);
			var dataClass = MatchClassBySourceName(srcName.Name);
			if (dataClass != null) {
				return 0;
			} else {
				return UnderlyingDalc.Delete(query);
			}
		}

		public void Insert(string tableName, IDictionary<string, IQueryValue> data) {
			var dataClass = MatchClassBySourceName(tableName);
			if (dataClass != null) {

			} else {
				UnderlyingDalc.Insert(tableName,data);
			}
		}

		public DataTable Load(Query query, DataSet ds) {
			var srcName = new QTable(query.Table.Name);
			var dataClass = MatchClassBySourceName(srcName.Name);
			if (dataClass != null) {
				throw new NotImplementedException();
			} else {
				Load(query, ds);
				return ds.Tables[query.Table.Name];
			}
		}

		public int Update(Query query, IDictionary<string, IQueryValue> data) {
			throw new NotImplementedException();
		}

		public void Update(DataTable t) {
			throw new NotImplementedException();
		}

		protected Query TransformQuery(Query q, Class mainClass) {
			return q;
		}
	}
}
