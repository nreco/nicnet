using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace NI.Data.Dalc {
	
	/// <summary>
	/// DALC Record Manager
	/// </summary>
	public class DalcManager {

		IDalc _Dalc;
		IDataSetProvider _DataSetProvider;

		/// <summary>
		/// Get or set DALC instance
		/// </summary>
		public IDalc Dalc {
			get { return _Dalc; }
			set { _Dalc = value; }
		}

		/// <summary>
		/// Get or set initialized dataset provider
		/// </summary>
		public IDataSetProvider DataSetProvider {
			get { return _DataSetProvider; }
			set { _DataSetProvider = value; }
		}

		public DalcManager() {

		}

		public DalcManager(IDalc dalc, IDataSetProvider dsPrv) {
			Dalc = dalc;
			DataSetProvider = dsPrv;
		}

		/// <summary>
		/// Create new record instance
		/// </summary>
		public DataRow Create(string sourceName) {
			DataSet ds = DataSetProvider.GetDataSet(sourceName);
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

		public DataRow Load(string sourceName, params object[] pk) {
			DataSet ds = DataSetProvider.GetDataSet(sourceName);
			if (ds == null)
				throw new Exception("Unknown source name");
			Query q = new Query(sourceName, ComposePkCondition(ds.Tables[sourceName], pk));
			Dalc.Load(ds, q);
			return ds.Tables[q.SourceName].Rows.Count > 0 ? ds.Tables[q.SourceName].Rows[0] : null;
		}

		public DataRow Load(IQuery q) {
			DataSet ds = DataSetProvider.GetDataSet(q.SourceName);
			if (ds == null)
				throw new Exception("Unknown source name");
			Dalc.Load(ds, q);
			return ds.Tables[q.SourceName].Rows.Count > 0 ? ds.Tables[q.SourceName].Rows[0] : null;
		}

		public void Delete(string sourceName, params object[] pk) {
			DataRow r = Load(sourceName, pk);
			Delete(r);
		}

		public void Delete(DataRow r) {
			if (r.RowState != DataRowState.Deleted)
				r.Delete();
			Dalc.Update(r.Table.DataSet, r.Table.TableName);
		}

		public void Update(DataRow r) {
			if (r.RowState == DataRowState.Detached)
				r.Table.Rows.Add(r);
			Dalc.Update(r.Table.DataSet, r.Table.TableName);
		}

		public void Update(string sourceName, object pk, IDictionary<string, object> changeset) {
			Update(sourceName, new object[] { pk }, changeset);
		}

		public void Update(string sourceName, object[] pk, IDictionary<string, object> changeset) {
			DataSet ds = DataSetProvider.GetDataSet(sourceName);
			if (ds == null)
				throw new Exception("Unknown source name");
			Query q = new Query(sourceName, ComposePkCondition(ds.Tables[sourceName], pk) );
			Dalc.Load(ds, q);
			if (ds.Tables[q.SourceName].Rows.Count==0)
				throw new Exception("Record does not exist");
			foreach (DataRow r in ds.Tables[q.SourceName].Rows) {
				foreach (KeyValuePair<string, object> entry in changeset)
					r[entry.Key] = PrepareValue(entry.Value);
			}
			Dalc.Update(ds, sourceName);
		}


		protected IQueryNode ComposePkCondition(DataTable tbl, params object[] pk) {
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
