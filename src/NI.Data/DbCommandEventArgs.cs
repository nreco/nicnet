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
using System.Data;

namespace NI.Data
{
	/// <summary>
	/// Database command event arguments.
	/// </summary>
	public class DbCommandEventArgs : EventArgs
	{

		/// <summary>
		/// Get or set affected source name.
		/// </summary>
		public string TableName { get; private set; }

		/// <summary>
		/// Get or set DB command type.
		/// </summary>
		public StatementType CommandType { get; private set; }	

		/// <summary>
		/// Get or set event argument 
		/// </summary>
		public IDbCommand Command { get; private set; }	
		
		
		public DbCommandEventArgs(string tableName, StatementType commandType, IDbCommand command)
		{
			TableName = tableName;
            CommandType = commandType;
			Command = command;
		}
	}

	/// <summary>
	/// Represents database command executing event data
	/// </summary>
	public class DbCommandExecutingEventArgs : DbCommandEventArgs {

		public DbCommandExecutingEventArgs(string tableName, StatementType commandType, IDbCommand command) :
			base(tableName, commandType, command) { }
	}

	/// <summary>
	/// Represents database command executed event data
	/// </summary>
	public class DbCommandExecutedEventArgs : DbCommandEventArgs {

		public DbCommandExecutedEventArgs(string tableName, StatementType commandType, IDbCommand command) :
			base(tableName, commandType, command) { }
	}

	
	
}
