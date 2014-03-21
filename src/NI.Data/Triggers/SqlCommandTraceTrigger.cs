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
using System.Text;
using System.Data;
using System.Diagnostics;

namespace NI.Data.Triggers {
	
	/// <summary>
	/// DB DALC trace logger
	/// </summary>
	public class SqlCommandTraceTrigger {

		static Logger log = new Logger(typeof(SqlCommandTraceTrigger));

		/// <summary>
		/// Get or set flag that indicates whether logger is enabled
		/// </summary>		
		public bool Enabled { get; set; }

		IDictionary<int, DateTime> cmdExecutingTime = new Dictionary<int, DateTime>();

		/// <summary>
		/// Initializes a new instance of SqlCommandTraceTrigger
		/// </summary>
		public SqlCommandTraceTrigger() {
			Enabled = true;
		}

		/// <summary>
		/// Initializes a new instance of SqlCommandTraceTrigger and subscribes it to data events
		/// </summary>
		public SqlCommandTraceTrigger(DataEventBroker eventBroker) : this() {
			eventBroker.Subscribe(new EventHandler<DbCommandExecutingEventArgs>(DbCommandExecuting) );
			eventBroker.Subscribe(new EventHandler<DbCommandExecutedEventArgs>(DbCommandExecuted));
		}

		public virtual void DbCommandExecuting(object sender, DbCommandExecutingEventArgs args) {
			if (!Enabled) return;
			try {
				cmdExecutingTime[args.Command.GetHashCode()] = DateTime.Now;
				Write(args.Command, FormatDbCommand(args.Command) );
			} catch (Exception ex) {
				Trace.Fail("Cannot write SQL command trace: "+ex.Message);
			}
		}

		public virtual void DbCommandExecuted(object sender, DbCommandEventArgs args) {
			if (!Enabled) return;
			try {
				// count execution time
				if (!cmdExecutingTime.ContainsKey(args.Command.GetHashCode()))
					Write(args.Command, "Cannot calculate execution time - 'executing' event wasn't raised?!");
				else {
					DateTime executingTime = cmdExecutingTime[args.Command.GetHashCode()];
					string cmdTimeMsg = String.Format("execution time: {0}", DateTime.Now.Subtract(executingTime) );
					Write(args.Command, cmdTimeMsg);
				}
			} catch (Exception ex) {
				Trace.Fail("Cannot write SQL command trace: " + ex.Message);
			}
		}

		protected virtual void Write(IDbCommand cmd, string message) {
			string msg = String.Format("[SQL][{0}] {1}", cmd.GetHashCode(), message );
			log.Info(msg);
		}


		protected string FormatDbCommand(IDbCommand dbCmd) {
			var commandText = new StringBuilder( dbCmd.CommandText );
			
			for (int i=dbCmd.Parameters.Count-1; i>=0; i--) {
				IDbDataParameter param = (IDbDataParameter)dbCmd.Parameters[i];
				
				commandText = commandText.Replace( param.ParameterName, FormatDbParameterValue( param ) );
			}
			
			return commandText.ToString();
		}

		protected string FormatDbParameterValue(IDbDataParameter dbParam) {
			string paramValue = dbParam.Value==null || dbParam.Value==DBNull.Value ?
				"NULL" : Convert.ToString(dbParam.Value);
			return "'"+paramValue.Replace("'", "''")+"'";
		}



	}
}
