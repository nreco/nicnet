using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using NI.Data;
using NI.Data.Storage.Model;

namespace NI.Data.Storage {
	
	/// <summary>
	/// <see cref="IDataSetFactory"/> implementation based on DataSchema. 
	/// </summary>
	public class SchemaDataSetFactory : IDataSetFactory {

		protected Func<DataSchema> GetSchema { get; set; }

		/// <summary>
		/// Initializes new instance of SchemaDataSetFactory with specified DataSchema provider.
		/// </summary>
		/// <param name="getSchema">delegate that returns DataSchema structure</param>
		public SchemaDataSetFactory(Func<DataSchema> getSchema) {
			GetSchema = getSchema;
		}

		/// <summary>
		/// Construct DataSet object with DataTable schema for specifed table name.
		/// </summary>
		/// <param name="tableName">table name</param>
		/// <returns>DataSet with DataTable for specified table name</returns>
		public DataSet GetDataSet(string tableName) {
			if (String.IsNullOrEmpty(tableName))
				throw new ArgumentNullException("tableName is empty");	
			var schema = GetSchema();
			var ds = new DataSet();
			var dataClass = schema.FindClassByID(tableName);
			if (dataClass==null)
				return null;

			var tbl = dataClass.CreateDataTable();
			ds.Tables.Add(tbl);
			return ds;
		}
	}
}
