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

		public string ObjectViewName { get; set; }

		public ObjectContainerSqlDalcStorage(DataRowDalcMapper objectDbMgr, IDalc logDalc, Func<DataSchema> getSchema) :
			base(objectDbMgr, logDalc, getSchema) {
		}

		protected override long[] LoadTranslatedQueryInternal(Class dataClass, Query translatedQuery, Query originalQuery, QSort[] sort) {
			if (String.IsNullOrEmpty(ObjectViewName))
				return base.LoadTranslatedQueryInternal(dataClass, translatedQuery, originalQuery, sort);
			
			translatedQuery.Table = (QTable)ObjectViewName;
			var joinSb = new StringBuilder();
			var sortFields = new List<QSort>();
			if (sort!=null && sort.Length>0) {
				foreach (var origSort in sort) {
					if (origSort.Field.Prefix!=null && origSort.Field.Prefix!=originalQuery.Table.Alias) {
						// related field?
						var relationship = dataClass.Schema.FindRelationshipByID(origSort.Field.Prefix);
						/*if (relatedDataClass!=null) {
							var relatedProperty = relatedDataClass.FindPropertyByID(origSort.Field.Name);
							if (relatedProperty!=null) {
								// determine relation
								dataClass.FindRelationship(
							}
						}*/
					}

					if (origSort.Field.Prefix==null || origSort.Field.Prefix==originalQuery.Table.Alias) {
						var p = dataClass.FindPropertyByID(origSort.Field);
						if (p!=null) {
							if (p.Multivalue)
								throw new Exception("Cannot sort by mulivalue property");

							var propTblName = DataTypeTableNames[p.DataType.ID];
							var propTblAlias = propTblName+"_"+sortFields.Count.ToString();
							sortFields.Add( new QSort( propTblAlias+".value", origSort.SortDirection ) );
							joinSb.AppendFormat("LEFT JOIN {0} {1} ON ({1}.object_id={2}.id and {1}.property_compact_id={3}) ",
								propTblName, propTblAlias, ObjectTableName, p.CompactID );

							continue;
						}
					}

					sortFields.Add(origSort);

				}
				translatedQuery.Sort = sortFields.ToArray();
				translatedQuery.ExtendedProperties = new Dictionary<string,object>();
				translatedQuery.ExtendedProperties["Joins"] = joinSb.ToString();
			}
			translatedQuery.StartRecord = originalQuery.StartRecord;
			translatedQuery.RecordCount = originalQuery.RecordCount;
			Console.WriteLine(translatedQuery);

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
