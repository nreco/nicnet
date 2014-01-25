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

namespace NI.Data {
	
	/// <summary>
	/// DB DALC trace logger
	/// </summary>
	public class SqlCommandTraceTrigger {

		/// <summary>
		/// Get or set flag that indicates whether logger is enabled
		/// </summary>		
		public bool Enabled { get; set; }

		/// <summary>
		/// Get or set write to log method (by default Debug
		/// </summary>
		public Action<string> LogWrite { get; set; }

		IDictionary<int, DateTime> cmdExecutingTime = new Dictionary<int, DateTime>();

		public SqlCommandTraceTrigger() : this( new Action<string>(TraceWrite) ) {
		}

		public SqlCommandTraceTrigger(Action<string> logWrite) {
			Enabled = true;
			LogWrite = logWrite;
		}

		static void TraceWrite(string s) {
			System.Diagnostics.Trace.Write(s);
		}

		public virtual void DbDalcCommandExecuting(object sender, DbCommandEventArgs args) {
			if (!Enabled) return;
			try {
				cmdExecutingTime[args.Command.GetHashCode()] = DateTime.Now;
				Write(args.Command, FormatDbCommand(args.Command) );
			} catch (Exception ex) {
				Trace.Fail("Cannot write SQL command trace: "+ex.Message);
			}
		}

		public virtual void DbDalcCommandExecuted(object sender, DbCommandEventArgs args) {
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
			string msg = String.Format("[SQL][{1}] {2}", cmd.GetHashCode(), message );
			if (LogWrite != null)
				LogWrite(msg);
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
