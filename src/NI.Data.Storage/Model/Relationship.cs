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
    /// Generic class relationship
    /// </summary>
	public class Relationship {

		public Class Subject { get; set; }

        public Class Predicate { get; set; }

        public Class Object { get; set; }

        public bool Multiplicity { get; set; }

		public bool Reversed { get; set; }

		public Relationship() {

		}


		public override string ToString() {
			return String.Format("Relationship(Subject:{0}, Predicate:{1}, Object:{2})", Subject.ID, Predicate.ID, Object.ID);
		}


	}
}
