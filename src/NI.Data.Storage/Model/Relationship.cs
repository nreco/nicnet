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
    /// Represents relationship determined by subject, predictate and object classes
    /// </summary>
	public class Relationship {

		public string ID { get; private set; }

		public Class Subject { get; private set; }

        public Class Predicate { get; private set; }

        public Class Object { get; private set; }

        public bool Multiplicity { get; private set; }

		public bool Reversed { get; private set; }

		public Relationship(Class subj, Class predicate, Class obj, bool multiplicity, bool reversed) {
			Subject = subj;
			Predicate = predicate;
			Object = obj;
			Multiplicity = multiplicity;
			Reversed = reversed;
			ID = String.Format("{0}_{1}_{2}", subj.ID, predicate.ID, obj.ID );
		}


		public override string ToString() {
			return String.Format("Relationship(Subject:{0}, Predicate:{1}, Object:{2})", Subject.ID, Predicate.ID, Object.ID);
		}


	}
}
