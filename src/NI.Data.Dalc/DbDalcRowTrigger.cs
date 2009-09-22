#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2008 NewtonIdeas
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
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Data;
using System.Data.Common;
using NI.Common;
using NI.Common.Operations;

namespace NI.Data.Dalc {
	
	/// <summary>
	/// DB DALC datarow events trigger based on external operation.
	/// </summary>
	public class DbDalcRowTrigger : BaseDbDalcRowTrigger {
		IOperation _Operation;

		/// <summary>
		/// Get or set source name that should be matched for this trigger.
		/// </summary>
		public IOperation Operation {
			get { return _Operation; }
			set { _Operation = value; }
		}


		public DbDalcRowTrigger() {
		}

		public DbDalcRowTrigger(string sourceName) : base(sourceName) {
		}

		protected override void Execute(EventType eventType, DataRow r, object sender, EventArgs args) {
			ListDictionary context = new ListDictionary();
			context["event"] = eventType;
			context["row"] = r;
			context["args"] = args;
			context["sender"] = sender;
			Operation.Execute(context);
		}

	}

}
