using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace NI.Data {
	
	/// <summary>
	/// DALC Record Manager
	/// </summary>
	public class DalcManager {

		/// <summary>
		/// Get or set DALC instance
		/// </summary>
		public IDalc Dalc { get; set; }

		/// <summary>
		/// Get or set provider that returns initialized dataset with schema by source name
		/// </summary>
		public Func<string,DataSet> GetDataSetForSourceName { get; set; }

		public DalcManager() {

		}

		public DalcManager(IDalc dalc, Func<string, DataSet> dsPrv) {
			Dalc = dalc;
			GetDataSetForSourceName = dsPrv;
		}

		/// <summary>
		/// Create new record instance
		/// </summary>
		public DataRow Create(string sourceName) {
			DataSet ds = GetDataSetForSourceName(sourceName);
			return ds.Tables[sourceName].NewRow();
		}

		protected object PrepareValue(object o) {
			return o == null ? DBNull.Value : o;
		}

		/// <summary>
		/// Create new record and insert it immediately.
		/// </summary>
		public DataRow Insert(string sourceName, IDictionary<string, object> data) {
			DataRow r = Create(sourceName);
			foreach (KeyValuePair<string, object> entry in data)
				r[entry.Key] = PrepareValue(entry.Value);
			Update(r);
			return r;
		}

		public DataRow Load(string sourceName, object pk) {
			return Load(sourceName, new object[]{pk});
		}

		public DataRow Load(string sourceName, params object[] pk) {
			DataSet ds = GetDataSetForSourceName(sourceName);
			if (ds == null)
				throw new Exception("Unknown source name");
			Query q = new Query(sourceName, ComposePkCondition(ds.Tables[sourceName], pk));
			Dalc.Load(q, ds);
			return ds.Tables[q.SourceName].Rows.Count > 0 ? ds.Tables[q.SourceName].Rows[0] : null;
		}

		public DataRow Load(Query q) {
			QSource source = new QSource(q.SourceName);
			DataSet ds = GetDataSetForSourceName(source.Name);
			if (ds == null)
				ds = new DataSet();
			var tbl = Dalc.Load(q, ds);
			return tbl.Rows.Count > 0 ? tbl.Rows[0] : null;
		}

		public DataTable LoadAll(Query q) {
			QSource source = new QSource(q.SourceName);
			DataSet ds = GetDataSetForSourceName(source.Name);
			if (ds == null)
				ds = new DataSet();
			var tbl = Dalc.Load(q, ds);
			return tbl;
		}

		public void Delete(string sourceName, object pk) {
			Delete(sourceName, new object[] { pk });
		}

		public void Delete(string sourceName, params object[] pk) {
			DataRow r = Load(sourceName, pk);
            if (r != null) {
                Delete(r);
            }
		}

        public void Delete(Query q) {
            DataTable tbl = LoadAll(q);
			foreach (DataRow r in tbl.Rows)
				r.Delete();
			Update(tbl);
        }

		public void Delete(DataRow r) {
			if (r.RowState != DataRowState.Deleted)
				r.Delete();
			Dalc.Update(r.Table);
		}

		public void Update(DataRow r) {
			// this is good place to replace 'null' with DBNull
			if (r.RowState!=DataRowState.Deleted)
				foreach (DataColumn c in r.Table.Columns)
					if (c.AllowDBNull && r[c] == null)
						r[c] = DBNull.Value;

			if (r.RowState == DataRowState.Detached)
				r.Table.Rows.Add(r);
			Dalc.Update(r.Table);
		}

		public void Update(DataTable tbl) {
			Dalc.Update(tbl);
		}

		public void Update(string sourceName, object pk, IDictionary<string, object> changeset) {
			Update(sourceName, new object[] { pk }, changeset);
		}

		public void Update(string sourceName, object[] pk, IDictionary<string, object> changeset) {
			DataSet ds = GetDataSetForSourceName(sourceName);
			if (ds == null)
				throw new Exception("Unknown source name");
			Query q = new Query(sourceName, ComposePkCondition(ds.Tables[sourceName], pk) );
			var t = Dalc.Load(q, ds);
			if (t.Rows.Count==0)
				throw new Exception("Record does not exist");
			foreach (DataRow r in t.Rows) {
				foreach (KeyValuePair<string, object> entry in changeset)
					r[entry.Key] = PrepareValue(entry.Value);
			}
			Dalc.Update(t);
		}

		public int Update(Query q, IDictionary<string, object> changeset) {
			var tbl = LoadAll(q);
			foreach (DataRow r in tbl.Rows)
				foreach (var entry in changeset)
					if (tbl.Columns.Contains(entry.Key))
						r[entry.Key] = entry.Value;
			Update(tbl);
			return tbl.Rows.Count;
		}


		protected QueryNode ComposePkCondition(DataTable tbl, params object[] pk) {
			QueryGroupNode grp = new QueryGroupNode(GroupType.And);
			if (tbl.PrimaryKey.Length != pk.Length)
				throw new Exception("Invalid primary key");
			for (int i=0; i<tbl.PrimaryKey.Length; i++) {
				grp.Nodes.Add( new QueryConditionNode( (QField)tbl.PrimaryKey[i].ColumnName, Conditions.Equal, new QConst(pk[i]) ) );
			}
			return grp;
		}



	}


}
