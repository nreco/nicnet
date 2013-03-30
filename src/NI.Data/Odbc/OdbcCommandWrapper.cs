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
using System.Data.Odbc;

namespace NI.Data.Odbc
{
	/// <summary>
	/// </summary>
	public class OdbcCommandWrapper : IDbCommandWrapper
	{
		IDbCommand _Command;
		DbTypeResolver DbTypeResolver;
		IQueryFieldValueFormatter _QueryFieldValueFormatter = null;

		public Func<string, string> CmdParameterPlaceholderProvider { get; set; }

		public IDbCommand Command { get { return _Command; } }
		
		/// <summary>
		/// Get or set default query field value formatter
		/// </summary>
		public IQueryFieldValueFormatter QueryFieldValueFormatter {
			get { return _QueryFieldValueFormatter; }
			set { _QueryFieldValueFormatter = value; }
		}
		
		public OdbcCommandWrapper(IDbCommand command)
		{
			_Command = command;
			DbTypeResolver = new DbTypeResolver();			
		}
		
		public string GetCmdParameterPlaceholder(string paramName) {
            return CmdParameterPlaceholderProvider != null ? 
                CmdParameterPlaceholderProvider(paramName) : paramName;
		}
		
		public IDbDataParameter CreateCmdParameter(DataColumn sourceColumn) {
			OdbcParameter odbcParam = new OdbcParameter();
			
			odbcParam.DbType = DbTypeResolver.Resolve(sourceColumn.DataType);
			odbcParam.SourceColumn = sourceColumn.ColumnName;
			odbcParam.IsNullable = sourceColumn.AllowDBNull;
			
			return odbcParam;
		}		
		
		public IDbDataParameter CreateCmdParameter(object constantValue) {
			OdbcParameter odbcParam = new OdbcParameter();
			odbcParam.Value = constantValue==null ? DBNull.Value : constantValue;
			return odbcParam;
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
			OdbcCommand cmd = new OdbcCommand("SELECT @@IDENTITY", (Command.Connection as OdbcConnection) );
			cmd.Transaction = (OdbcTransaction) Command.Transaction;
			return cmd.ExecuteScalar();
		}
		
		
	}
}
