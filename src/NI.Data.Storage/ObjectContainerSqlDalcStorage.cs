using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

using NI.Data;
using NI.Data.Storage.Model;

namespace NI.Data.Storage {
	
	public class ObjectContainerSqlDalcStorage : ObjectContainerDalcStorage {

		public string ObjectsViewName { get; set; }

		public ObjectContainerSqlDalcStorage(DataRowDalcMapper objectDbMgr, IDalc logDalc, Func<DataSchema> getSchema) :
			base(objectDbMgr, logDalc, getSchema) {
		}

		protected override long[] LoadTranslatedQueryInternal(Class dataClass, Query translatedQuery, Query originalQuery, QSort[] sort) {
			if (String.IsNullOrEmpty(ObjectsViewName))
				return base.LoadTranslatedQueryInternal(dataClass, translatedQuery, originalQuery, sort);
			
			translatedQuery.Table = (QTable)ObjectsViewName;
			var joinSb = new StringBuilder();
			var sortFields = new List<QSort>();
			foreach (var origSort in sort) {
				var p = dataClass.FindPropertyByID(origSort.Field);
				if (p.Multivalue)
					throw new Exception("Cannot sort by mulivalue property");

				var propTblName = DataTypeTableNames[p.DataType.ID];
				var propTblAlias = propTblName+"_"+sortFields.Count.ToString();
				sortFields.Add( new QSort( propTblAlias+".value", origSort.SortDirection ) );
				joinSb.AppendFormat("LEFT JOIN {0} {1} ON ({1}.object_id={2}.id and property_compact_id={3}) ",
					propTblName, propTblAlias, ObjectTableName, p.CompactID );
			}
			translatedQuery.Sort = sortFields.ToArray();
			translatedQuery.ExtendedProperties["joinSortValues"] = joinSb.ToString();

			var ids = new List<long>();
			DbMgr.Dalc.ExecuteReader(translatedQuery, (rdr) => {
				while (rdr.Read()) {
					var id = Convert.ToInt64(rdr.GetValue(0));
					ids.Add(id);
				}
			});
			return ids.ToArray();
		}

	}
}
