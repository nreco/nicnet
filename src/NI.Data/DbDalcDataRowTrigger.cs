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

namespace NI.Data {
	
	/// <summary>
	/// Base class for DB DALC datarow events trigger.
	/// </summary>
	public class DbDalcDataRowTrigger {

		[Flags]
		public enum EventType {
			None = 0,
			
			/// <summary>
			/// Occurs before row insert.
			/// </summary>
			Inserting = 1,

			/// <summary>
			/// Occurs after row insert.
			/// </summary>
			Inserted = 2,

			/// <summary>
			/// Occurs before row update.
			/// </summary>
			Updating = 4,

			/// <summary>
			/// Occurs after row update
			/// </summary>
			Updated = 8,

			/// <summary>
			/// Occurs before row delete
			/// </summary>
			Deleting = 16,

			/// <summary>
			/// Occurs after row delete
			/// </summary>
			Deleted = 32,

			/// <summary>
			/// Inserting or Updating
			/// </summary>
			Saving = 1+4,

			/// <summary>
			/// Inserted or Updated
			/// </summary>
			Saved = 2+8
		}

		string _MatchSourceName = null;
		EventType _MatchEvent = EventType.None;

		/// <summary>
		/// Get or set source name that should be matched for this trigger.
		/// </summary>
		public string MatchSourceName {
			get { return _MatchSourceName; }
			set { _MatchSourceName = value; }
		}

		/// <summary>
		/// Get or set flags that determine what events trigger should match. 
		/// </summary>
		public EventType MatchEvent {
			get { return _MatchEvent; }
			set { _MatchEvent = value; }
		}

		public Action<DataRowTriggerEventArgs> Operation { get; set; }

		public DbDalcDataRowTrigger() {
		}

		public DbDalcDataRowTrigger(string sourceName) {
			MatchSourceName = sourceName;
		}

		protected virtual void LogDebug(string s) {
			Debug.Write(s);
		}

		protected virtual void LogError(string s) {
			Trace.TraceError(s);
		}		

		private EventType GetBeforeEventType(StatementType statement) {
			switch (statement) {
				case StatementType.Insert: return EventType.Inserting;
				case StatementType.Update: return EventType.Updating;
				case StatementType.Delete: return EventType.Deleting;
			}
			return EventType.None;
		}

		private EventType GetAfterEventType(StatementType statement) {
			switch (statement) {
				case StatementType.Insert: return EventType.Inserted;
				case StatementType.Update: return EventType.Updated;
				case StatementType.Delete: return EventType.Deleted;
			}
			return EventType.None;
		}

		protected virtual bool IsMatch(DataRow r, EventType eventType) {
			// event 
			if ((MatchEvent & eventType) != eventType)
				return false;
			// sourcename
			if (MatchSourceName != null && MatchSourceName != r.Table.TableName)
				return false;
			return true;
		}

		public void RowUpdatingHandler(object sender, RowUpdatingEventArgs e) {
			EventType eventType = GetBeforeEventType(e.StatementType);
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

		public void RowUpdatedHandler(object sender, RowUpdatedEventArgs e) {
			EventType eventType = GetAfterEventType( e.StatementType );
			if (!IsMatch(e.Row, eventType)) return;
			Execute(eventType, e.Row, sender, e);
		}

		protected virtual void Execute(EventType eventType, DataRow r, object sender, EventArgs args) {
			if (Operation != null)
				Operation(new DataRowTriggerEventArgs(eventType, r, sender, args));
		}


		public class DataRowTriggerEventArgs : EventArgs {
			public EventType Event { get; private set; }
			public DataRow Row { get; private set; }
			public object Sender { get; private set; }
			public EventArgs Args { get; private set; }

			public DataRowTriggerEventArgs(EventType eventType, DataRow r, object sender, EventArgs args) {
				Event = eventType;
				Row = r;
				Sender = sender;
				Args = args;
			}
		}


	}


}
