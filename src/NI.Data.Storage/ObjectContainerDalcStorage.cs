#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013 Vitalii Fedorchenko
 * Copyright 2014 NewtonIdeas
 * Distributed under the LGPL licence
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Globalization;

using NI.Data;
using NI.Data.Storage.Model;

namespace NI.Data.Storage {
	
	public class ObjectContainerDalcStorage : IObjectContainerStorage {

		static Logger log = new Logger(typeof(ObjectContainerDalcStorage));

		#region Source names

		public string ObjectLogTableName { get; set; }
		public string ObjectTableName { get; set; }
		public string ObjectRelationTableName { get; set; }
		public string ObjectRelationLogTableName { get; set; }
		public IDictionary<string, string> DataTypeTableNames { get; set; }
		
		/// <summary>
		/// Value source name to log source name mapping
		/// </summary>
		public IDictionary<string, string> ValueTableNameForLogs { get; set; }

		#endregion

		protected DataRowDalcMapper DbMgr;

		protected IDalc LogDalc;

		protected Func<DataSchema> GetSchema;

		public Func<object,object> GetContextAccountId { get; set; }

		public bool LoggingEnabled { get; set; }

		public IComparer ValueComparer { get; set; }

		public ObjectContainerDalcStorage(DataRowDalcMapper objectDbMgr, IDalc logDalc, Func<DataSchema> getSchema) {
			DbMgr = objectDbMgr;
			LogDalc = logDalc;
			GetSchema = getSchema;
			ObjectLogTableName = "objects_log";
			ObjectTableName = "objects";
			ObjectRelationTableName = "object_relations";
			ObjectRelationLogTableName = "object_relations_log";
			DataTypeTableNames = new Dictionary<string, string>() {
				{PropertyDataType.Boolean.ID, "object_number_values"},
				{PropertyDataType.Decimal.ID, "object_number_values"},
				{PropertyDataType.String.ID, "object_string_values"},
				{PropertyDataType.Date.ID, "object_datetime_values"},
				{PropertyDataType.DateTime.ID, "object_datetime_values"}
			};
			ValueTableNameForLogs = new Dictionary<string, string>() {
				{"object_number_values", "object_number_values_log"},
				{"object_string_values", "object_string_values_log"},
				{"object_datetime_values", "object_datetime_values_log"}
			};

			LoggingEnabled = true;
		}

		protected void WriteObjectLog(DataRow objRow, string action) {
			if (!LoggingEnabled)
				return;

			var logData = new Hashtable();
			logData["timestamp"] = DateTime.Now;
			if (GetContextAccountId!=null)
				logData["account_id"] = GetContextAccountId(null);
			logData["compact_class_id"] = objRow["compact_class_id"];
			logData["object_id"] = objRow["id"];
			logData["action"] = action;
			LogDalc.Insert(ObjectLogTableName, logData);
		}

		protected void WriteValueLog(DataRow valRow, bool deleted = false) {
			if (!LoggingEnabled)
				return;

			var logSrcName = ValueTableNameForLogs[valRow.Table.TableName];

			var logData = new Hashtable();
			logData["timestamp"] = DateTime.Now;
			if (GetContextAccountId!=null)
				logData["account_id"] = GetContextAccountId(null);
			logData["object_id"] = valRow["object_id"];
			logData["property_compact_id"] = valRow["property_compact_id"];
			logData["value"] = valRow["value"];
			logData["deleted"] = deleted;
			LogDalc.Insert(logSrcName, logData);
		}

		protected void WriteRelationLog(DataRow refRow, bool deleted = false) {
			if (!LoggingEnabled)
				return;

			var logData = new Hashtable();
			logData["timestamp"] = DateTime.Now;
			if (GetContextAccountId!=null)
				logData["account_id"] = GetContextAccountId(null);
			logData["deleted"] = deleted;

			logData["subject_id"] = refRow["subject_id"];
			logData["predicate_class_compact_id"] = refRow["predicate_class_compact_id"];
			logData["object_id"] = refRow["object_id"];

			LogDalc.Insert(ObjectRelationLogTableName, logData);		
		}

		protected void EnsureKnownDataType(string dataType) {
			if (!DataTypeTableNames.ContainsKey(dataType))
				throw new Exception("Unknown data type: "+dataType);
		}

		protected object SerializeValueData(Property p, object val) {
			if (val == null)
				return DBNull.Value;
			
			var convertedVal = p.DataType.ConvertToValueType(val);
			if (convertedVal is bool)
				return ((bool)convertedVal) ? 1d : 0d;

			return convertedVal;
		}

		protected object DeserializeValueData(Property prop, object val) {
			if (DBNull.Value.Equals(val))
				return null;
			if (prop.DataType.ValueType == typeof(bool)) {
				return Convert.ToDecimal(val) != 0;
			}
			return val;
		}

		IEnumerable<DataRow> findPropertyRows(Property p, DataTable tbl) {
			for (int i = 0; i < tbl.Rows.Count; i++) {
				var r = tbl.Rows[i];
				if (r.RowState != DataRowState.Added
					&& r.RowState != DataRowState.Deleted
					&& Convert.ToInt32(r["property_compact_id"]) == p.CompactID)
					yield return tbl.Rows[i];
			}
		}

		protected void SaveValues(ObjectContainer obj) {
			// determine values to load
			var propSrcNameProps = new Dictionary<string, IList<int>>();
			foreach (var v in obj) {
				EnsureKnownDataType(v.Key.DataType.ID);
				var valueSrcName = DataTypeTableNames[v.Key.DataType.ID];

				if (!propSrcNameProps.ContainsKey(valueSrcName))
					propSrcNameProps[valueSrcName] = new List<int>();

				if (!propSrcNameProps[valueSrcName].Contains(v.Key.CompactID))
					propSrcNameProps[valueSrcName].Add(v.Key.CompactID);
			}
			// load value tables
			var propSrcNameToTbl = new Dictionary<string, DataTable>();
			foreach (var srcNameEntry in propSrcNameProps) {
				propSrcNameToTbl[srcNameEntry.Key] = DbMgr.LoadAll(
					new Query(srcNameEntry.Key, 
						(QField)"object_id" == new QConst(obj.ID.Value)
						&
						new QueryConditionNode((QField)"property_compact_id", Conditions.In, new QConst(srcNameEntry.Value))));
			}
			Action<DataRow> deleteValueRow = r => {
				r["value"] = DBNull.Value;
				WriteValueLog(r, true);
				r.Delete();
			};
			Action<DataTable,Property, object> addValueRow = (tbl, p, v) => {
				var valRow = tbl.NewRow();
				valRow["object_id"] = obj.ID.Value;
				valRow["property_compact_id"] = p.CompactID;
				valRow["value"] = SerializeValueData(p,v);
				WriteValueLog(valRow);
				tbl.Rows.Add(valRow);
			};

			// process values
			foreach (var v in obj) {
				var valueSrcName = DataTypeTableNames[v.Key.DataType.ID];
				var tbl = propSrcNameToTbl[valueSrcName];

				var isEmpty = IsEmpty(v.Key, v.Value);
				if (isEmpty) {
					// just remove all rows of the property
					foreach (var r in findPropertyRows(v.Key, tbl)) {
						deleteValueRow(r);
					}

				} else {
					var propRows = findPropertyRows(v.Key, tbl).ToArray();

					if (v.Key.Multivalue) {
						var vs = v.Value is IList ? (IList)v.Value : new[] { v.Value };
						var unchangedRows = new List<DataRow>();
						foreach (var vEntry in vs) {
							var isNewValue = true;
							var dbValue = SerializeValueData(v.Key, vEntry);
							for (int i = 0; i < propRows.Length; i++) {
								var pRow = propRows[i];
								if (unchangedRows.Contains(pRow))
									continue;
								if (DbValueEquals(pRow["value"], dbValue)) {
									isNewValue = false;
									unchangedRows.Add(pRow);
								}
							}
							if (isNewValue) {
								addValueRow(tbl, v.Key, vEntry);
							}
						}
						// remove not matched rows
						foreach (var r in propRows)
							if (!unchangedRows.Contains(r))
								deleteValueRow(r);

					} else {
						// hm... cleanup
						if (propRows.Length > 1)
							for (int i = 1; i < propRows.Length; i++)
								deleteValueRow(propRows[i]);
						if (propRows.Length == 0) {
							addValueRow(tbl, v.Key, v.Value);
						} else {
							// just update
							var newValue = SerializeValueData(v.Key, v.Value);
							if (!DbValueEquals(propRows[0]["value"], newValue)) {
								propRows[0]["value"] = newValue;
								WriteValueLog(propRows[0]);
							}
						}
					}

				}

			}

			// push changes to DB
			foreach (var entry in propSrcNameToTbl)
				DbMgr.Update(entry.Value);
		}

		protected bool DbValueEquals(object oldValue, object newValue) {
			if (oldValue == null)
				oldValue = DBNull.Value;
			if (newValue == null)
				newValue = DBNull.Value;
			return newValue.Equals(oldValue);
		}

		public IDictionary<long, ObjectContainer> Load(long[] ids) {
			return Load(ids, null);
		}

		/// <summary>
		/// Load objects by explicit list of ID.
		/// </summary>
		/// <param name="props">Properties to load. If null all properties are loaded</param>
		/// <param name="ids">object IDs</param>
		/// <returns>All matched by ID objects</returns>
		public IDictionary<long,ObjectContainer> Load(long[] ids, Property[] props) {
			var objById = new Dictionary<long,ObjectContainer>();
			if (ids.Length==0)
				return objById;
			var objRowTbl = DbMgr.LoadAll(new Query(ObjectTableName, new QueryConditionNode((QField)"id", Conditions.In, new QConst(ids))));
			var loadWithoutProps = props!=null && props.Length==0;

			var dataSchema = GetSchema();
			var valueSourceNames = new Dictionary<string,List<int>>();
			// construct object containers + populate source names for values to load
			foreach (DataRow objRow in objRowTbl.Rows) {
				var compactClassId = Convert.ToInt32(objRow["compact_class_id"]);
				var objClass = dataSchema.FindClassByCompactID(compactClassId);
				if (objClass == null) {
					log.Info("Class compact_id={0} of object id={1} not found; load object skipped", compactClassId, objRow["id"]);
					continue;
				}
				var obj = new ObjectContainer(objClass, Convert.ToInt64(objRow["id"]));
				
				// populate value source names by class properties
				if (!loadWithoutProps) {
					foreach (var p in objClass.Properties) {
						if (props!=null && !props.Contains(p))
							continue;
						EnsureKnownDataType(p.DataType.ID);
						var pSrcName = DataTypeTableNames[p.DataType.ID];
						if (!valueSourceNames.ContainsKey(pSrcName))
							valueSourceNames[pSrcName] = new List<int>();
						valueSourceNames[pSrcName].Add( p.CompactID );
					}
				}
				objById[ obj.ID.Value ] = obj;
			}
			// special case: no objects at all
			if (objById.Count==0)
				return objById;
			// special case: no properties to load
			if (loadWithoutProps)
				return objById;

			// load values by sourcenames
			var objIds = objById.Keys.ToArray();
			foreach (var valSrcName in valueSourceNames) {
				var valData = DbMgr.Dalc.LoadAllRecords(
						new Query(valSrcName.Key, 
							new QueryConditionNode((QField)"object_id", Conditions.In, new QConst(objIds))
							&
							new QueryConditionNode((QField)"property_compact_id",
								Conditions.In, new QConst(valSrcName.Value))
						));
				foreach (var val in valData) {
					var propertyCompactId = Convert.ToInt32(val["property_compact_id"]);
					var objId = Convert.ToInt64(val["object_id"]);
					var prop = dataSchema.FindPropertyByCompactID(propertyCompactId);
					if (prop != null) {
						// UNDONE: handle multi-values props
						if (objById.ContainsKey(objId))
							objById[objId][prop] = DeserializeValueData(prop, val["value"]);
					}
				}
			}

			return objById;
		}

		public void Insert(ObjectContainer obj) {
			var objRow = DbMgr.Insert(ObjectTableName, new Dictionary<string, object>() {
				{"compact_class_id", obj.GetClass().CompactID}
			});
			obj.ID = Convert.ToInt64( objRow["id"] );

			SaveValues(obj);

			//SaveReferences(obj, obj.References, new ObjectReference[0] );

			if (LoggingEnabled)
				WriteObjectLog(objRow, "insert");
		}

		protected Query ComposeLoadRelationsQuery(ObjectRelation[] relations) {
			var loadRelQ = new Query(ObjectRelationTableName);
			var orCondition = new QueryGroupNode(QueryGroupNodeType.Or);
			loadRelQ.Condition = orCondition;
			foreach (var r in relations) {
				var subjIdFld = r.Relation.Reversed ? "object_id" : "subject_id";
				var objIdFld = r.Relation.Reversed ? "subject_id" : "object_id";
				var relCondition = (QField)subjIdFld == new QConst(r.SubjectID)
					& (QField)objIdFld == new QConst(r.ObjectID)
					& (QField)"predicate_class_compact_id" == new QConst(r.Relation.Predicate.CompactID);
				orCondition.Nodes.Add(relCondition);
			}
			return loadRelQ;
		}

		public void AddRelations(params ObjectRelation[] relations) {
			var loadRelQ = ComposeLoadRelationsQuery(relations);
			var relTbl = DbMgr.LoadAll(loadRelQ);
			foreach (var r in relations) {
				var subjIdFld = r.Relation.Reversed ? "object_id" : "subject_id";
				var objIdFld = r.Relation.Reversed ? "subject_id" : "object_id";
				
				DataRow relRow = null;
				foreach (DataRow row in relTbl.Rows) {
					if (Convert.ToInt64(row[subjIdFld])==r.SubjectID &&
						Convert.ToInt64(row[objIdFld])==r.ObjectID &&
						Convert.ToInt32(row["predicate_class_compact_id"])==r.Relation.Predicate.CompactID) {
						relRow = row;
						break;
					}
				}
				if (relRow == null) {
					// check multiplicity constraint
					if (!r.Relation.Multiplicity) {
						if (DbMgr.Dalc.RecordsCount( ComposeSubjectRelationQuery(r.Relation, r.SubjectID) )>0)
							throw new ConstraintException(String.Format("{0} doesn't allow multiplicity", r.Relation ) );
					}

					// create new relation entry
					relRow = relTbl.NewRow();
					relRow[subjIdFld] = r.SubjectID;
					relRow[objIdFld] = r.ObjectID;
					relRow["predicate_class_compact_id"] = r.Relation.Predicate.CompactID;
					relTbl.Rows.Add(relRow);
					WriteRelationLog(relRow);
				}
			}
			DbMgr.Update(relTbl);
		}

		protected Query ComposeSubjectRelationQuery(Relationship relationship, long subjectId) {
			var q = new Query(ObjectRelationTableName);
			var cond = QueryGroupNode.And((QField)"predicate_class_compact_id" == new QConst(relationship.Predicate.CompactID));
			if (relationship.Reversed) {
				cond.Nodes.Add( (QField)"object_id"==new QConst(subjectId) );
			} else {
				cond.Nodes.Add((QField)"subject_id" == new QConst(subjectId));
			}
			q.Condition = cond;
			return q;
		}

		public void RemoveRelations(params ObjectRelation[] relations) {
			var loadRelQ = ComposeLoadRelationsQuery(relations);
			var relTbl = DbMgr.LoadAll(loadRelQ);

			foreach (DataRow relRow in relTbl.Rows) {
				if (relRow!=null) {
					WriteRelationLog(relRow,true);
					relRow.Delete();
				}
			}
			DbMgr.Update(relTbl);
		}

		public void Delete(ObjectContainer obj) {
			if (!obj.ID.HasValue)
				throw new ArgumentException("Object ID is required for delete");
			var delCount = Delete(obj.ID.Value);
			if (delCount == 0)
				throw new DBConcurrencyException(String.Format("Object id={0} doesn't exist", obj.ID.Value));
		}

		public int Delete(params long[] objIds) {
			if (objIds.Length==0)
				return 0;

			var objTbl = DbMgr.LoadAll( new Query(ObjectTableName,
				new QueryConditionNode( (QField)"id", Conditions.In, new QConst(objIds) ) ) );
			var loadedObjIds = objTbl.Rows.Cast<DataRow>().Select( r => Convert.ToInt64(r["id"]) ).ToArray();
			if (loadedObjIds.Length==0)
				return 0;

			// load all values & remove'em
			foreach (var valSrcName in DataTypeTableNames.Values.Distinct()) {
				var valTbl = DbMgr.LoadAll(new Query(valSrcName, 
						new QueryConditionNode( (QField)"object_id", Conditions.In, new QConst(loadedObjIds) )
					) );
				foreach (DataRow valRow in valTbl.Rows) {
					valRow["value"] = DBNull.Value;
					WriteValueLog(valRow);
					valRow.Delete();
				}
				DbMgr.Update(valTbl);
			}
			// load all relations & remove'em
			var refTbl = DbMgr.LoadAll(new Query(ObjectRelationTableName,
					new QueryConditionNode( (QField)"subject_id", Conditions.In, new QConst(loadedObjIds) )
					|
					new QueryConditionNode( (QField)"object_id", Conditions.In, new QConst(loadedObjIds) )
				) );
			foreach (DataRow r in refTbl.Rows) {
				WriteRelationLog(r,true);
				r.Delete();
			}
			DbMgr.Update(refTbl);

			var delCount = objTbl.Rows.Count;
			foreach (DataRow objRow in objTbl.Rows) {
				if (LoggingEnabled)
					WriteObjectLog(objRow, "delete");
				objRow.Delete();
			}
			DbMgr.Update(objTbl);
			return delCount;
		}

		public void Update(ObjectContainer obj) {
			if (!obj.ID.HasValue)
				throw new ArgumentException("Object ID is required for update");

			var objRow = DbMgr.Load(ObjectTableName, obj.ID);
			if (objRow == null)
				throw new DBConcurrencyException(String.Format("Object with ID={0} doesn't exist", obj.ID));

			SaveValues(obj);

			if (LoggingEnabled)
				WriteObjectLog(objRow, "update");
		}

		bool IsEmpty(Property p, object v) {
			if (p.DataType.IsEmpty(v))
				return true;
			return DBNull.Value.Equals(v);
		}

		bool AreValuesEqual(object o1, object o2) {
			// normalize null
			var o1norm = o1==DBNull.Value ? null : o1;
			var o2norm = o2==DBNull.Value ? null : o2;

			if (ValueComparer!=null)
				return ValueComparer.Compare(o1norm, o2norm)==0;

			return DbValueComparer.Instance.Compare( o1norm, o2norm )==0;
		}

		public IEnumerable<ObjectRelation> LoadRelations(ObjectContainer obj, Class[] predicates = null) {
			return LoadRelations(new[] { obj }, predicates);
		}
		public IEnumerable<ObjectRelation> LoadRelations(ObjectContainer[] objs, Class[] predicates = null) {
			if (objs.Length == 0)
				return new ObjectRelation[0];

			var loadQ = new Query(ObjectRelationTableName);
			var andCond = new QueryGroupNode(QueryGroupNodeType.And);
			loadQ.Condition = andCond;
			var objOrCond = new QueryGroupNode(QueryGroupNodeType.Or);
			andCond.Nodes.Add(objOrCond);

			var objIds = objs.Where(o => o.ID.HasValue).Select(o => o.ID.Value).ToArray();
			if (objIds.Length == 1) {
				objOrCond.Nodes.Add((QField)"subject_id" == new QConst(objIds[0]));
				objOrCond.Nodes.Add((QField)"object_id" == new QConst(objIds[0]));
			} else {
				// degenerated case: nothing to do
				if (objIds.Length==0)
					return new ObjectRelation[0];
				objOrCond.Nodes.Add(new QueryConditionNode(
						(QField)"subject_id", Conditions.In, new QConst(objIds)) );
				objOrCond.Nodes.Add(new QueryConditionNode(
						(QField)"object_id", Conditions.In, new QConst(objIds)) );
			}
			if (predicates != null && predicates.Length > 0) {
				var pCompactIds = predicates.Select(p=>p.CompactID).ToArray();
				andCond.Nodes.Add( new QueryConditionNode(
						(QField)"predicate_class_compact_id", Conditions.In, new QConst(pCompactIds)
					) );
			}
			
			var relTbl = DbMgr.LoadAll(loadQ);
			var rs = new List<ObjectRelation>();

			var relObjToLoad = new List<long>();
			// 1st pass: collect related object IDs
			foreach (DataRow relRow in relTbl.Rows) {
				var subjId = Convert.ToInt64(relRow["subject_id"]);
				var relObjId = Convert.ToInt64(relRow["object_id"]);
				if (!objIds.Contains(subjId))
					relObjToLoad.Add(subjId);
				if (!objIds.Contains(relObjId))
					relObjToLoad.Add(relObjId);
			}
			// load related objects without properties (we need to know their classes)
			var relObjects = Load(relObjToLoad.ToArray(), new Property[0]);
			var objIdToClass = new Dictionary<long,Class>();
			foreach (var o in objs)
				if (o.ID.HasValue)
					objIdToClass[o.ID.Value] = o.GetClass();
			foreach (var o in relObjects.Values)
				objIdToClass[o.ID.Value] = o.GetClass();

			foreach (DataRow relRow in relTbl.Rows) {
				var subjId = Convert.ToInt64(relRow["subject_id"]);
				var objId = Convert.ToInt64(relRow["object_id"]);
				var predCompactId = Convert.ToInt32(relRow["predicate_class_compact_id"]);
				
				long relSubjId, relObjId;
				var isReversed = !objIds.Contains(subjId);
				if (isReversed) {
					relSubjId = objId;
					relObjId = subjId;
				} else {
					relSubjId = subjId;
					relObjId = objId;
				}

				var subjClass = objIdToClass[relSubjId];
				var predClass = subjClass.Schema.FindClassByCompactID(predCompactId);
				if (predClass==null) {
					log.Info("Predicate with compact ID={0} doesn't exist: relation skipped", predCompactId);
					continue;
				}

				var rel = subjClass.FindRelationship(predClass, objIdToClass[relObjId], isReversed);
				if (rel!=null) {
					rs.Add(new ObjectRelation(relSubjId, rel, relObjId));
				} else {
					log.Info( "Relation between ObjectID={0} and ObjectID={1} with predicate ClassID={0} doesn't exist: relation skipped",
						relSubjId, relObjId, predClass.ID);
				}
			}
			
			return rs;
		}

		public IEnumerable<ObjectRelation> LoadRelations(Query query) {
			if (query.Fields!=null)
				throw new NotSupportedException("Relation query does not support explicit list of fields");

			var schema = GetSchema();
			// check for relation table
			var relationship = schema.FindRelationshipByID(query.Table.Name);
			if (relationship == null)
				throw new Exception(String.Format("Relationship with ID={0} does not exist", query.Table.Name));

			var qTranslator = new DalcStorageQueryTranslator(schema, this );
			var relQuery = qTranslator.TranslateSubQuery( query );
			relQuery.Sort = query.Sort; // leave as is
			
			var rs = new List<ObjectRelation>();
			DbMgr.Dalc.ExecuteReader( relQuery, (rdr) => {
				while (rdr.Read()) {
					var subjectId = Convert.ToInt64( rdr["subject_id"] );
					var objectId = Convert.ToInt64( rdr["object_id"] );
					rs.Add( new ObjectRelation(subjectId, relationship, objectId) );
				}
			});

			return rs;
		}


		public long[] ObjectIds(Query q) {
			var schema = GetSchema();
			var dataClass = schema.FindClassByID(q.Table.Name);
			var qTranslator = new DalcStorageQueryTranslator(schema, this );

			var translatedQuery = new Query( new QTable( ObjectTableName, q.Table.Alias ) );
			translatedQuery.StartRecord = q.StartRecord;
			translatedQuery.RecordCount = q.RecordCount;
			translatedQuery.Condition = TranslateQueryCondition(dataClass, schema, q.Condition);

			translatedQuery.Fields = new[] { new QField(q.Table.Alias, "id", null) };
			return LoadTranslatedQueryInternal(dataClass, translatedQuery, q, q.Sort );
		}

		protected virtual long[] LoadTranslatedQueryInternal(Class dataClass, Query translatedQuery, Query originalQuery, QSort[] sort) {
			var applySort = sort!=null && sort.Length>0;
			var ids = new List<long>();
			if (applySort) {
				translatedQuery.StartRecord = 0;
				translatedQuery.RecordCount = Int32.MaxValue;
			}
			var loadedIds = DbMgr.Dalc.LoadAllValues( translatedQuery );
			var idsArr = new long[loadedIds.Length];
			for (int i=0; i<loadedIds.Length; i++)
				idsArr[i] = Convert.ToInt64(loadedIds[i]);

			if (applySort) {
				// the following "in-code" implementation is used for abstract IDalc implementations
				var sortProperties = new List<Property>();
				foreach (var sortFld in sort) {
					// id is not handled. TBD: predefined column
					var p = dataClass.FindPropertyByID( sortFld.Field );
					if (p==null)
						throw new Exception("Unknown property "+sortFld.Field );
					sortProperties.Add(p);
				}

				var idToObj = Load(idsArr, sortProperties.ToArray() );
				Array.Sort( idsArr, (a, b) => {
					for (int i=0; i<sortProperties.Count; i++) {
						var aVal = idToObj.ContainsKey(a) ? idToObj[a][sortProperties[i]] : null;
						var bVal = idToObj.ContainsKey(b) ? idToObj[b][sortProperties[i]] : null;
						var compareRes = DbValueComparer.Instance.Compare(aVal,bVal);
						if (sort[i].SortDirection==System.ComponentModel.ListSortDirection.Descending)
							compareRes = -compareRes;
						if (compareRes!=0)
							return compareRes;
					}
					return 0;
				});
				idsArr = idsArr.Skip(originalQuery.StartRecord).Take(originalQuery.RecordCount).ToArray();
			}
			return idsArr;
		}

		public int ObjectsCount(Query q) {
			var schema = GetSchema();
			var dataClass = schema.FindClassByID(q.Table.Name);
			var translatedQuery = new Query(new QTable(ObjectTableName, q.Table.Alias));
			translatedQuery.Condition = TranslateQueryCondition(dataClass, schema, q.Condition);

			return DbMgr.Dalc.RecordsCount( translatedQuery );
		}

		protected QueryNode TranslateQueryCondition(Class dataClass, DataSchema schema, QueryNode condition) {
			var conditionGrp = QueryGroupNode.And();
			conditionGrp.Nodes.Add(
				(QField)"compact_class_id" == (QConst)dataClass.CompactID
			);
			var qTranslator = new DalcStorageQueryTranslator(schema, this);
			if (condition != null)
				conditionGrp.Nodes.Add(qTranslator.TranslateQueryNode(dataClass, condition));
			
			return conditionGrp;
		}

		public QueryNode ComposePropertyCondition(Class dataClass, Property prop, Conditions cnd, IQueryValue val) {
			var pSrcName = DataTypeTableNames[prop.DataType.ID];
			return new QueryConditionNode(
				new QField(null, "id", null),
				Conditions.In,
				new Query( pSrcName, 
					(QField)"property_compact_id"==(QConst)prop.CompactID
					&
					new QueryConditionNode( (QField)"value", cnd, val )
				) {
					Fields = new[] { (QField)"object_id" }
				}
			);
		}

	}
}
