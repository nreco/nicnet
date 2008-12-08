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
using System.Data;
using System.Data.Common;

namespace NI.Data.Dalc
{
	/// <summary>
	/// Database DALC events mediator.
	/// </summary>
	public class DbDalcEventsMediator : IDbDalcEventsMediator
	{
		public event DbRowUpdatingEventHandler RowUpdating;

		public event DbRowUpdatedEventHandler RowUpdated;

		public event DbCommandEventHandler CommandExecuting;

		public event DbCommandEventHandler CommandExecuted;

		public DbDalcEventsMediator()
		{
		}


		public void OnRowUpdating(RowUpdatingEventArgs e) {
			if (RowUpdating!=null)
				RowUpdating(this, e);		
		}

		public void OnRowUpdated(RowUpdatedEventArgs e) {
			if (RowUpdated!=null)
				RowUpdated(this, e);
		}

		public void OnCommandExecuting(DbCommandEventArgs e) {
			if (CommandExecuting!=null)
				CommandExecuting(this, e);
		}

		public void OnCommandExecuted(DbCommandEventArgs e) {
			if (CommandExecuted!=null)
				CommandExecuted(this, e);
		}

	}
}
