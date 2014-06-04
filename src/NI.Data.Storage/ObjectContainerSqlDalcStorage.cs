using System;
using System.Collections;
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
		public string ObjectRelationViewName { get; set; }

		public ObjectContainerSqlDalcStorage(DataRowDalcMapper objectDbMgr, IDalc logDalc, Func<DataSchema> getSchema) :
			base(objectDbMgr, logDalc, getSchema) {
		}

		protected override IList<RelationData> LoadRelationData(Query q) {
			if (!String.IsNullOrEmpty(ObjectRelationViewName)) {
				var viewQuery = new Query(ObjectRelationViewName, q.Condition);
				var rels = new List<RelationData>();
				DbMgr.Dalc.ExecuteReader( viewQuery, (rdr) => {
					while (rdr.Read()) {
						var rd = new RelationData();
						rd.SubjectId = Convert.ToInt64(rdr["subject_id"]);
						rd.ObjectId = Convert.ToInt64(rdr["object_id"]);
						rd.PredicateClassCompactId = Convert.ToInt32(rdr["predicate_class_compact_id"]);
						
						var subjectCompactClassId = rdr["subject_compact_class_id"];
						if (subjectCompactClassId!=null && !DBNull.Value.Equals(subjectCompactClassId))
							rd.SubjectClassCompactId = Convert.ToInt32(subjectCompactClassId);
						
						var objectCompactClassId = rdr["object_compact_class_id"];
						if (objectCompactClassId != null && !DBNull.Value.Equals(objectCompactClassId))
							rd.ObjectClassCompactId = Convert.ToInt32(objectCompactClassId);
						rels.Add(rd);
					}
				});
				return rels;
			} else {
				return base.LoadRelationData(q);
			}
		}

		protected override long[] LoadTranslatedQueryInternal(Class dataClass, Query translatedQuery, Query originalQuery, QSort[] sort) {
			if (String.IsNullOrEmpty(ObjectViewName))
				return base.LoadTranslatedQueryInternal(dataClass, translatedQuery, originalQuery, sort);
			
			translatedQuery.Table = (QTable)ObjectViewName;
			var joinSb = new StringBuilder();
			var sortFields = new List<QSort>();
			if (sort!=null && sort.Length>0) {
				var objTableAlias = originalQuery.Table.Alias ?? ObjectTableName;
				foreach (var origSort in sort) {
					if (origSort.Field.Prefix!=null && origSort.Field.Prefix!=originalQuery.Table.Alias) {
						// related field?
						var relationship = dataClass.Schema.FindRelationshipByID(origSort.Field.Prefix);
						if (relationship==null)
							relationship = dataClass.Schema.InferRelationshipByID(origSort.Field.Prefix, dataClass);

						if (relationship!=null) {
							if (!relationship.Inferred && relationship.Object==dataClass) {
								var revRelationship = dataClass.FindRelationship(relationship.Predicate, relationship.Subject, true);
								if (revRelationship == null)
									throw new ArgumentException(
										String.Format("Relationship {0} cannot be used in reverse direction", relationship.ID));
								relationship = revRelationship;
							}

							if (relationship.Subject!=dataClass)
								throw new ArgumentException(
									String.Format("Relationship {0} cannot be used with {1}", relationship.ID, dataClass.ID));								

								
							var p = relationship.Object.FindPropertyByID(origSort.Field.Name);
							if (p==null)
								throw new ArgumentException(
									String.Format("Sort field {0} referenced by relationship {1} doesn't exist",
										origSort.Field.Name, origSort.Field.Prefix));
							if (p.Multivalue)
								throw new ArgumentException(
									String.Format("Cannot sort by multivalue property {0}", p.ID));

							// matched related object property
							if (relationship.Multiplicity)
								throw new ArgumentException(
									String.Format("Sorting by relationship {0} is not possible because of multiplicity", origSort.Field.Prefix));									
									
							var propTblName = DataTypeTableNames[p.DataType.ID];
							var propTblAlias = propTblName+"_"+sortFields.Count.ToString();

							var lastRelObjIdFld = GenerateRelationshipJoins(joinSb, propTblAlias, String.Format("{0}.id", objTableAlias),
									relationship.Inferred ? relationship.InferredByRelationships : new[]{ relationship } );

							if (p.PrimaryKey) {
								sortFields.Add(new QSort(lastRelObjIdFld, origSort.SortDirection));
							} else {
								sortFields.Add(new QSort(propTblAlias + ".value", origSort.SortDirection));
								joinSb.AppendFormat("LEFT JOIN {0} {1} ON ({1}.object_id={2} and {1}.property_compact_id={3}) ",
									propTblName, propTblAlias, lastRelObjIdFld, p.CompactID);
							}

							continue;
						}
					}

					if (origSort.Field.Prefix==null || origSort.Field.Prefix==originalQuery.Table.Alias) {
						var p = dataClass.FindPropertyByID(origSort.Field);
						if (p!=null) {
							if (p.Multivalue)
								throw new ArgumentException("Cannot sort by mulivalue property");

							if (p.PrimaryKey) {
								sortFields.Add(new QSort( String.Format("{0}.id", objTableAlias), origSort.SortDirection));
							} else {
								var propTblName = DataTypeTableNames[p.DataType.ID];
								var propTblAlias = propTblName+"_"+sortFields.Count.ToString();
								sortFields.Add( new QSort( propTblAlias+".value", origSort.SortDirection ) );
								joinSb.AppendFormat("LEFT JOIN {0} {1} ON ({1}.object_id={2}.id and {1}.property_compact_id={3}) ",
									propTblName, propTblAlias, objTableAlias, p.CompactID);
							}
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

			var idValues = DbMgr.Dalc.LoadAllValues( translatedQuery );
			var ids = new long[idValues.Length];
			for (int i=0; i<ids.Length; i++)
				ids[i] =  Convert.ToInt64(idValues[i]);
			return ids;
		}

		protected string GenerateRelationshipJoins(StringBuilder sqlBuilder, string joinTblPrefix, string subjIdFld, IEnumerable<Relationship> rels) {
			string lastRelObjIdFld = subjIdFld;
			var joinCount = 0;
			foreach (var r in rels) {
				var subjFieldName = r.Reversed ? "object_id" : "subject_id";
				var objFieldName = r.Reversed ? "subject_id" : "object_id";

				var relTblAlias = String.Format("{0}_{1}_rel", joinTblPrefix, joinCount++);

				sqlBuilder.AppendFormat("LEFT JOIN {0} {1} ON ({1}.{2}={3} and {1}.predicate_class_compact_id={4})\n",
					ObjectRelationTableName, relTblAlias, subjFieldName, lastRelObjIdFld, r.Predicate.CompactID);

				lastRelObjIdFld = relTblAlias+"."+objFieldName;
			}
			return lastRelObjIdFld;
		}

	}
}
