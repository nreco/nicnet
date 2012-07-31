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
	/// Database dalc events mediator.
	/// </summary>
	public interface IDbDalcEventsMediator
	{
		/// <summary>
		/// Occurs during an update operation before a command is executed against the data source.
		/// </summary>
		event DbRowUpdatingEventHandler RowUpdating;

		/// <summary>
		/// Occurs during an update operation after a command is executed against the data source.
		/// </summary>
		event DbRowUpdatedEventHandler RowUpdated;
		
		/// <summary>
		/// Occurs before a DB command is executed
		/// </summary>
		event DbCommandEventHandler CommandExecuting;

		/// <summary>
		/// Occurs after a DB command is executed
		/// </summary>
		event DbCommandEventHandler CommandExecuted;
		
		
		/// <summary>
		/// Raise BeforeRowUpdate event
		/// </summary>
		void OnRowUpdating(object sender, RowUpdatingEventArgs e);
		
		/// <summary>
		/// Raise AfterRowUpdate event
		/// </summary>
		void OnRowUpdated(object sender, RowUpdatedEventArgs e);
		
		/// <summary>
		/// Raise BeforeCommandExecute event
		/// </summary>
		void OnCommandExecuting(object sender, DbCommandEventArgs e);

		/// <summary>
		/// Raise AfterCommandExecute event
		/// </summary>
		void OnCommandExecuted(object sender, DbCommandEventArgs e);


	}
	
	
	
}
