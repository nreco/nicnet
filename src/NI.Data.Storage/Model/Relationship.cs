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
using System.Data;
using System.Threading.Tasks;

namespace NI.Data.Storage.Model {
	
    /// <summary>
    /// Represents relationship determined by subject, predictate and object classes
    /// </summary>
	public sealed class Relationship {

		/// <summary>
		/// Unique identifier of this relationship (applicable only for non-reversed relations)
		/// </summary>
		public string ID { get; private set; }

		/// <summary>
		/// Subject class of this relationship
		/// </summary>
		public Class Subject { get; private set; }

        /// <summary>
        /// Predicate class of this relationship (applicable only for non-inferred relations)
        /// </summary>
		public Class Predicate { get; private set; }

        /// <summary>
        /// Object class of this relationship
        /// </summary>
		public Class Object { get; private set; }

        /// <summary>
        /// True if cardinality of this relationship supports multiplicity (*:N)
        /// </summary>
		public bool Multiplicity { get; private set; }

		/// <summary>
		/// True if this relationship is reversed view of another relationship
		/// </summary>
		public bool Reversed { get; private set; }

		/// <summary>
		/// True if this relationship is inferred view of relationship sequence
		/// </summary>
		public bool Inferred { get; private set; }

		/// <summary>
		/// Sequence of relationships (applicable only for inferred relationship)
		/// </summary>
		public IEnumerable<Relationship> InferredByRelationships { get; private set; }

		public Relationship(Class subj, Class predicate, Class obj, bool multiplicity, bool reversed) {
			Subject = subj;
			Predicate = predicate;
			Object = obj;
			Multiplicity = multiplicity;
			Reversed = reversed;
			Inferred = false;
			
			// only "real" relations have an ID
			if (!reversed)
				ID = String.Format("{0}_{1}_{2}", subj.ID, predicate.ID, obj.ID );
		}

		public Relationship(Class subj, IEnumerable<Relationship> inferredByRelationships, Class obj) {
			Inferred = true;
			Subject = subj;
			Object = obj;
			InferredByRelationships = inferredByRelationships;
			
			Multiplicity = false;
			// validate and check multiplicity
			var lastSubj = Subject;
			foreach (var r in inferredByRelationships) {
				if (r.Subject!=lastSubj)
					throw new ArgumentException("Relationship cannot be inferred from given relationships");
				lastSubj = r.Object;
				if (r.Multiplicity)
					Multiplicity = true;
			}
			if (lastSubj!=Object)
				throw new ArgumentException("Relationship cannot be inferred from given relationships");
		}


		public DataTable CreateDataTable() {
			if (Inferred)
				throw new NotSupportedException("Inferred relationship doesn't support DataTable creation");
			if (Reversed)
				throw new NotSupportedException("Reversed relationship doesn't support DataTable creation");

			var t = new DataTable(ID);
			var subjCol = t.Columns.Add("subject_id", typeof(long));
			subjCol.AllowDBNull = false;
			var objCol = t.Columns.Add("object_id", typeof(long));
			objCol.AllowDBNull = false;
			t.PrimaryKey = new[] { subjCol, objCol };
			return t;
		}

		public override int GetHashCode() {
			var hash = Subject.GetHashCode() ^ Object.GetHashCode();
			if (Inferred) {
				foreach (var r in InferredByRelationships) {
					hash = hash^r.GetHashCode();
				}
			} else {
				hash = hash^Predicate.GetHashCode();
			}
			return hash;
		}

		public override bool Equals(object obj) {
			if (obj is Relationship) {
				var r = (Relationship)obj;
				if (r.Subject!=Subject || r.Object!=Object)
					return false;
				if (r.Inferred && Inferred) {
					var otherInfRels = r.InferredByRelationships.GetEnumerator();
					foreach (var infRel in InferredByRelationships) {
						if (!otherInfRels.MoveNext()) // sequence is shorter
							return false;
						if (infRel!=otherInfRels.Current) // element doesn't match
							return false;
					}
					if (otherInfRels.MoveNext()) // sequence is longer
						return false;
					return true;
				}
				return r.Predicate==Predicate;
			}
			return base.Equals(obj);
		}

		public override string ToString() {
			string predicateStr;
			if (Inferred) {
				predicateStr = "Inferred: "+String.Join(" -> ", InferredByRelationships.Select( r=>r.ToString()).ToArray() );
			} else {
				predicateStr = String.Format("Predicate: {0}", Predicate.ID);
			}
			return String.Format("Relationship(Subject:{0}, {1}, Object:{2})", Subject.ID, predicateStr, Object.ID);
		}


	}
}
