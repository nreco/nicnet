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
using System.Data.OleDb;

namespace NI.Data.OleDb
{
	/// <summary>
	/// </summary>
	public class OleDbCommandWrapper : IDbCommandWrapper
	{
		IDbCommand _Command;
		DbTypeResolver DbTypeResolver;
		IQueryFieldValueFormatter _QueryFieldValueFormatter = null;
		
		public IDbCommand Command { get { return _Command; } }
		
		/// <summary>
		/// Get or set default query field value formatter
		/// </summary>
		public IQueryFieldValueFormatter QueryFieldValueFormatter {
			get { return _QueryFieldValueFormatter; }
			set { _QueryFieldValueFormatter = value; }
		}
		
		public OleDbCommandWrapper(IDbCommand command)
		{
			_Command = command;
			DbTypeResolver = new DbTypeResolver();			
		}
		
		public string GetCmdParameterPlaceholder(string paramName) {
			return "?";
		}
		
		public IDbDataParameter CreateCmdParameter(DataColumn sourceColumn) {
			OleDbParameter oleDbParam = new OleDbParameter();
			
			// something wrong with OleDbType.DBTimeStamp
			if (sourceColumn.DataType==typeof(DateTime)) {
				oleDbParam.OleDbType = OleDbType.Date;
			} else {
				oleDbParam.DbType = DbTypeResolver.Resolve(sourceColumn.DataType);
			}
			oleDbParam.SourceColumn = sourceColumn.ColumnName;
			oleDbParam.IsNullable = sourceColumn.AllowDBNull;
			
			return oleDbParam;
		}		
		
		public IDbDataParameter CreateCmdParameter(object constantValue) {
			OleDbParameter oleDbParam = new OleDbParameter();
			oleDbParam.Value = constantValue==null ? DBNull.Value : constantValue;
			// MSDN says, that DbType should be resolved automatically by Value's .net type
			// SURPRISE! OleDb is very stupid :)
			if (constantValue!=null) {
				Type valueType = constantValue.GetType();
				if (valueType==typeof(DateTime))
					oleDbParam.OleDbType = OleDbType.Date;
				else 
					oleDbParam.DbType = DbTypeResolver.Resolve(valueType);
			}
			return oleDbParam;
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
			OleDbCommand cmd = new OleDbCommand("SELECT @@IDENTITY", (Command.Connection as OleDbConnection) );
			cmd.Transaction = (OleDbTransaction) Command.Transaction;
			return cmd.ExecuteScalar();
		}
		
		
	}
}
