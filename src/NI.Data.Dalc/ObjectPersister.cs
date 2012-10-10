using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace NI.Data.Dalc {
	
	
	public class ObjectPersister<T> where T : class, new() {

		public string SourceName { get; set; }

		public IDictionary<string,string> FieldToProperty { get; set; }

		public DalcManager DbManager { get; set; }

		public ObjectPersister() {
		}

		public ObjectPersister(string sourceName, IDictionary<string, string> fieldToProperty, DalcManager dbMgr) {
			SourceName = sourceName;
			FieldToProperty = fieldToProperty;
			DbManager = dbMgr;
		}

		protected string GetPropertyName(string fldName) {
			return FieldToProperty!=null && FieldToProperty.ContainsKey(fldName) ? FieldToProperty[fldName] : fldName;
		}

		public T Load(Query q) {
			var ds = new DataSet();
			var recordQ = new Query(q);
			q.StartRecord = 0;
			q.RecordCount = 1;
			DbManager.Dalc.Load(ds, recordQ);
			var srcName = new QSourceName(q.SourceName);
			if (ds.Tables[srcName.Name].Rows.Count == 0)
				return null;
			var t = new T();
			CopyDataRowToObject(ds.Tables[srcName.Name].Rows[0], t);
			return t;
		}

		public IEnumerable<T> LoadAll(Query q) {
			var ds = new DataSet();
			DbManager.Dalc.Load(ds, q);
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

		protected IQueryNode ComposePkCondition(T t) {
			var qcnd = new QueryGroupNode(GroupType.And);
			var ds = DbManager.DataSetProvider.GetDataSet(SourceName);
			foreach (DataColumn c in ds.Tables[SourceName].PrimaryKey) {
				var pInfo = typeof(T).GetProperty(GetPropertyName(c.ColumnName));
				var pVal = pInfo.GetValue(t, null);
				qcnd.Nodes.Add(new QueryConditionNode((QField)c.ColumnName, Conditions.Equal, new QConst( pVal )));
			}
			return qcnd;
		}

		protected void CopyObjectToDataRow(object o, DataRow r, bool ignorePk) {
			foreach (DataColumn c in r.Table.Columns) {
				var pInfo = typeof(T).GetProperty( GetPropertyName( c.ColumnName ) );
				if (pInfo == null)
					continue;
				if (ignorePk && Array.IndexOf(r.Table.PrimaryKey, c) >= 0)
					continue;

				var pVal = pInfo.GetValue(o, null);
				if (pVal == null) {
					pVal = DBNull.Value;
				} else {
					pVal = Convert.ChangeType(pVal, c.DataType, CultureInfo.InvariantCulture);
				}
				r[c] = pVal;
			}
		}

		protected void CopyDataRowToObject(DataRow r, object o) {
			foreach (DataColumn c in r.Table.Columns) {
				var pInfo = typeof(T).GetProperty( GetPropertyName(c.ColumnName) );
				if (pInfo != null) {
					var rVal = r[c];
					if (rVal == null || DBNull.Value.Equals(rVal)) {
						rVal = Nullable.GetUnderlyingType(pInfo.PropertyType) != null ? null : default(T);
					} else {
						var propType = pInfo.PropertyType;
						if (Nullable.GetUnderlyingType(propType) != null)
							propType = Nullable.GetUnderlyingType(propType);

						rVal = Convert.ChangeType(rVal, propType, CultureInfo.InvariantCulture);
					}
					pInfo.SetValue(o, rVal, null);
				}
			}
		}


	}

}
