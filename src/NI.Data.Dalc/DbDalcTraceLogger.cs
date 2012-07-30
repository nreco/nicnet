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
using System.Collections;
using System.Text;
using System.Data;
using System.Diagnostics;

using NI.Common;
using NI.Common.Providers;

namespace NI.Data.Dalc {
	
	/// <summary>
	/// DB DALC trace logger
	/// </summary>
	public class DbDalcTraceLogger {

		IDbDalcEventsMediator _DbDalcEventsMediator = null;
		bool _ReplaceParamPlaceholders = true;
		string _LogMsgPrefix = "[SQL]";
		bool _Enabled = true;
		IObjectProvider _LogFilter = null;

		/// <summary>
		/// Get or set flag that indicates whether logger is enabled
		/// </summary>		
		public bool Enabled {
			get { return _Enabled; }
			set { _Enabled = value; }
		}

		/// <summary>
		/// Get or set custom log filter 
		/// </summary>
		public IObjectProvider LogFilter {
			get { return _LogFilter; }
			set { _LogFilter = value; }
		}


		/// <summary>
		/// Get or set log message prefix 
		/// </summary>
		public string LogMsgPrefix {
			get { return _LogMsgPrefix; }
			set { _LogMsgPrefix = value; }
		}

		/// <summary>
		/// Get or set flag that indicates whether command parameters
		/// should be replaced with their values in command text
		/// </summary>
		public bool ReplaceParamPlaceholders {
			get { return _ReplaceParamPlaceholders; }
			set { _ReplaceParamPlaceholders = value; }
		}

		/// <summary>
		/// Get or set DALC events mediator
		/// </summary>
		public IDbDalcEventsMediator DbDalcEventsMediator {
			get { return _DbDalcEventsMediator; }
			set { 
				if (_DbDalcEventsMediator!=null) {
					_DbDalcEventsMediator.CommandExecuting -= new DbCommandEventHandler(DbDalcCommandExecuting);
					_DbDalcEventsMediator.CommandExecuted -= new DbCommandEventHandler(DbDalcCommandExecuted);
				}
				_DbDalcEventsMediator = value;
				if (_DbDalcEventsMediator!=null) {
					_DbDalcEventsMediator.CommandExecuting += new DbCommandEventHandler(DbDalcCommandExecuting);
					_DbDalcEventsMediator.CommandExecuted += new DbCommandEventHandler(DbDalcCommandExecuted);
				
				}
			}
		}

		IDictionary _LastLogTime = new Hashtable();

		protected IDictionary LastLogTime {
			get { return _LastLogTime; }
		}

		public DbDalcTraceLogger() { }

		protected virtual void DbDalcCommandExecuting(object sender, DbCommandEventArgs args) {
			if (!Enabled) return;
			try {
				Write(args.Command, FormatDbCommand(args.Command) );
			} catch (Exception ex) {
				Trace.Fail("Cannot write SQL command trace: "+ex.Message);
			}
		}

		protected virtual void DbDalcCommandExecuted(object sender, DbCommandEventArgs args) {
			if (!Enabled) return;
			try {
				// count execution time
				if (!LastLogTime.Contains( args.Command.GetHashCode() ))
					Write(args.Command, "Cannot calculate execution time - 'executing' event wasn't raised?!");
				else {
					DateTime executingTime = (DateTime)LastLogTime[args.Command.GetHashCode()];
					string cmdTimeMsg = String.Format("execution time: {0:f}", DateTime.Now.Subtract(executingTime) );
					Write(args.Command, cmdTimeMsg);
				}
			} catch (Exception ex) {
				Trace.Fail("Cannot write SQL command trace: " + ex.Message);
			}
		}

		protected virtual void Write(IDbCommand cmd, string message) {
			LastLogTime[cmd.GetHashCode()] = DateTime.Now;
			string msg = String.Format("{0} [{1}] {2}", LogMsgPrefix, cmd.GetHashCode(), message );
			if (LogFilter != null) {
				msg = LogFilter.GetObject(msg) as string;
			}
			if (msg!=null)
				Trace.WriteLine( msg );
		}


		protected string FormatDbCommand(IDbCommand dbCmd) {
			string commandText = dbCmd.CommandText;
			if (ReplaceParamPlaceholders) {
				for (int i=dbCmd.Parameters.Count-1; i>=0; i--) {
					IDbDataParameter param = (IDbDataParameter)dbCmd.Parameters[i];
					commandText = ReplaceDbParameter(commandText, param);
				}
			} else {
				StringBuilder paramsBuilder = new StringBuilder();
				foreach (IDbDataParameter param in dbCmd.Parameters)
					paramsBuilder.Append( FormatDbParameter(param) );
				commandText = commandText + paramsBuilder.ToString();
			}
			return commandText;
		}

		protected string FormatDbParameterValue(IDbDataParameter dbParam) {
			string paramValue = dbParam.Value==null || dbParam.Value==DBNull.Value ?
				"NULL" : Convert.ToString(dbParam.Value);
			return "'"+paramValue.Replace("'", "''")+"'";
		}

		protected string FormatDbParameter(IDbDataParameter dbParam) {
			return String.Format("; {0}={1}", dbParam.ParameterName, FormatDbParameterValue(dbParam) );
		}

		protected string ReplaceDbParameter(string cmdText, IDbDataParameter dbParam) {
			int paramIndex = cmdText.IndexOf(dbParam.ParameterName);
			if (paramIndex<0)
				return cmdText += FormatDbParameter(dbParam);
			return cmdText.Replace(dbParam.ParameterName, FormatDbParameterValue( dbParam ) ); 
		}

	}
}
