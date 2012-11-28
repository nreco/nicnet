using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace NI.Data {
	
	/// <summary>
	/// Simple plain object persister for typical CRUD operations
	/// </summary>
	public class ObjectPersister<T> where T : class, new() {

		protected string SourceName { get; set; }

		protected IObjectMapper ObjectMapper { get; set; }

		protected DalcManager DbManager { get; set; }


		public ObjectPersister(string sourceName, IDictionary<string, string> fieldToProperty, DalcManager dbMgr) {
			SourceName = sourceName;
			ObjectMapper = new PropertyObjectMapper(fieldToProperty);
			DbManager = dbMgr;
		}

		public ObjectPersister(string sourceName, IObjectMapper customObjectMapper, DalcManager dbMgr) {
			SourceName = sourceName;
			ObjectMapper = customObjectMapper;
			DbManager = dbMgr;
		}

		public T Load(Query q) {
			var ds = new DataSet();
			var recordQ = new Query(q);
			q.StartRecord = 0;
			q.RecordCount = 1;
			DbManager.Dalc.Load(recordQ, ds);
			var srcName = new QSourceName(q.SourceName);
			if (ds.Tables[srcName.Name].Rows.Count == 0)
				return null;
			var t = new T();
			CopyDataRowToObject(ds.Tables[srcName.Name].Rows[0], t);
			return t;
		}

		public IEnumerable<T> LoadAll(Query q) {
			var ds = new DataSet();
			DbManager.Dalc.Load(q, ds);
			var srcName = new QSourceName(q.SourceName);
			var rs = new List<T>();
			foreach (DataRow r in ds.Tables[srcName.Name].Rows) {
				var t = new T();
				CopyDataRowToObject(r, t);
				rs.Add(t);
			}
			return rs;
		}

		public void Add(T record) {
			var ds = DbManager.DataSetProvider.GetDataSet(SourceName);
			var r = ds.Tables[SourceName].NewRow();
			CopyObjectToDataRow(record, r, false);
			ds.Tables[SourceName].Rows.Add(r);
			DbManager.Update(r);
		}

		public void Update(T record) {
			var r = DbManager.Load(new Query(SourceName, ComposePkCondition(record)));
			CopyObjectToDataRow(record, r, true);
			DbManager.Update(r);
		}

		public void Delete(T record) {
			DbManager.Delete(new Query(SourceName, ComposePkCondition(record)));
		}

		protected QueryNode ComposePkCondition(T t) {
			var qcnd = new QueryGroupNode(GroupType.And);
			var ds = DbManager.DataSetProvider.GetDataSet(SourceName);
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
