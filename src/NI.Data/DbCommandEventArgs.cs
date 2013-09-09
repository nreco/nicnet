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

namespace NI.Data
{
	/// <summary>
	/// Database command event arguments.
	/// </summary>
	public class DbCommandEventArgs : EventArgs
	{
        string sourceName;
        StatementType type;

		IDbCommand _Command;

		/// <summary>
		/// Get or set affected source name.
		/// </summary>
		public string SourceName {
			get { return sourceName; }
			set { sourceName = value; }
		}

		/// <summary>
		/// Get or set DB command type.
		/// </summary>
		public StatementType CommandType {
			get { return type; }
			set { type = value; }
		}	

		/// <summary>
		/// Get or set event argument 
		/// </summary>
		public IDbCommand Command {
			get { return _Command; }
			set { _Command = value; }
		}	
		
		public DbCommandEventArgs(StatementType commandType, IDbCommand command)
		{
			Command = command;
		}
		
		public DbCommandEventArgs(string dbSourceName, StatementType commandType, IDbCommand command)
		{
            SourceName = dbSourceName;
            CommandType = commandType;
			Command = command;
		}
	}
	
	
}
