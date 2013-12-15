using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace NI.Data {
	
	/// <summary>
	/// Data transfer objects mapper that uses DataRowDalcMapper for storing object data
	/// </summary>
	public class ObjectDalcMapper<T> where T : class, new() {

		protected string SourceName { get; set; }

		protected IObjectDataRowMapper ObjectMapper { get; set; }

		protected DataRowDalcMapper DbManager { get; set; }

		/// <summary>
		/// Initializes a new instance of ObjectDalcMapper
		/// </summary>
		/// <param name="dbMgr">DataRowManager</param>
		/// <param name="sourceName">data source identifier</param>
		/// <param name="colNameToProperty">column name -> object property map</param>
		public ObjectDalcMapper(DataRowDalcMapper dbMgr, string sourceName, IDictionary<string, string> colNameToProperty) {
			SourceName = sourceName;
			ObjectMapper = new PropertyDataRowMapper(colNameToProperty);
			DbManager = dbMgr;
		}

		/// <summary>
		/// Initializes a new instance of ObjectDalcMapper with custom object properties mapper
		/// </summary>
		/// <param name="dbMgr"></param>
		/// <param name="sourceName"></param>
		/// <param name="customObjectMapper"></param>
		public ObjectDalcMapper(DataRowDalcMapper dbMgr, string sourceName, IObjectDataRowMapper customObjectMapper) {
			SourceName = sourceName;
			ObjectMapper = customObjectMapper;
			DbManager = dbMgr;
		}

		/// <summary>
		/// Load object by primary key 
		/// </summary>
		/// <param name="pk"></param>
		/// <returns>persisted object or null</returns>
		public T Load(params object[] pk) {
			var r = DbManager.Load(SourceName, pk);
			if (r==null) return null;
			var t = new T();
			CopyDataRowToObject(r, t);
			return t;
		}

		/// <summary>
		/// Load first object matched by query
		/// </summary>
		/// <param name="q">query</param>
		/// <returns>matched object with data or null</returns>
		public T Load(Query q) {
			var ds = new DataSet();
			var recordQ = new Query(q);
			q.StartRecord = 0;
			q.RecordCount = 1;
			var tbl = DbManager.Dalc.Load(recordQ, ds);
			if (tbl.Rows.Count == 0)
				return null;
			var t = new T();
			CopyDataRowToObject(tbl.Rows[0], t);
			return t;
		}

		/// <summary>
		/// Load all objects matched by query
		/// </summary>
		/// <param name="q">query</param>
		/// <returns>list of matched objects</returns>
		public IEnumerable<T> LoadAll(Query q) {
			var ds = new DataSet();
			DbManager.Dalc.Load(q, ds);
			var srcName = new QSource(q.SourceName);
			var rs = new List<T>();
			foreach (DataRow r in ds.Tables[srcName.Name].Rows) {
				var t = new T();
				CopyDataRowToObject(r, t);
				rs.Add(t);
			}
			return rs;
		}

		/// <summary>
		/// Add a new record associated with object
		/// </summary>
		/// <param name="o">object to add</param>
		public void Add(T o) {
			var ds = DbManager.CreateDataSet(SourceName);
			var r = ds.Tables[SourceName].NewRow();
			CopyObjectToDataRow(o, r, false);
			ds.Tables[SourceName].Rows.Add(r);
			DbManager.Update(r);
			CopyDataRowToObject(r, o);
		}

		/// <summary>
		/// Update record associated with object 
		/// </summary>
		/// <param name="o">object to update</param>
		/// <param name="createNew">create a new record if no associated records with specified object</param>
		public void Update(T o, bool createNew = false) {
			var r = DbManager.Load(new Query(SourceName, ComposePkCondition(o)));
			if (r == null) {
				if (createNew) {
					r = DbManager.Create(SourceName);
				} else {
					throw new DBConcurrencyException();
				}
			}
			CopyObjectToDataRow(o, r, r.RowState!=DataRowState.Added);
			DbManager.Update(r);
			CopyDataRowToObject(r, o);
		}

		/// <summary>
		/// Delete a record associated with object
		/// </summary>
		/// <param name="o"></param>
		public void Delete(T o) {
			DbManager.Delete(new Query(SourceName, ComposePkCondition(o)));
		}

		protected QueryNode ComposePkCondition(T t) {
			var qcnd = new QueryGroupNode(GroupType.And);
			var ds = DbManager.CreateDataSet(SourceName);
			foreach (DataColumn c in ds.Tables[SourceName].PrimaryKey) {
				var pVal = ObjectMapper.GetFieldValue(t, c);
				qcnd.Nodes.Add(new QueryConditionNode((QField)c.ColumnName, Conditions.Equal, new QConst( pVal )));
			}
			return qcnd;
		}

		protected void CopyObjectToDataRow(object o, DataRow r, bool ignorePk) {
			ObjectMapper.MapFrom(o, r, ignorePk);
		}

		protected void CopyDataRowToObject(DataRow r, object o) {
			ObjectMapper.MapTo(r,o);
		}

	}

}
