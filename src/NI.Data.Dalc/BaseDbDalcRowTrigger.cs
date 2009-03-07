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
using System.Text;
using System.Data;
using System.Data.Common;
using NI.Common;
using NI.Common.Providers;

namespace NI.Data.Dalc {
	
	/// <summary>
	/// Base class for DB DALC datarow events trigger.
	/// </summary>
	public abstract class BaseDbDalcRowTrigger {

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
			Deleted = 32
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

		public BaseDbDalcRowTrigger() {
		}

		public BaseDbDalcRowTrigger(string sourceName) {
			MatchSourceName = sourceName;
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
			Execute(eventType, e.Row);
			// lets ensure that command has actual values
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
			Execute(eventType, e.Row);
		}

		protected abstract void Execute(EventType eventType, DataRow r);

	}

}
