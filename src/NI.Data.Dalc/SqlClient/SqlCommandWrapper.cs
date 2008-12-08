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
using System.Data;
using System.Data.SqlClient;

namespace NI.Data.Dalc.SqlClient
{
	/// <summary>
	/// </summary>
	public class SqlCommandWrapper : IDbCommandWrapper
	{
		IDbCommand _Command;
		DbTypeResolver DbTypeResolver;
		bool TopOptimizationEnabled = false;
		bool ConstOptimizationEnabled = false;
		
		public IDbCommand Command { get { return _Command; } }
		

		public SqlCommandWrapper(IDbCommand command, DbTypeResolver dbTypeResolver, bool topOptEnabled, bool constOptEnabled)
		{
			_Command = command;
			DbTypeResolver = dbTypeResolver;
			TopOptimizationEnabled = topOptEnabled;
			ConstOptimizationEnabled = constOptEnabled;
		}
		
		public string GetCmdParameterPlaceholder(string paramName) {
			return paramName;
		}

		public IDbDataParameter CreateCmdParameter(DataColumn sourceColumn) {
			SqlParameter sqlParam = new SqlParameter();

			sqlParam.DbType = DbTypeResolver.Resolve(sourceColumn.DataType);
			sqlParam.SourceColumn = sourceColumn.ColumnName;
			sqlParam.IsNullable = sourceColumn.AllowDBNull;
			//if (param.DbType==DbType.DateTime && (param is System.Data.OleDb.OleDbParameter) ) {
			//	((System.Data.OleDb.OleDbParameter)param).OleDbType = System.Data.OleDb.OleDbType.Date;
			//}
			
			return sqlParam;
		}
		
		public IDbDataParameter CreateCmdParameter(object constantValue) {
			SqlParameter sqlParam = new SqlParameter();
			sqlParam.Value = constantValue==null ? DBNull.Value : constantValue;

			// MSDN says, that DbType should be resolved automatically by Value's .net type
			if (constantValue!=null && constantValue!=DBNull.Value) {
				sqlParam.DbType = DbTypeResolver.Resolve(constantValue);
			} else {
				sqlParam.IsNullable = true;
			}
			
			return sqlParam;
		}

		
		public void SetTransaction(IDbTransaction transaction) {
			if (transaction==null)
				try {
					Command.Transaction = transaction;
				} catch (ArgumentException ex) { }
			else
				Command.Transaction = transaction;
		}

		public IDbSqlBuilder CreateSqlBuilder() {
			IDbSqlBuilder builder = new SqlClientDbSqlBuilder(this, TopOptimizationEnabled, ConstOptimizationEnabled);
			return builder;
		}
			

		/// <summary>
		/// Returns inserted row id
		/// </summary>
		public object GetInsertId() {
			SqlCommand cmd = new SqlCommand("SELECT @@IDENTITY", (Command.Connection as SqlConnection) );
			cmd.Transaction = (SqlTransaction) Command.Transaction;
			return cmd.ExecuteScalar();
		}
		
		
		
		
		
		
	}
}
