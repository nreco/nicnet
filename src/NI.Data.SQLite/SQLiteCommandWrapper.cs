#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2013 NewtonIdeas
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
using System.Data.SQLite;

namespace NI.Data.SQLite
{
	/// <summary>
	/// </summary>
	public class SQLiteCommandWrapper : IDbCommandWrapper
	{
		IDbCommand _Command;
		DbTypeResolver DbTypeResolver;
		Func<QField,string> _QueryFieldValueFormatter = null;
		
		public IDbCommand Command { get { return _Command; } }
		
		/// <summary>
		/// Get or set default query field value formatter
		/// </summary>
		public Func<QField,string> QueryFieldValueFormatter {
			get { return _QueryFieldValueFormatter; }
			set { _QueryFieldValueFormatter = value; }
		}

		public SQLiteCommandWrapper(IDbCommand command)
		{
			_Command = command;
			DbTypeResolver = new DbTypeResolver();			
		}
		
		public string GetCmdParameterPlaceholder(string paramName) {
			return "?";
		}
		
		public IDbDataParameter CreateCmdParameter(DataColumn sourceColumn) {
			var cmdParam = new SQLiteParameter();
			
			cmdParam.DbType = DbTypeResolver.Resolve(sourceColumn.DataType);
			cmdParam.SourceColumn = sourceColumn.ColumnName;
			cmdParam.IsNullable = sourceColumn.AllowDBNull;
			
			return cmdParam;
		}		
		
		public IDbDataParameter CreateCmdParameter(object constantValue) {
			var cmdParam = new SQLiteParameter();
			cmdParam.Value = constantValue ?? DBNull.Value;
			return cmdParam;
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
			return ((SQLiteConnection)Command.Connection).LastInsertRowId;
		}
		
		
	}
}
