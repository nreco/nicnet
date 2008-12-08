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
using System.Data.Common;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

using NI.Common;

namespace NI.Data.Dalc {

	/// <summary>
	/// Database Data Access Layer Component
	/// </summary>
	public class DbDalc : NI.Common.Component, IDbDalc {

		IDbConnection _Connection;
		IDbTransaction _Transaction;
		IDbCommandGenerator _CommandGenerator;
		IDbDataAdapterWrapperFactory _AdapterWrapperFactory;
		IDbDalcEventsMediator _DbDalcEventsMediator;
		
		Hashtable adapterCache = new Hashtable();
		
		/// <summary>
		/// Get or set database commands generator
		/// </summary>
		[Dependency]
		public IDbCommandGenerator CommandGenerator {
			get { return _CommandGenerator; }
			set { _CommandGenerator = value; }
		}
		
		/// <summary>
		/// Get or set adapter wrapper factory component
		/// </summary>
		[Dependency]
		public IDbDataAdapterWrapperFactory AdapterWrapperFactory {
			get { return _AdapterWrapperFactory; }
			set { _AdapterWrapperFactory = value;	}
		}
		
		/// <summary>
		/// Get or set database connection
		/// </summary>
		[Dependency]
		public virtual IDbConnection Connection {
			get { return _Connection; }
			set { _Connection = value; }
		}

		/// <summary>
		/// Database transaction object
		/// </summary>
		[Dependency(Required=false)]
		public virtual IDbTransaction Transaction {
			get {
				return _Transaction;
			}
			set { _Transaction = value; }
		}
		
		[Dependency(Required=false)]
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
		virtual public void Load(DataSet ds, IQuery query) {
			IDbCommandWrapper selectCmdWrapper = CommandGenerator.ComposeSelect( query );
			
			selectCmdWrapper.Command.Connection = Connection;
			selectCmdWrapper.SetTransaction(Transaction);

			IDbDataAdapterWrapper adapterWrapper = AdapterWrapperFactory.CreateInstance();
			adapterWrapper.RowUpdating += new DbRowUpdatingEventHandler(this.OnRowUpdating);
			adapterWrapper.RowUpdated += new DbRowUpdatedEventHandler(this.OnRowUpdated);

			adapterWrapper.SelectCommadWrapper = selectCmdWrapper;
			
			OnCommandExecuting(query.SourceName, StatementType.Select, selectCmdWrapper.Command);
			if (adapterWrapper.Adapter is DbDataAdapter)
				((DbDataAdapter)adapterWrapper.Adapter).Fill(ds, query.StartRecord, query.RecordCount, query.SourceName);
			else
				adapterWrapper.Adapter.Fill( ds );
			OnCommandExecuted(query.SourceName, StatementType.Select, selectCmdWrapper.Command);
		}

		/// <summary>
		/// Delete data by query
		/// </summary>
		virtual public int Delete(IQuery query) {
			IDbCommandWrapper deleteCmd = CommandGenerator.ComposeDelete(query);
			return ExecuteInternal(deleteCmd, query.SourceName, StatementType.Delete);
		}


		
		/// <summary>
		/// Update one table in DataSet
		/// Note: method 'AcceptChanges' will not called automatically
		/// </summary>
		virtual public void Update(DataSet ds, string tableName) {
			
			IDbDataAdapterWrapper adapterWrapper = adapterCache[tableName] as IDbDataAdapterWrapper;
			if (adapterWrapper==null) {
				adapterWrapper = AdapterWrapperFactory.CreateInstance();
				adapterWrapper.RowUpdating += new DbRowUpdatingEventHandler(this.OnRowUpdating);
				adapterWrapper.RowUpdated += new DbRowUpdatedEventHandler(this.OnRowUpdated);
				
				GenerateAdapterCommands( adapterWrapper, ds.Tables[tableName]);
				
				adapterCache[tableName] = adapterWrapper;
			}
			
			adapterWrapper.Adapter.InsertCommand.Connection = Connection;
			adapterWrapper.InsertCommandWrapper.SetTransaction( Transaction );
			
			adapterWrapper.Adapter.UpdateCommand.Connection = Connection;
			adapterWrapper.UpdateCommandWrapper.SetTransaction( Transaction );
			
			adapterWrapper.Adapter.DeleteCommand.Connection = Connection;
			adapterWrapper.DeleteCommandWrapper.SetTransaction( Transaction );
			
			if (adapterWrapper.Adapter is DbDataAdapter)
				((DbDataAdapter)adapterWrapper.Adapter).Update(ds, tableName);
			else
				adapterWrapper.Adapter.Update(ds);
		}
		
		/// <summary>
		/// Update data from dictionary container to datasource by query
		/// </summary>
		/// <param name="data">Container with record changes</param>
		/// <param name="query">query</param>
		virtual public int Update(IDictionary data, IQuery query) {
			IDbCommandWrapper cmdWrapper = CommandGenerator.ComposeUpdate(data, query);
			cmdWrapper.Command.Connection = Connection;
			cmdWrapper.SetTransaction( Transaction );
			
			return ExecuteInternal( cmdWrapper, query.SourceName, StatementType.Update );
		}

		/// <summary>
		/// <see cref="IDalc.Insert"/>
		/// </summary>
		virtual public void Insert(IDictionary data, string sourceName) {
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
		public virtual int Execute(string sqlText) {
			IDbCommandWrapper cmdWrapper = CommandGenerator.CommandWrapperFactory.CreateInstance();
			cmdWrapper.Command.Connection = Connection;
			cmdWrapper.SetTransaction( Transaction );
			cmdWrapper.Command.CommandText = sqlText;
			return ExecuteInternal(cmdWrapper, null, StatementType.Update);
		}
		
		/// <summary>
		/// <see cref="IDbDalc.Execute"/>
		/// </summary>
		public IDataReader ExecuteReader(string sqlText) {
			IDbCommandWrapper cmdWrapper = CommandGenerator.CommandWrapperFactory.CreateInstance();
			cmdWrapper.Command.Connection = Connection;
			cmdWrapper.SetTransaction( Transaction );
			cmdWrapper.Command.CommandText = sqlText;
			
			OnCommandExecuting(null, StatementType.Select, cmdWrapper.Command);
			IDataReader rdr = cmdWrapper.Command.ExecuteReader();
			OnCommandExecuted(null, StatementType.Select, cmdWrapper.Command);

			return rdr;
		}
		
		/// <summary>
		/// <see cref="IDbDalc.Execute"/>
		/// </summary>
		public IDataReader LoadReader(IQuery q) {
			IDbCommandWrapper cmdWrapper = CommandGenerator.ComposeSelect(q);
			cmdWrapper.SetTransaction( Transaction );
			cmdWrapper.Command.Connection = Connection;
			
			OnCommandExecuting(q.SourceName, StatementType.Select, cmdWrapper.Command);
			IDataReader rdr = cmdWrapper.Command.ExecuteReader();
			OnCommandExecuted(q.SourceName, StatementType.Select, cmdWrapper.Command);

			return rdr;
		}
		
		/// <summary>
		/// <see cref="IDbDalc.Load"/>
		/// </summary>
		public virtual void Load(DataSet ds, string sqlText) {
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

		/// <summary>
		/// Load single record into hashtable
		/// </summary>
		/// <param name="tablename">Table's name</param>
		/// <param name="filter">Record filter</param>
		/// <returns>Hashtable with 'field'=>'value' pairs (or null if no data)</returns>
		virtual public bool LoadRecord(IDictionary data, IQuery query) {
			// allow command generator build optimized sql select by specifying only one record to load
			Query oneRecordQuery = new Query(query);
			oneRecordQuery.RecordCount = 1;

			IDbCommandWrapper cmdWrapper = CommandGenerator.ComposeSelect( oneRecordQuery );
			return LoadRecordInternal(data, cmdWrapper, query.SourceName);
		}
		
		/// <summary>
		/// Load single record into hashtable
		/// </summary>
		/// <param name="sqlCommandText">SQL command text to execute</param>
		/// <returns>Hashtable with 'field'=>'value' pairs (or null if no data)</returns>
		virtual public bool LoadRecord(IDictionary data, string sqlCommandText) {
			IDbCommandWrapper cmdWrapper = CommandGenerator.CommandWrapperFactory.CreateInstance();
			cmdWrapper.Command.CommandText = sqlCommandText;
			return LoadRecordInternal(data, cmdWrapper, null);			
		}

		virtual public int RecordsCount(string sourceName, IQueryNode conditions) {
			Query q = new Query(sourceName, conditions);
			q.Fields = new string[] {"count(*)"}; // standart sql
			ListDictionary res = new ListDictionary();
			if (LoadRecord(res, q)) {
                foreach (object value in res.Values)
                {
                    try
                    {
                        return Convert.ToInt32(value);
                    }
                    // CHECK: some very strange bug here: sometimes 'value' cannot be cast to int (???)
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Count query returned incorrect value that cannot be casted to int, returning 0 instead. Exception: " + ex.ToString());
                        return 0;
                    }
                }
			}
			throw new Exception(); //TODO: add structured exception here
		}
		
		

#region Internal methods

		protected virtual void OnCommandExecuting(string sourceName, StatementType type, IDbCommand cmd) {
			if (DbDalcEventsMediator!=null)
				DbDalcEventsMediator.OnCommandExecuting( new DbCommandEventArgs(sourceName, type, cmd) );
		}
		
		protected virtual void OnCommandExecuted(string sourceName, StatementType type, IDbCommand cmd) {
			if (DbDalcEventsMediator!=null)
				DbDalcEventsMediator.OnCommandExecuted( new DbCommandEventArgs(sourceName, type, cmd) );
		}

		/// <summary>
		/// This method should be called before row updating
		/// </summary>
		protected virtual void OnRowUpdating(object sender, RowUpdatingEventArgs e) {
			//Trace.WriteLine( e.Command.CommandText, "SQL" );
			OnCommandExecuting(e.Row.Table.TableName, StatementType.Update, e.Command);
			if (DbDalcEventsMediator!=null)
				DbDalcEventsMediator.OnRowUpdating(e);
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
				DbDalcEventsMediator.OnRowUpdated(e);
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
