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
using System.Data.Common;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;


namespace NI.Data {

	/// <summary>
	/// Database Data Access Layer Component
	/// </summary>
	public class DbDalc : ISqlDalc {

		IDbConnection _Connection;
		IDbTransaction _Transaction;
		IDbCommandGenerator _CommandGenerator;
		IDbDataAdapterWrapperFactory _AdapterWrapperFactory;
		IDbDalcEventsMediator _DbDalcEventsMediator;
		
		Hashtable adapterCache = new Hashtable();
		
		/// <summary>
		/// Get or set database commands generator
		/// </summary>
		public IDbCommandGenerator CommandGenerator {
			get { return _CommandGenerator; }
			set { _CommandGenerator = value; }
		}
		
		/// <summary>
		/// Get or set adapter wrapper factory component
		/// </summary>
		public IDbDataAdapterWrapperFactory AdapterWrapperFactory {
			get { return _AdapterWrapperFactory; }
			set { _AdapterWrapperFactory = value;	}
		}
		
		/// <summary>
		/// Get or set database connection
		/// </summary>
		public virtual IDbConnection Connection {
			get { return _Connection; }
			set { _Connection = value; }
		}

		/// <summary>
		/// Database transaction object
		/// </summary>
		public virtual IDbTransaction Transaction {
			get {
				return _Transaction;
			}
			set { _Transaction = value; }
		}
		
		public IDbDalcEventsMediator DbDalcEventsMediator {
			get { return _DbDalcEventsMediator; }
			set { _DbDalcEventsMediator = value; }
		}

		/// <summary>
		/// Initializes a new instance of the DbDalc class.
		/// Note: Connection and CommandGenerator should be initialized
		/// before using this component.
		/// </summary>
		public DbDalc() {
		}
		
		/// <summary>
		/// Load data to dataset by query
		/// </summary>
		public virtual DataTable Load(Query query, DataSet ds) {
			IDbCommandWrapper selectCmdWrapper = CommandGenerator.ComposeSelect( query );
			QSourceName source = new QSourceName(query.SourceName);

			selectCmdWrapper.Command.Connection = Connection;
			selectCmdWrapper.SetTransaction(Transaction);

			IDbDataAdapterWrapper adapterWrapper = AdapterWrapperFactory.CreateInstance();
			adapterWrapper.RowUpdating += new DbRowUpdatingEventHandler(this.OnRowUpdating);
			adapterWrapper.RowUpdated += new DbRowUpdatedEventHandler(this.OnRowUpdated);

			adapterWrapper.SelectCommadWrapper = selectCmdWrapper;
			
			OnCommandExecuting(source.Name, StatementType.Select, selectCmdWrapper.Command);
			if (adapterWrapper.Adapter is DbDataAdapter)
				((DbDataAdapter)adapterWrapper.Adapter).Fill(ds, query.StartRecord, query.RecordCount, source.Name);
			else
				adapterWrapper.Adapter.Fill( ds );
			OnCommandExecuted(source.Name, StatementType.Select, selectCmdWrapper.Command);

			return ds.Tables[source.Name];
		}

		/// <summary>
		/// Delete data by query
		/// </summary>
		public virtual int Delete(Query query) {
			IDbCommandWrapper deleteCmd = CommandGenerator.ComposeDelete(query);
			return ExecuteInternal(deleteCmd, query.SourceName, StatementType.Delete);
		}


		
		/// <summary>
		/// Update one table in DataSet
		/// </summary>
		public virtual void Update(DataTable t) {
			var tableName = t.TableName;

			IDbDataAdapterWrapper adapterWrapper = adapterCache[tableName] as IDbDataAdapterWrapper;
			if (adapterWrapper==null) {
				adapterWrapper = AdapterWrapperFactory.CreateInstance();
				adapterWrapper.RowUpdating += new DbRowUpdatingEventHandler(this.OnRowUpdating);
				adapterWrapper.RowUpdated += new DbRowUpdatedEventHandler(this.OnRowUpdated);
				
				GenerateAdapterCommands( adapterWrapper, t);
				
				adapterCache[tableName] = adapterWrapper;
			}
			
			adapterWrapper.Adapter.InsertCommand.Connection = Connection;
			adapterWrapper.InsertCommandWrapper.SetTransaction( Transaction );
			
			adapterWrapper.Adapter.UpdateCommand.Connection = Connection;
			adapterWrapper.UpdateCommandWrapper.SetTransaction( Transaction );
			
			adapterWrapper.Adapter.DeleteCommand.Connection = Connection;
			adapterWrapper.DeleteCommandWrapper.SetTransaction( Transaction );
			
			if (adapterWrapper.Adapter is DbDataAdapter)
				((DbDataAdapter)adapterWrapper.Adapter).Update(t.DataSet, tableName);
			else
				adapterWrapper.Adapter.Update(t.DataSet);
		}
		
		/// <summary>
		/// Update data from dictionary container to datasource by query
		/// </summary>
		/// <param name="data">Container with record changes</param>
		/// <param name="query">query</param>
		public virtual int Update(Query query, IDictionary data) {
			IDbCommandWrapper cmdWrapper = CommandGenerator.ComposeUpdate(data, query);
			cmdWrapper.Command.Connection = Connection;
			cmdWrapper.SetTransaction( Transaction );
			
			return ExecuteInternal( cmdWrapper, query.SourceName, StatementType.Update );
		}

		/// <summary>
		/// <see cref="IDalc.Insert"/>
		/// </summary>
		public virtual void Insert(string sourceName, IDictionary data) {
			IDbCommandWrapper cmdWrapper = CommandGenerator.ComposeInsert(data, sourceName);
			cmdWrapper.Command.Connection = Connection;
			cmdWrapper.SetTransaction( Transaction );
			
			ExecuteInternal( cmdWrapper, sourceName, StatementType.Insert );
		}
		
		
		/// <summary>
		/// Execute SQL command
		/// </summary>
		/// <param name="sqlText">SQL command text to execute</param>
		/// <returns>number of rows affected</returns>
		public virtual int ExecuteNonQuery(string sqlText) {
			IDbCommandWrapper cmdWrapper = CommandGenerator.CommandWrapperFactory.CreateInstance();
			cmdWrapper.Command.Connection = Connection;
			cmdWrapper.SetTransaction( Transaction );
			cmdWrapper.Command.CommandText = sqlText;
			return ExecuteInternal(cmdWrapper, null, StatementType.Update);
		}

        protected virtual void ExecuteReaderInternal(IDbCommandWrapper cmdWrapper, string sourceName, Action<IDataReader> callback) {
            bool closeConnection = false;
            if (Connection.State != ConnectionState.Open) {
                Connection.Open();
                closeConnection = true;
            }
            try {
                OnCommandExecuting(sourceName, StatementType.Select, cmdWrapper.Command);
                IDataReader rdr = cmdWrapper.Command.ExecuteReader();
                OnCommandExecuted(sourceName, StatementType.Select, cmdWrapper.Command);

                callback(rdr);

                if (!rdr.IsClosed)
                    rdr.Close();
            } finally {
                // close only if was opened
                if (closeConnection)
                    Connection.Close();
            }
        }

        /// <summary>
        /// Load data into datareader by custom SQL
        /// </summary>
		public virtual void ExecuteReader(string sqlText, Action<IDataReader> callback) {
   			IDbCommandWrapper cmdWrapper = CommandGenerator.CommandWrapperFactory.CreateInstance();
			cmdWrapper.Command.Connection = Connection;
			cmdWrapper.SetTransaction(Transaction);
			cmdWrapper.Command.CommandText = sqlText;

            ExecuteReaderInternal(cmdWrapper, null, callback);
		}

        /// <summary>
        /// Load data into datareader by query
        /// </summary>
        public virtual void ExecuteReader(Query q, Action<IDataReader> callback) {
            IDbCommandWrapper cmdWrapper = CommandGenerator.ComposeSelect(q);
            cmdWrapper.SetTransaction(Transaction);
            cmdWrapper.Command.Connection = Connection;

            ExecuteReaderInternal(cmdWrapper, q.SourceName, callback);
        }
		
        /// <summary>
        /// Load data into dataset by custom SQL
        /// </summary>
        public virtual void Load(string sqlText, DataSet ds) {
			IDbCommandWrapper cmdWrapper = CommandGenerator.CommandWrapperFactory.CreateInstance();
			cmdWrapper.Command.Connection = Connection;
			cmdWrapper.SetTransaction( Transaction );
			cmdWrapper.Command.CommandText = sqlText;
			
			IDbDataAdapterWrapper adapterWrapper = AdapterWrapperFactory.CreateInstance();
			adapterWrapper.SelectCommadWrapper = cmdWrapper;			
			
			OnCommandExecuting(null, StatementType.Select, cmdWrapper.Command);
			adapterWrapper.Adapter.Fill( ds );
			OnCommandExecuted(null, StatementType.Select, cmdWrapper.Command);
		}
		
		

#region Internal methods

		protected virtual void OnCommandExecuting(string sourceName, StatementType type, IDbCommand cmd) {
			if (DbDalcEventsMediator!=null)
				DbDalcEventsMediator.OnCommandExecuting(this, new DbCommandEventArgs(sourceName, type, cmd) );
		}
		
		protected virtual void OnCommandExecuted(string sourceName, StatementType type, IDbCommand cmd) {
			if (DbDalcEventsMediator!=null)
				DbDalcEventsMediator.OnCommandExecuted(this, new DbCommandEventArgs(sourceName, type, cmd) );
		}

		/// <summary>
		/// This method should be called before row updating
		/// </summary>
		protected virtual void OnRowUpdating(object sender, RowUpdatingEventArgs e) {
			//Trace.WriteLine( e.Command.CommandText, "SQL" );
			OnCommandExecuting(e.Row.Table.TableName, StatementType.Update, e.Command);
			if (DbDalcEventsMediator!=null)
				DbDalcEventsMediator.OnRowUpdating(this, e);
		}
		
		/// <summary>
		/// This method should be called after row updated
		/// </summary>
		protected virtual void OnRowUpdated(object sender, RowUpdatedEventArgs e) {
			if (e.StatementType == StatementType.Insert) {
				// extract insert id
				object insertId = ((IDbDataAdapterWrapper)sender).InsertCommandWrapper.GetInsertId();
				
				if (insertId!=null && insertId!=DBNull.Value)
					foreach (DataColumn col in e.Row.Table.Columns)
						if (col.AutoIncrement) {
							bool readOnly = col.ReadOnly;
							try {
								col.ReadOnly = false;
								e.Row[col] = insertId;
							} finally {
								col.ReadOnly = readOnly;
							}
							break;
						}
			}
			
			if (DbDalcEventsMediator!=null)
				DbDalcEventsMediator.OnRowUpdated(this, e);
			OnCommandExecuted(e.Row.Table.TableName, StatementType.Update, e.Command);
		}

		/// <summary>
		/// Automatically generates Insert/Update/Delete commands for Adapter
		/// </summary>
		protected virtual void GenerateAdapterCommands(IDbDataAdapterWrapper adapterWrapper, DataTable table) {
			// Init DataAdapter
			adapterWrapper.UpdateCommandWrapper = CommandGenerator.ComposeUpdate(table);
			adapterWrapper.InsertCommandWrapper = CommandGenerator.ComposeInsert(table);
			adapterWrapper.DeleteCommandWrapper = CommandGenerator.ComposeDelete(table);
		}
		
		/// <summary>
		/// Execute SQL command
		/// </summary>
		virtual protected int ExecuteInternal(IDbCommandWrapper cmdWrapper, string sourceName, StatementType commandType) {
			cmdWrapper.SetTransaction( Transaction );
			cmdWrapper.Command.Connection = Connection;
			
			//Trace.WriteLine( cmdWrapper.Command.CommandText, "SQL" );

			bool closeConn = false;
			if (cmdWrapper.Command.Connection.State!=ConnectionState.Open) {
				cmdWrapper.Command.Connection.Open();
				closeConn = true;
			}
			
			int res = 0;
			try {
				OnCommandExecuting(sourceName, commandType, cmdWrapper.Command);
				res = cmdWrapper.Command.ExecuteNonQuery();
				OnCommandExecuted(sourceName, commandType, cmdWrapper.Command);
			} finally {
				if (closeConn) cmdWrapper.Command.Connection.Close();
			}
			
			return res;
		}		
		
		virtual protected bool LoadRecordInternal(IDictionary data, IDbCommandWrapper cmdWrapper, string sourceName) {

            cmdWrapper.SetTransaction( Transaction );
			cmdWrapper.Command.Connection = Connection;
			
			bool closeConn = false;
			if (cmdWrapper.Command.Connection.State!=ConnectionState.Open) {
				cmdWrapper.Command.Connection.Open();
				closeConn = true;
			}

			//Trace.WriteLine( cmdWrapper.Command.CommandText, "SQL" );
			
			IDataReader reader = null;
			try {
				OnCommandExecuting(sourceName, StatementType.Select, cmdWrapper.Command);
				reader = cmdWrapper.Command.ExecuteReader( System.Data.CommandBehavior.SingleRow );
				OnCommandExecuted(sourceName, StatementType.Select, cmdWrapper.Command);

				if (reader.Read()) {
					// fetch all fields & values in hashtable
					for (int i=0; i<reader.FieldCount; i++)
						data[ reader.GetName(i) ] = reader.GetValue(i);
						
					return true;
				}
				
				return false;
			} finally {
				if (closeConn) cmdWrapper.Command.Connection.Close();
				if (reader!=null) reader.Close();
			}
		}
		
	
#endregion				
	

	}
	
	
}
