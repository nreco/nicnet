#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
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

namespace NI.Data
{
	/// <summary>
	/// Database DALC events mediator.
	/// </summary>
	public class DbDalcEventsMediator : IDbDalcEventsMediator
	{
		public event DbRowUpdatingEventHandler RowUpdating;

		public event DbRowUpdatedEventHandler RowUpdated;

		public event EventHandler<DbCommandEventArgs> CommandExecuting;

		public event EventHandler<DbCommandEventArgs> CommandExecuted;

		public DbDalcEventsMediator()
		{
		}


		public void OnRowUpdating(object sender, RowUpdatingEventArgs e) {
			if (RowUpdating!=null)
				RowUpdating(sender, e);		
		}

		public void OnRowUpdated(object sender, RowUpdatedEventArgs e) {
			if (RowUpdated!=null)
				RowUpdated(sender, e);
		}

		public void OnCommandExecuting(object sender, DbCommandEventArgs e) {
			if (CommandExecuting!=null)
				CommandExecuting(sender, e);
		}

		public void OnCommandExecuted(object sender, DbCommandEventArgs e) {
			if (CommandExecuted!=null)
				CommandExecuted(sender, e);
		}

	}
}
