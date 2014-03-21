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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;

namespace NI.Data
{
	/// <summary>
	/// Database Command Generator that supports data views
	/// </summary>
	public class DbCommandGenerator : IDbCommandGenerator
	{

		/// <summary>
		/// DB Factory instance
		/// </summary>
		protected IDbProviderFactory DbFactory {  get; set; }

		/// <summary>
		/// Dalc views
		/// </summary>
		public IDbDalcView[] Views {
			get; set;
		}

		/// <summary>
		/// Initializes a new instance of the DbCommandGenerator class with specified IDbDalcFactory component.
		/// </summary>
		/// <param name="dbFactory">IDbDalcFactory implementation</param>
		public DbCommandGenerator(IDbProviderFactory dbFactory) {
			DbFactory = dbFactory;
		}

		/// <summary>
		/// Initializes a new instance of the DbCommandGenerator class with specified IDbDalcFactory component and list of data views.
		/// </summary>
		/// <param name="dbFactory">IDbDalcFactory implementation</param>
		/// <param name="views">list of data views</param>
		public DbCommandGenerator(IDbProviderFactory dbFactory, IDbDalcView[] views) {
			DbFactory = dbFactory;
			Views = views;
		}

		protected virtual Query PrepareSelectQuery(Query q) {
			return q;
		}

		/// <see cref="NI.Data.IDbCommandGenerator.ComposeSelect(NI.Data.Query)"/>
		public virtual IDbCommand ComposeSelect(Query query) {
			var cmd = DbFactory.CreateCommand();
			var cmdSqlBuilder = DbFactory.CreateSqlBuilder(cmd);

			if (Views != null) {
				for (int i = 0; i < Views.Length; i++) {
					var view = Views[i];
					if (view.IsMatchTable(query.Table)) {
						cmd.CommandText = view.ComposeSelect(PrepareSelectQuery(query), cmdSqlBuilder);
						return cmd;
					}
				}
			}
			cmd.CommandText = cmdSqlBuilder.BuildSelect(PrepareSelectQuery(query));
			return cmd;
		}

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

		public virtual IDbCommand ComposeDelete(DataTable table) {
			if (table.PrimaryKey.Length == 0)
				throw new Exception("Cannot generate DELETE command for table without primary key");			
			
			var cmd = DbFactory.CreateCommand();
			var dbSqlBuilder = DbFactory.CreateSqlBuilder(cmd);

			// prepare WHERE part
			var pkCondition = ComposeDeleteCondition(table, dbSqlBuilder);
			var whereSql = dbSqlBuilder.BuildExpression(pkCondition);

			cmd.CommandText = String.Format(
				"DELETE FROM {0} WHERE {1}",
				table.TableName,
				whereSql);

			return cmd;
		}

		protected QueryNode ComposePkCondition(DataTable table, IDbSqlBuilder dbSqlBuilder, DataRowVersion rowValueVersion) {
			var pkCondition = new QueryGroupNode(QueryGroupNodeType.And);
			foreach (DataColumn col in table.PrimaryKey) {
				pkCondition.Nodes.Add(
					(QField)col.ColumnName == new QRawSql(dbSqlBuilder.BuildCommandParameter(col, rowValueVersion)) );
			}
			return pkCondition;
		}

		protected virtual QueryNode ComposeDeleteCondition(DataTable table, IDbSqlBuilder dbSqlBuilder) {
			return ComposePkCondition(table, dbSqlBuilder, DataRowVersion.Current);
		}

		protected virtual QueryNode ComposeDeleteCondition(Query query) {
			return query.Condition;
		}

		/// <see cref="NI.Data.IDbCommandGenerator.ComposeDelete(NI.Data.Query)"/>
		public virtual IDbCommand ComposeDelete(Query query) {
			var cmd = DbFactory.CreateCommand();
			var dbSqlBuilder = DbFactory.CreateSqlBuilder(cmd);

			// prepare WHERE part
			var whereExpression = dbSqlBuilder.BuildExpression( ComposeDeleteCondition( query ) );
			
			cmd.CommandText = String.Format("DELETE FROM {0}",query.Table);
			if (whereExpression!=null && whereExpression.Length>0)
				cmd.CommandText += " WHERE "+whereExpression;

			return cmd;
		}

		protected virtual QueryNode ComposeUpdateCondition(DataTable table, IDbSqlBuilder dbSqlBuilder) {
			return ComposePkCondition(table, dbSqlBuilder, DataRowVersion.Original);
		}

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
			var primaryKeyGroup = ComposeUpdateCondition(table, dbSqlBuilder);
			
			if (primaryKeyGroup.Nodes.Count==0)
				throw new Exception("Cannot generate UPDATE command for table without primary key");
			
			var whereExpression = dbSqlBuilder.BuildExpression(primaryKeyGroup);
			cmd.CommandText = String.Format(
				"UPDATE {0} SET {1} WHERE {2}",
				table.TableName, updateExpression, whereExpression );
				
			return cmd;			
		}

		protected virtual QueryNode ComposeUpdateCondition(Query query) {
			return query.Condition;
		}

		/// <see cref="NI.Data.IDbCommandGenerator.ComposeUpdate(NI.Data.Query,System.Collections.Generic.IDictionary<System.String,NI.Data.IQueryValue>)"/>
		public virtual IDbCommand ComposeUpdate(Query query, IDictionary<string, IQueryValue> changesData) {
			var cmd = DbFactory.CreateCommand();
			var dbSqlBuilder = DbFactory.CreateSqlBuilder(cmd);

			// prepare fields Part
			var updateFieldNames = new List<string>();
			var updateFieldValues = new List<string>();
			foreach (var setField in changesData) {
				updateFieldNames.Add(setField.Key);
				updateFieldValues.Add(dbSqlBuilder.BuildValue(setField.Value)); 
			}
			string setExpression = BuildSetExpression(dbSqlBuilder,
				updateFieldNames.ToArray(), updateFieldValues.ToArray() );
			
			// prepare WHERE part
			string whereExpression = dbSqlBuilder.BuildExpression( ComposeUpdateCondition( query ) );
			
			cmd.CommandText = String.Format(
				"UPDATE {0} SET {1}",
				query.Table, setExpression);
			if (whereExpression!=null)
				cmd.CommandText += " WHERE "+whereExpression;
			
			return cmd;
		}

		/// <see cref="NI.Data.IDbCommandGenerator.ComposeInsert(System.String,System.Collections.Generic.IDictionary<System.String,NI.Data.IQueryValue>)"/>
		public virtual IDbCommand ComposeInsert(string tableName, IDictionary<string, IQueryValue> data) {
			var cmd = DbFactory.CreateCommand();
			var dbSqlBuilder = DbFactory.CreateSqlBuilder(cmd);
			
			// Prepare fields part
			var insertFields = new List<string>();
			var insertValues = new List<string>();
			foreach (var setField in data) {
				insertFields.Add(setField.Key);
				insertValues.Add(dbSqlBuilder.BuildValue(setField.Value));
			}
			
			cmd.CommandText = String.Format(
				"INSERT INTO {0} ({1}) VALUES ({2})",
				tableName,
				String.Join(",", insertFields.ToArray()) ,
				String.Join(",", insertValues.ToArray()) );
			
			return cmd;
		}

		/// <see cref="NI.Data.IDbCommandGenerator.ComposeAdapterUpdateCommands(System.Data.IDbDataAdapter,System.Data.DataTable)"/>
		public void ComposeAdapterUpdateCommands(IDbDataAdapter adapter, DataTable table) {
			adapter.UpdateCommand = ComposeUpdate(table);
			adapter.InsertCommand = ComposeInsert(table);
			adapter.DeleteCommand = ComposeDelete(table);
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
