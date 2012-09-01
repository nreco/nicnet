#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2009 NewtonIdeas
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
using MySql.Data.MySqlClient;
using NI.Common.Providers;

namespace NI.Data.MySql
{
	/// <summary>
	/// </summary>
	public class MySqlCommandWrapper : IDbCommandWrapper
	{
		IDbCommand _Command;
		DbTypeResolver DbTypeResolver;
		QueryFieldValueFormatter _QueryFieldValueFormatter = null;
        IObjectProvider _CmdParameterPlaceholderProvider;

        public IObjectProvider CmdParameterPlaceholderProvider
        {
            get { return _CmdParameterPlaceholderProvider;  }
            set { _CmdParameterPlaceholderProvider = value;  }
        }

		public IDbCommand Command { get { return _Command; } }
		
		/// <summary>
		/// Get or set default query field value formatter
		/// </summary>
		public QueryFieldValueFormatter QueryFieldValueFormatter {
			get { return _QueryFieldValueFormatter; }
			set { _QueryFieldValueFormatter = value; }
		}

		public MySqlCommandWrapper(IDbCommand command)
		{
			_Command = command;
			DbTypeResolver = new DbTypeResolver();			
		}
		
		public string GetCmdParameterPlaceholder(string paramName) {
            return CmdParameterPlaceholderProvider != null ? 
                (string)CmdParameterPlaceholderProvider.GetObject(paramName) : paramName;
		}
		
		public IDbDataParameter CreateCmdParameter(DataColumn sourceColumn) {
			MySqlParameter param = new MySqlParameter();
			
			param.DbType = DbTypeResolver.Resolve(sourceColumn.DataType);
			param.SourceColumn = sourceColumn.ColumnName;
			param.IsNullable = sourceColumn.AllowDBNull;
			
			return param;
		}		
		
		public IDbDataParameter CreateCmdParameter(object constantValue) {
			MySqlParameter param = new MySqlParameter();
			param.Value = constantValue==null ? DBNull.Value : constantValue;
			return param;
		}		
		
		public void SetTransaction(IDbTransaction transaction) {
			Command.Transaction = transaction;
		}
		
		public IDbSqlBuilder CreateSqlBuilder() {
			IDbSqlBuilder builder = new DbSqlBuilder(this);
			if (QueryFieldValueFormatter!=null)
				builder.QueryFieldValueFormatter = QueryFieldValueFormatter;
			return builder;
		}

		/// <summary>
		/// Returns inserted row id
		/// </summary>
		public object GetInsertId() {
			MySqlCommand cmd = new MySqlCommand("SELECT LAST_INSERT_ID()", (Command.Connection as MySqlConnection));
			cmd.Transaction = (MySqlTransaction) Command.Transaction;
			return cmd.ExecuteScalar();
		}
		
		
	}
}
