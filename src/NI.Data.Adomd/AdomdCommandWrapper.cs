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
using Microsoft.AnalysisServices.AdomdClient;

namespace NI.Data.Adomd
{
	/// <summary>
	/// </summary>
	public class AdomdCommandWrapper : IDbCommandWrapper
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

        public AdomdCommandWrapper(IDbCommand command)
		{
			_Command = command;
			DbTypeResolver = new DbTypeResolver();			
		}
		
		public string GetCmdParameterPlaceholder(string paramName) {
			return paramName;
		}
		
		public IDbDataParameter CreateCmdParameter(DataColumn sourceColumn) {
			AdomdParameter adomdParam = new AdomdParameter();
            adomdParam.DbType = DbTypeResolver.Resolve(sourceColumn.DataType);
            adomdParam.SourceColumn = sourceColumn.ColumnName;
            adomdParam.IsNullable = sourceColumn.AllowDBNull;

            return adomdParam;
		}		
		
		public IDbDataParameter CreateCmdParameter(object constantValue) {
            AdomdParameter adomdParam = new AdomdParameter();
            adomdParam.Value = constantValue;
            return adomdParam;
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
            AdomdCommand cmd = new AdomdCommand("SELECT @@IDENTITY", (Command.Connection as AdomdConnection));
			//cmd.Transaction = (AdomdTransaction) Command.Transaction;
			return cmd.ExecuteScalar();
		}
		
		
	}
}
