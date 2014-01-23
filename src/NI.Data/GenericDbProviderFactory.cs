using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace NI.Data {

	public class GenericDbProviderFactory : IDbProviderFactory {

		protected DbProviderFactory DbPrvFactory;

		public string ParamPlaceholderFormat { get; set; }

		public int CommandTimeout { get; set; }

		public GenericDbProviderFactory(DbProviderFactory dbProviderFactory) {
			DbPrvFactory = dbProviderFactory;
			CommandTimeout = -1;
		}

		public virtual IDbDataAdapter CreateDataAdapter(EventHandler<RowUpdatingEventArgs> onRowUpdating, EventHandler<RowUpdatedEventArgs> onRowUpdated) {
			var dataAdapter = DbPrvFactory.CreateDataAdapter();

			var rowUpdating = dataAdapter.GetType().GetEvent("RowUpdating");
			rowUpdating.AddEventHandler(dataAdapter, Delegate.CreateDelegate(rowUpdating.EventHandlerType, onRowUpdating.Target, onRowUpdating.Method) );

			var rowUpdated = dataAdapter.GetType().GetEvent("RowUpdated");
			rowUpdated.AddEventHandler(dataAdapter, Delegate.CreateDelegate(rowUpdated.EventHandlerType, onRowUpdated.Target, onRowUpdated.Method) );

			return dataAdapter;
		} 

		public virtual IDbCommand CreateCommand() {
			var cmd = DbPrvFactory.CreateCommand();
			if (CommandTimeout >= 0)
				cmd.CommandTimeout = CommandTimeout;
			return cmd;
		}

		public IDbConnection CreateConnection() {
			return DbPrvFactory.CreateConnection();
		}

		public string AddCommandParameter(IDbCommand cmd, object value) {
			var param = DbPrvFactory.CreateParameter();
			param.ParameterName = String.Format("@p{0}", cmd.Parameters.Count);
			param.Value = value ?? DBNull.Value;
			cmd.Parameters.Add(param);
			return GetCmdParameterPlaceholder(param.ParameterName);
		}

		public string AddCommandParameter(IDbCommand cmd, DataColumn column, DataRowVersion sourceVersion) {
			var param = DbPrvFactory.CreateParameter();
			param.ParameterName = String.Format("@p{0}", cmd.Parameters.Count);

			param.DbType = ResolveDbType(column.DataType);
			param.SourceColumn = column.ColumnName;
			param.IsNullable = column.AllowDBNull;
			param.SourceVersion = sourceVersion;

			cmd.Parameters.Add(param);
			return GetCmdParameterPlaceholder(param.ParameterName);
		}

		public virtual IDbSqlBuilder CreateSqlBuilder(IDbCommand dbCommand) {
			var dbSqlBuilder = new DbSqlBuilder(dbCommand, this);
			return dbSqlBuilder;
		}

		public virtual object GetInsertId(IDbConnection connection) {
			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException("GetInsertId requires opened connection");
			using (var cmd = CreateCommand()) {
				cmd.CommandText = "SELECT @@IDENTITY";
				cmd.Connection = connection;
				return cmd.ExecuteScalar();
			}
		}

		protected virtual string GetCmdParameterPlaceholder(string paramName) {
			if (ParamPlaceholderFormat == null)
				return paramName;
			return String.Format(ParamPlaceholderFormat, paramName);
		}

		public virtual DbType ResolveDbType(Type type) {
			if (type == typeof(byte))
				return DbType.Byte;
			if (type == typeof(bool))
				return DbType.Boolean;
			if (type == typeof(long))
				return DbType.Int64;
			if (type == typeof(int))
				return DbType.Int32;
			if (type == typeof(double))
				return DbType.Double;
			if (type == typeof(float))
				return DbType.Single;
			if (type == typeof(string))
				return DbType.String;
			if (type == typeof(byte[]))
				return DbType.Binary;
			if (type == typeof(DateTime))
				return DbType.DateTime;
			if (type == typeof(Guid))
				return DbType.Guid;
			if (type == typeof(Decimal))
				return DbType.Decimal;
			if (type == typeof(TimeSpan))
				return DbType.Time;
			if (type == typeof(DateTimeOffset))
				return DbType.DateTimeOffset;
			return DbType.Object;
		}


		public static GenericDbProviderFactory OleDb {
			get {
				return new GenericDbProviderFactory(System.Data.OleDb.OleDbFactory.Instance) {
					ParamPlaceholderFormat = "?"
				};
			}
		}

		public static GenericDbProviderFactory Odbc {
			get {
				return new GenericDbProviderFactory(System.Data.Odbc.OdbcFactory.Instance);
			}
		}

	}
}
