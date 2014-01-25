#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
 * Copyright 2008-2013 Vitalii Fedorchenko (changes and v.2)
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
using System.Diagnostics;
using System.Text;
using System.Data;
using System.Data.Common;

namespace NI.Data.Triggers {
	
	/// <summary>
	/// Generic DataRow trigger that consumes data events and triggers an action on certain conditions.
	/// </summary>
	public class DataRowTrigger {
		
		static Logger log = new Logger(typeof(DataRowTrigger));

		/// <summary>
		/// Get or set table name that should be matched by this trigger.
		/// </summary>
		public string TableName { get; set; }

		/// <summary>
		/// Get or set flags that determine what DataRow actions trigger should match. 
		/// </summary>
		public DataRowActionType Action { get; set; }

		/// <summary>
		/// Get or set trigger handler delegate
		/// </summary>
		public Action<DataRowTriggerEventArgs> Handler { get; set; }

		/// <summary>
		/// Initializes new instance of DataRowTrigger with specified row action, table name and handler
		/// </summary>
		/// <param name="rowAction">row action flags</param>
		/// <param name="tableName">table name to match</param>
		/// <param name="handler">handler delegate</param>
		public DataRowTrigger(DataRowActionType rowAction, string tableName, Action<DataRowTriggerEventArgs> handler) :
			this(null, rowAction, tableName, handler) {
		}

		/// <summary>
		/// Initializes new instance of DataRowTrigger with specified row action, table name, handler and subscribes it to appropriate data events.
		/// </summary>
		/// <param name="broker">data events broker</param>
		/// <param name="rowAction">row action flags</param>
		/// <param name="tableName">table name to match</param>
		/// <param name="handler">handler delegate</param>
		public DataRowTrigger(DataEventBroker broker, DataRowActionType rowAction, string tableName, Action<DataRowTriggerEventArgs> handler) {
			TableName = tableName;
			Action = rowAction;
			Handler = handler;
			if (broker!=null) {
				broker.Subscribe( new Func<EventArgs,bool>(IsMatchRowUpdating), new EventHandler<RowUpdatingEventArgs>(RowUpdatingHandler) );
				broker.Subscribe( new Func<EventArgs,bool>(IsMatchRowUpdated), new EventHandler<RowUpdatedEventArgs>(RowUpdatedHandler));
			}
		}

		protected bool IsMatchRowUpdating(EventArgs args) {
			if (!(args is RowUpdatingEventArgs)) return false;
			var rowUpdatingArgs = (RowUpdatingEventArgs)args;
			return IsMatch( rowUpdatingArgs.Row, GetBeforeActionType(rowUpdatingArgs.StatementType) );
		}
		protected bool IsMatchRowUpdated(EventArgs args) {
			if (!(args is RowUpdatedEventArgs)) return false;
			var rowUpdatedArgs = (RowUpdatedEventArgs)args;
			return IsMatch(rowUpdatedArgs.Row, GetAfterActionType(rowUpdatedArgs.StatementType));
		}

		private DataRowActionType GetBeforeActionType(StatementType statement) {
			switch (statement) {
				case StatementType.Insert: return DataRowActionType.Inserting;
				case StatementType.Update: return DataRowActionType.Updating;
				case StatementType.Delete: return DataRowActionType.Deleting;
			}
			return DataRowActionType.None;
		}

		private DataRowActionType GetAfterActionType(StatementType statement) {
			switch (statement) {
				case StatementType.Insert: return DataRowActionType.Inserted;
				case StatementType.Update: return DataRowActionType.Updated;
				case StatementType.Delete: return DataRowActionType.Deleted;
			}
			return DataRowActionType.None;
		}

		protected virtual bool IsMatch(DataRow r, DataRowActionType eventType) {
			// event 
			if ((Action & eventType) != eventType)
				return false;
			// table name
			if (TableName != null && TableName != r.Table.TableName)
				return false;
			return true;
		}

		public virtual void RowUpdatingHandler(object sender, RowUpdatingEventArgs e) {
			var eventType = GetBeforeActionType(e.StatementType);
			if (!IsMatch(e.Row, eventType)) return;

			Execute(eventType, e.Row, sender, e);
			// lets ensure that command has actual values
			if (e.Row.RowState!=DataRowState.Deleted && e.Row.RowState!=DataRowState.Detached)
				foreach (IDataParameter param in e.Command.Parameters)
					if ((param.Direction == ParameterDirection.Input ||
						param.Direction == ParameterDirection.InputOutput) &&
						!String.IsNullOrEmpty(param.SourceColumn) ) {
						param.Value = e.Row[param.SourceColumn];
					}
		}

		public virtual void RowUpdatedHandler(object sender, RowUpdatedEventArgs e) {
			var eventType = GetAfterActionType( e.StatementType );
			if (!IsMatch(e.Row, eventType)) return;

			Execute(eventType, e.Row, sender, e);
		}

		protected virtual void Execute(DataRowActionType eventType, DataRow r, object sender, EventArgs args) {
			if (Handler != null)
				Handler(new DataRowTriggerEventArgs(eventType, r, sender, args));
		}

	}


	public class DataRowTriggerEventArgs : EventArgs {
		public DataRowActionType Action { get; private set; }
		public DataRow Row { get; private set; }
		public object Sender { get; private set; }
		public EventArgs Args { get; private set; }

		public DataRowTriggerEventArgs(DataRowActionType action, DataRow r, object sender, EventArgs args) {
			Action = action;
			Row = r;
			Sender = sender;
			Args = args;
		}
	}


}
