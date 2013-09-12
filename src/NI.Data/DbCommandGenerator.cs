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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;

namespace NI.Data
{
	/// <summary>
	/// Database Command Generator
	/// </summary>
	public class DbCommandGenerator : IDbCommandGenerator
	{

		/// <summary>
		/// DB Factory instance
		/// </summary>
		protected IDbDalcFactory DbFactory {  get; set; }
		
		/// <summary>
		/// Initializes a new instance of the DbCommandGenerator class.
		/// </summary>
		public DbCommandGenerator(IDbDalcFactory dbFactory) {
			DbFactory = dbFactory;
		}
		
		/// <summary>
		/// Generate SELECT statement by query structure
		/// </summary>
		public virtual IDbCommand ComposeSelect(Query query) {
			var cmd = DbFactory.CreateCommand();
			var cmdSqlBuilder = DbFactory.CreateSqlBuilder(cmd);
			cmd.CommandText = cmdSqlBuilder.BuildSelect(query);
			return cmd;
		}
		
		/// <summary>
		/// Generates INSERT statement by DataTable
		/// </summary>
		public virtual IDbCommand ComposeInsert(DataTable table) {
			var cmd = DbFactory.CreateCommand();
			var dbSqlBuilder = DbFactory.CreateSqlBuilder(cmd);
			
			// Prepare fields part
			var insertFields = new List<string>();
			var insertValues = new List<string>();
			foreach (DataColumn col in table.Columns)
				if (!col.AutoIncrement) {
					insertFields.Add(col.ColumnName);
					insertValues.Add( dbSqlBuilder.BuildCommandParameter( col, DataRowVersion.Current ) );
				}
				
			cmd.CommandText = String.Format(
				"INSERT INTO {0} ({1}) VALUES ({2})",
				table.TableName,
				String.Join(",", insertFields.ToArray()) ,
				String.Join(",", insertValues.ToArray()) );
			
			return cmd;
		}
		
		/// <summary>
		/// Generates DELETE statement by DataTable
		/// </summary>
		public virtual IDbCommand ComposeDelete(DataTable table) {
			var cmd = DbFactory.CreateCommand();
			var dbSqlBuilder = DbFactory.CreateSqlBuilder(cmd);

			// prepare WHERE part
			var primaryKeys = new List<string>();
			foreach (DataColumn col in table.PrimaryKey) {
				string condition = String.Format(
						"{0}={1}",
						col.ColumnName,
						dbSqlBuilder.BuildCommandParameter( col, DataRowVersion.Current ) );
				primaryKeys.Add( condition );
			}

			if (primaryKeys.Count==0)
				throw new Exception("Cannot generate DELETE command for table without primary key");

			cmd.CommandText = String.Format(
				"DELETE FROM {0} WHERE {1}",
				table.TableName,
				String.Join(" AND ", primaryKeys.ToArray()) );

			return cmd;
		}
		
		/// <summary>
		/// Generates DELETE statement by query
		/// </summary>
		public virtual IDbCommand ComposeDelete(Query query) {
			var cmd = DbFactory.CreateCommand();
			var dbSqlBuilder = DbFactory.CreateSqlBuilder(cmd);

			// prepare WHERE part
			var whereExpression = dbSqlBuilder.BuildExpression( query.Condition );
			
			cmd.CommandText = String.Format("DELETE FROM {0}",query.SourceName);
			if (whereExpression!=null && whereExpression.Length>0)
				cmd.CommandText += " WHERE "+whereExpression;

			return cmd;
		}		
		
		
		/// <summary>
		/// Generates UPDATE statement by DataTable
		/// </summary>
		public virtual IDbCommand ComposeUpdate(DataTable table) {
			var cmd = DbFactory.CreateCommand();
			var dbSqlBuilder = DbFactory.CreateSqlBuilder(cmd);
			
			// prepare fields Part
			var updateFieldNames = new List<string>();
			var updateFieldValues = new List<string>();
			foreach (DataColumn col in table.Columns) 
				if (!col.AutoIncrement) {
					updateFieldNames.Add( col.ColumnName );
					updateFieldValues.Add( 
						dbSqlBuilder.BuildCommandParameter( col, DataRowVersion.Current ) );
				}
			string updateExpression = BuildSetExpression(dbSqlBuilder,
				updateFieldNames.ToArray(), updateFieldValues.ToArray() );
			
			// prepare WHERE part
			var primaryKeyGroup = new QueryGroupNode(GroupType.And);
			foreach (DataColumn col in table.PrimaryKey) {
				string valueParameterName = dbSqlBuilder.BuildCommandParameter(
					col, DataRowVersion.Original);
				primaryKeyGroup.Nodes.Add(
					(QField)col.ColumnName==new QRawSql(valueParameterName) );
			}
			
			if (primaryKeyGroup.Nodes.Count==0)
				throw new Exception("Cannot generate UPDATE command for table without primary key");
			
			var whereExpression = dbSqlBuilder.BuildExpression(primaryKeyGroup);
			cmd.CommandText = String.Format(
				"UPDATE {0} SET {1} WHERE {2}",
				table.TableName, updateExpression, whereExpression );
				
			return cmd;			
		}
		
		/// <summary>
		/// Create UPDATE command by changes data and query
		/// </summary>
		/// <param name="changesData"></param>
		/// <param name="query"></param>
		/// <returns></returns>
		public virtual IDbCommand ComposeUpdate(IDictionary changesData, Query query) {
			var cmd = DbFactory.CreateCommand();
			var dbSqlBuilder = DbFactory.CreateSqlBuilder(cmd);

			// prepare fields Part
			var updateFieldNames = new List<string>();
			var updateFieldValues = new List<string>();
			foreach (object field in changesData.Keys) {
				updateFieldNames.Add( Convert.ToString( field ) );
				if (changesData[field] is QRawSql) {
					// QRawConst can be used for specifying SQL-specific update
					updateFieldValues.Add( ((QRawSql)changesData[field]).SqlText );
				} else {
					updateFieldValues.Add( dbSqlBuilder.BuildCommandParameter( changesData[field] ) ); 
				}
			}
			string setExpression = BuildSetExpression(dbSqlBuilder,
				updateFieldNames.ToArray(), updateFieldValues.ToArray() );
			
			// prepare WHERE part
			string whereExpression = dbSqlBuilder.BuildExpression( query.Condition );
			
			cmd.CommandText = String.Format(
				"UPDATE {0} SET {1}",
				query.SourceName, setExpression);
			if (whereExpression!=null)
				cmd.CommandText += " WHERE "+whereExpression;
			
			return cmd;
		}



		/// <summary>
		/// Generates INSERT statement by row data
		/// </summary>
		public virtual IDbCommand ComposeInsert(IDictionary data, string sourceName) {
			var cmd = DbFactory.CreateCommand();
			var dbSqlBuilder = DbFactory.CreateSqlBuilder(cmd);
			
			// Prepare fields part
			var insertFields = new List<string>();
			var insertValues = new List<string>();
			foreach (object field in data.Keys) {
				insertFields.Add( Convert.ToString( field ) );
				insertValues.Add( dbSqlBuilder.BuildCommandParameter( data[field] ) );
			}
			
			cmd.CommandText = String.Format(
				"INSERT INTO {0} ({1}) VALUES ({2})",
				sourceName,
				String.Join(",", insertFields.ToArray()) ,
				String.Join(",", insertValues.ToArray()) );
			
			return cmd;
		}

		protected virtual string BuildSetExpression(IDbSqlBuilder sqlBuilder, string[] fieldNames, string[] fieldValues) {
			if (fieldNames.Length != fieldValues.Length)
				throw new ArgumentException();
			var parts = new List<string>();
			for (int i = 0; i < fieldNames.Length; i++) {
				string condition = String.Format("{0}={1}",
					sqlBuilder.BuildValue(new QField(fieldNames[i])), fieldValues[i]);
				parts.Add(condition);
			}
			return String.Join(",", parts.ToArray());
		}

		

	}
}
