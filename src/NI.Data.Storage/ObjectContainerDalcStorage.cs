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

		public string ObjectLogSourceName { get; set; }
		public string ObjectSourceName { get; set; }
		public string ObjectRelationSourceName { get; set; }
		public string ObjectRelationLogSourceName { get; set; }
		public IDictionary<string, string> DataTypeSourceNames { get; set; }
		
		/// <summary>
		/// Value source name to log source name mapping
		/// </summary>
		public IDictionary<string, string> ValueSourceNameForLogs { get; set; }

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
			ObjectLogSourceName = "objects_log";
			ObjectSourceName = "objects";
			ObjectRelationSourceName = "object_relations";
			ObjectRelationLogSourceName = "object_relations_log";
			DataTypeSourceNames = new Dictionary<string, string>() {
				{"boolean", "object_number_values"},
				{"decimal", "object_number_values"},
				{"string", "object_string_values"},
				{"date", "object_datetime_values"},
				{"datetime", "object_datetime_values"}
			};
			ValueSourceNameForLogs = new Dictionary<string, string>() {
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
			LogDalc.Insert(ObjectLogSourceName, logData);
		}

		protected void WriteValueLog(DataRow valRow, bool deleted = false) {
			if (!LoggingEnabled)
				return;

			var logSrcName = ValueSourceNameForLogs[valRow.Table.TableName];

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

			LogDalc.Insert(ObjectRelationLogSourceName, logData);		
		}

		protected void EnsureKnownDataType(string dataType) {
			if (!DataTypeSourceNames.ContainsKey(dataType))
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
				var valueSrcName = DataTypeSourceNames[v.Key.DataType.ID];

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
				var valueSrcName = DataTypeSourceNames[v.Key.DataType.ID];
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
			var objRowTbl = DbMgr.LoadAll(new Query(ObjectSourceName, new QueryConditionNode((QField)"id", Conditions.In, new QConst(ids))));
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
						var pSrcName = DataTypeSourceNames[p.DataType.ID];
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
				var valTbl = DbMgr.LoadAll(
						new Query(valSrcName.Key, 
							new QueryConditionNode((QField)"object_id", Conditions.In, new QConst(objIds))
							&
							new QueryConditionNode((QField)"property_compact_id",
								Conditions.In, new QConst(valSrcName.Value))
						));
				foreach (DataRow valRow in valTbl.Rows) {
					var propertyCompactId = Convert.ToInt32(valRow["property_compact_id"]);
					var objId = Convert.ToInt64(valRow["object_id"]);
					var prop = dataSchema.FindPropertyByCompactID(propertyCompactId);
					if (prop != null) {
						// UNDONE: handle multi-values props
						if (objById.ContainsKey(objId))
							objById[objId][prop] = DeserializeValueData(prop, valRow["value"] );
					}
				}
			}

			return objById;
		}

		public void Insert(ObjectContainer obj) {
			var objRow = DbMgr.Insert(ObjectSourceName, new Dictionary<string, object>() {
				{"compact_class_id", obj.GetClass().CompactID}
			});
			obj.ID = Convert.ToInt64( objRow["id"] );

			SaveValues(obj);

			//SaveReferences(obj, obj.References, new ObjectReference[0] );

			if (LoggingEnabled)
				WriteObjectLog(objRow, "insert");
		}

		protected Query ComposeLoadRelationsQuery(ObjectRelation[] relations) {
			var loadRelQ = new Query(ObjectRelationSourceName);
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
			Delete(obj.ID.Value);
		}

		public void Delete(long objId) {

			var objRow = DbMgr.Load(ObjectSourceName, objId);
			if (objRow == null)
				throw new DBConcurrencyException(String.Format("Object id={0} doesn't exist", objId));

			// load all values & remove'em
			foreach (var valSrcName in DataTypeSourceNames.Values.Distinct()) {
				var valTbl = DbMgr.LoadAll(new Query(valSrcName, (QField)"object_id" == new QConst(objId) ));
				foreach (DataRow valRow in valTbl.Rows) {
					valRow["value"] = DBNull.Value;
					WriteValueLog(valRow);
					valRow.Delete();
				}
				DbMgr.Update(valTbl);
			}
			// load all relations & remove'em
			var refTbl = DbMgr.LoadAll(new Query(ObjectRelationSourceName,
					(QField)"subject_id" == new QConst(objId) | (QField)"object_id" == new QConst(objId)));
			foreach (DataRow r in refTbl.Rows) {
				WriteRelationLog(r,true);
				r.Delete();
			}
			DbMgr.Update(refTbl);

			if (LoggingEnabled)
				WriteObjectLog(objRow, "delete");
			objRow.Delete();
			DbMgr.Update(objRow);
		}

		public void Update(ObjectContainer obj) {
			if (!obj.ID.HasValue)
				throw new ArgumentException("Object ID is required for update");

			var objRow = DbMgr.Load(ObjectSourceName, obj.ID);
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

			var loadQ = new Query(ObjectRelationSourceName);
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
				if (objIds.Contains(subjId)) {
					relSubjId = subjId;
					relObjId = objId;
				} else {
					relSubjId = objId;
					relObjId = subjId;
				}

				var subjClass = objIdToClass[relSubjId];
				var predClass = subjClass.Schema.FindClassByCompactID(predCompactId);
				if (predClass==null) {
					log.Info("Predicate with compact ID={0} doesn't exist: relation skipped", predCompactId);
					continue;
				}

				var rel = subjClass.FindRelationship(predClass, objIdToClass[relObjId]);
				if (rel!=null) {
					rs.Add(new ObjectRelation(relSubjId, rel, relObjId));
				} else {
					log.Info( "Relation between ObjectID={0} and ObjectID={1} with predicate ClassID={0} doesn't exist: relation skipped",
						relSubjId, relObjId, predClass.ID);
				}
			}
			
			return rs;
		}

		public long[] ObjectIds(Query q) {
			var schema = GetSchema();
			var dataClass = schema.FindClassByID(q.Table.Name);
			var qTranslator = new DalcStorageQueryTranslator(schema, this );

			var translatedQuery = new Query( new QTable( ObjectSourceName, q.Table.Alias ) );
			translatedQuery.StartRecord = q.StartRecord;
			translatedQuery.RecordCount = q.RecordCount;
			translatedQuery.Condition = TranslateQueryCondition(dataClass, schema, q.Condition);
			
			translatedQuery.Fields = new [] { new QField(q.Table.Alias, "id", null) };

			var ids = new List<long>();
			DbMgr.Dalc.ExecuteReader(translatedQuery, (rdr) => {
				while (rdr.Read()) {
					var id = Convert.ToInt64( rdr.GetValue(0) );
					ids.Add(id);
				}
			});
			return ids.ToArray();
		}

		public int ObjectsCount(Query q) {
			var schema = GetSchema();
			var dataClass = schema.FindClassByID(q.Table.Name);
			var translatedQuery = new Query(new QTable(ObjectSourceName, q.Table.Alias));
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



		public QueryNode ComposeFieldCondition(Class dataClass, QField field, Conditions cnd, IQueryValue val) {
			//tmp hack for predefined fields
			if (field.Name=="id") {
				return new QueryConditionNode( field, cnd, val);
			}

			var prop = dataClass.FindPropertyByID(field.Name);
			if (prop==null)
				throw new Exception(String.Format("Class ID={0} doesn't contain property ID={1}", dataClass.ID, field.Name) );

			var pSrcName = DataTypeSourceNames[prop.DataType.ID];
			return new QueryConditionNode( 
				new QField( field.Prefix, "id",null ),
				Conditions.In,
				new Query( pSrcName, 
					(QField)"property_compact_id"==(QConst)prop.CompactID
					&
					new QueryConditionNode( (QConst)"value", cnd, val )
				) {
					Fields = new[] { (QField)"object_id" }
				}
			);
		}

	}
}
