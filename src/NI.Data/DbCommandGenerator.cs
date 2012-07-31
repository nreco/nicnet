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
using System.Data;
using System.Diagnostics;
using System.ComponentModel;

using NI.Common;

namespace NI.Data
{
	/// <summary>
	/// Database Command Generator
	/// </summary>
	public class DbCommandGenerator : NI.Common.Component, IDbCommandGenerator
	{
		private IDbCommandWrapperFactory _CommandWrapperFactory = null;

		/// <summary>
		/// DB Command Wrapper Factory
		/// </summary>
		public IDbCommandWrapperFactory CommandWrapperFactory { 
			get { return _CommandWrapperFactory; }
			set { _CommandWrapperFactory = value; }
		}
		
		/// <summary>
		/// Initializes a new instance of the DbCommandGenerator class.
		/// Note: DbFactory property should be set before using this component.
		/// </summary>
		public DbCommandGenerator() {
		}
		
		/// <summary>
		/// Initializes a new instance of the DbCommandGenerator class.
		/// </summary>
		/// <param name="dbFactory">IDbFactory implementation</param>
		public DbCommandGenerator(IDbCommandWrapperFactory commandWrapperFactory) {
			CommandWrapperFactory = commandWrapperFactory;
		}
		
		/// <summary>
		/// </summary>
		public virtual IDbCommandWrapper ComposeSelect(IQuery query) {
			IDbCommandWrapper cmdWrapper = CommandWrapperFactory.CreateInstance();
			IDbSqlBuilder dbSqlBuilder = cmdWrapper.CreateSqlBuilder();
			cmdWrapper.Command.CommandText = dbSqlBuilder.BuildSelect(query);
			return cmdWrapper;
		}



		
		/// <summary>
		/// Generates INSERT statement by DataTable
		/// </summary>
		public virtual IDbCommandWrapper ComposeInsert(DataTable table) {
			IDbCommandWrapper cmdWrapper = CommandWrapperFactory.CreateInstance();
			IDbSqlBuilder dbSqlBuilder = cmdWrapper.CreateSqlBuilder();
			
			// Prepare fields part
			ArrayList insertFields = new ArrayList();
			ArrayList insertValues = new ArrayList();
			foreach (DataColumn col in table.Columns)
				if (!col.AutoIncrement || false) {
					insertFields.Add(col.ColumnName);
					insertValues.Add( dbSqlBuilder.BuildCommandParameter( col, DataRowVersion.Current ) );
				}
				
			cmdWrapper.Command.CommandText = String.Format(
				"INSERT INTO {0} ({1}) VALUES ({2})",
				table.TableName,
				String.Join(",", (string[]) insertFields.ToArray(typeof(string))) ,
				String.Join(",", (string[]) insertValues.ToArray(typeof(string))) );
			
			return cmdWrapper;
		}
		
		/// <summary>
		/// Generates DELETE statement by DataTable
		/// </summary>
		public virtual IDbCommandWrapper ComposeDelete(DataTable table) {
			IDbCommandWrapper cmdWrapper = CommandWrapperFactory.CreateInstance();
			IDbSqlBuilder dbSqlBuilder = cmdWrapper.CreateSqlBuilder();

			// prepare WHERE part
			ArrayList primaryKeys = new ArrayList();
			foreach (DataColumn col in table.PrimaryKey) {
				string condition = String.Format(
						"{0}={1}",
						col.ColumnName,
						dbSqlBuilder.BuildCommandParameter( col, DataRowVersion.Current ) );
				primaryKeys.Add( condition );
			}

			if (primaryKeys.Count==0)
				throw new Exception("Cannot generate DELETE command for table without primary key");

			cmdWrapper.Command.CommandText = String.Format(
				"DELETE FROM {0} WHERE {1}",
				table.TableName,
				String.Join(" AND ", (string[]) primaryKeys.ToArray(typeof(string))) );

			return cmdWrapper;
		}
		
		/// <summary>
		/// Generates DELETE statement by query
		/// </summary>
		public virtual IDbCommandWrapper ComposeDelete(IQuery query) {
			IDbCommandWrapper cmdWrapper = CommandWrapperFactory.CreateInstance();
			IDbSqlBuilder dbSqlBuilder = cmdWrapper.CreateSqlBuilder();

			// prepare WHERE part
			string whereExpression = dbSqlBuilder.BuildExpression( query.Root );
			
			cmdWrapper.Command.CommandText = String.Format("DELETE FROM {0}",query.SourceName);
			if (whereExpression!=null && whereExpression.Length>0)
				cmdWrapper.Command.CommandText += " WHERE "+whereExpression;

			return cmdWrapper;
		}		
		
		
		/// <summary>
		/// Generates UPDATE statement by DataTable
		/// </summary>
		public virtual IDbCommandWrapper ComposeUpdate(DataTable table) {
			IDbCommandWrapper cmdWrapper = CommandWrapperFactory.CreateInstance();
			IDbSqlBuilder dbSqlBuilder = cmdWrapper.CreateSqlBuilder();
			
			// prepare fields Part
			ArrayList updateFieldNames = new ArrayList();
			ArrayList updateFieldValues = new ArrayList();
			foreach (DataColumn col in table.Columns) if (!col.AutoIncrement) {
				updateFieldNames.Add( col.ColumnName );
				updateFieldValues.Add( dbSqlBuilder.BuildCommandParameter(
					col, DataRowVersion.Current ) );
			}
			string updateExpression = dbSqlBuilder.BuildSetExpression(
				(string[])updateFieldNames.ToArray(typeof(string)),
				(string[])updateFieldValues.ToArray(typeof(string)) );
			
			// prepare WHERE part
			QueryGroupNode primaryKeyGroup = new QueryGroupNode(GroupType.And);
			foreach (DataColumn col in table.PrimaryKey) {
				string valueParameterName = dbSqlBuilder.BuildCommandParameter(
					col, DataRowVersion.Original);
				primaryKeyGroup.Nodes.Add(
					(QField)col.ColumnName==new QRawConst(valueParameterName) );
			}
			
			if (primaryKeyGroup.Nodes.Count==0)
				throw new Exception("Cannot generate UPDATE command for table without primary key");
			
			string whereExpression = dbSqlBuilder.BuildExpression(primaryKeyGroup);
			
			cmdWrapper.Command.CommandText = String.Format(
				"UPDATE {0} SET {1} WHERE {2}",
				table.TableName,
				updateExpression,
				whereExpression );
				
			return cmdWrapper;			
		}
		
		/// <summary>
		/// Create UPDATE command by changes data and query
		/// </summary>
		/// <param name="changesData"></param>
		/// <param name="query"></param>
		/// <returns></returns>
		public virtual IDbCommandWrapper ComposeUpdate(IDictionary changesData, IQuery query) {
			IDbCommandWrapper cmdWrapper = CommandWrapperFactory.CreateInstance();
			IDbSqlBuilder dbSqlBuilder = cmdWrapper.CreateSqlBuilder();

			// prepare fields Part
			ArrayList updateFieldNames = new ArrayList();
			ArrayList updateFieldValues = new ArrayList();
			foreach (object field in changesData.Keys) {
				updateFieldNames.Add( field );
				if (changesData[field] is QRawConst) {
					// QRawConst can be used for specifying SQL-specific update
					updateFieldValues.Add( ((QRawConst)changesData[field]).Value );
				} else {
					updateFieldValues.Add( dbSqlBuilder.BuildCommandParameter( changesData[field] ) ); 
				}
			}
			string setExpression = dbSqlBuilder.BuildSetExpression( 
				(string[])updateFieldNames.ToArray(typeof(string)),
				(string[])updateFieldValues.ToArray(typeof(string)) );
			
			// prepare WHERE part
			string whereExpression = dbSqlBuilder.BuildExpression( query.Root );
			
			cmdWrapper.Command.CommandText = String.Format(
				"UPDATE {0} SET {1}",
				query.SourceName, setExpression);
			if (whereExpression!=null)
				cmdWrapper.Command.CommandText += " WHERE "+whereExpression;
			
			return cmdWrapper;
		}



		/// <summary>
		/// Generates INSERT statement by row data
		/// </summary>
		public virtual IDbCommandWrapper ComposeInsert(IDictionary data, string sourceName) {
			IDbCommandWrapper cmdWrapper = CommandWrapperFactory.CreateInstance();
			IDbSqlBuilder dbSqlBuilder = cmdWrapper.CreateSqlBuilder();
			
			// Prepare fields part
			ArrayList insertFields = new ArrayList();
			ArrayList insertValues = new ArrayList();
			foreach (object field in data.Keys) {
				insertFields.Add( field );
				insertValues.Add( dbSqlBuilder.BuildCommandParameter( data[field] ) );
			}
			
			cmdWrapper.Command.CommandText = String.Format(
				"INSERT INTO {0} ({1}) VALUES ({2})",
				sourceName,
				String.Join(",", (string[]) insertFields.ToArray(typeof(string))) ,
				String.Join(",", (string[]) insertValues.ToArray(typeof(string))) );
			
			return cmdWrapper;
		}

		

	}
}
