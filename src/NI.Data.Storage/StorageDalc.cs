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
using System.Text.RegularExpressions;
using System.Data;
using System.Threading.Tasks;

using NI.Data.Storage.Model;
using NI.Data;

namespace NI.Data.Storage
{
    public class StorageDalc : IDalc {

		protected IObjectContainerStorage ObjectContainerStorage { get; set; }
		protected IDalc UnderlyingDalc { get; set; }

		protected Func<DataSchema> GetSchema { get; set; }

		public StorageDalc(IDalc dalc, IObjectContainerStorage objContainerStorage, Func<DataSchema> getSchema) {
			UnderlyingDalc = dalc;
			GetSchema = getSchema;
			ObjectContainerStorage = objContainerStorage;
        }


		public DataTable Load(Query query, DataSet ds) {
			var schema = GetSchema();
			var dataClass = schema.FindClassByID(query.Table.Name);
			if (dataClass != null) {
				return LoadObjectTable(query, ds, dataClass);
			}
			// check for relation table
			var relation = schema.FindRelationshipByID(query.Table.Name);
			if (relation!=null) {
				// matched
				return LoadRelationTable(query, ds);
			}

			return UnderlyingDalc.Load(query, ds);
		}

		protected DataTable LoadRelationTable(Query query, DataSet ds) {
			var relQuery = new Query(query);
			relQuery.Fields = null; // no explicit fields

			var relations = ObjectContainerStorage.LoadRelations(relQuery);
			if (!ds.Tables.Contains(query.Table.Name))
				ds.Tables.Add(query.Table.Name);
				
			var tbl = ds.Tables[query.Table.Name];
			// ensure columns
			var fields = query.Fields ?? new[] { (QField)"subject_id", (QField)"object_id" };
			foreach (var f in fields)
				if (!tbl.Columns.Contains(f.Name))
					tbl.Columns.Add( f.Name, typeof(long) );

			var loadSubj = tbl.Columns.Contains("subject_id");
			var loadObj = tbl.Columns.Contains("object_id");
			foreach (var rel in relations) {
				var row = tbl.NewRow();
				if (loadSubj)
					row["subject_id"] = rel.SubjectID;
				if (loadObj)
					row["object_id"] = rel.ObjectID;

				tbl.Rows.Add(row);
			}
			tbl.AcceptChanges();

			return tbl;
		}

		protected DataTable LoadObjectTable(Query query, DataSet ds, Class dataClass) {
			// special count query
			if (query.Fields != null &&
				query.Fields.Length == 1 &&
				query.Fields[0].Expression != null &&
				query.Fields[0].Expression.ToLower() == "count(*)") {

				if (ds.Tables.Contains(query.Table.Name))
					ds.Tables.Remove(query.Table.Name);
				var t = ds.Tables.Add(query.Table.Name);
				t.Columns.Add("count", typeof(int));
				var cntRow = t.NewRow();
				cntRow["count"] = ObjectContainerStorage.ObjectsCount(query);
				t.Rows.Add(cntRow);
				return t;
			}

			DataTable tbl;
			if (!ds.Tables.Contains(dataClass.ID)) {
				tbl = ds.Tables.Add(dataClass.ID);
			} else {
				tbl = ds.Tables[dataClass.ID];
			}
			// check columns
			IEnumerable<Property> propsToLoad = null;
			var relatedPropsToLoad = new Dictionary<Relationship,List<Property>>();
			var relationFieldNamePrefix = new Dictionary<Relationship,string>();
			var relatedProps = new List<Property>();

			bool includeId = query.Fields == null || query.Fields.Where(f => f.Name == "id" && f.Prefix == query.Table.Alias).Any();

			if (includeId) {
				// ensure special "id" column
				if (!tbl.Columns.Contains("id") || tbl.Columns["id"].DataType != typeof(long)) {
					if (tbl.Columns.Contains("id"))
						tbl.Columns.Remove("id");
					tbl.Columns.Add("id", typeof(long));
				}
			}
			if (query.Fields == null) {
				propsToLoad = dataClass.Properties;
			} else {
				var queryProps = new List<Property>();
				foreach (var fld in query.Fields) {
					if (fld.Name == "id") continue; // tmp

					if (fld.Prefix!=null) {
						var rel = dataClass.Schema.FindRelationshipByID( fld.Prefix );
						if (rel==null)
							rel = dataClass.Schema.InferRelationshipByID( fld.Prefix, dataClass );

						if (rel!=null) {
							if (rel.Object == dataClass && !rel.Inferred) {
								rel  = dataClass.FindRelationship( rel.Predicate, rel.Subject, true );
							}
							if (rel.Subject!=dataClass)
								throw new ArgumentException(String.Format(
									"Relation {0} is not applicable with {1}", fld.Prefix, dataClass.ID ) );
							var relProp = rel.Object.FindPropertyByID( fld.Name );

							if (relProp != null) {
								if (!relatedPropsToLoad.ContainsKey(rel))
									relatedPropsToLoad[rel] = new List<Property>();
								relatedPropsToLoad[rel].Add(relProp);
								relationFieldNamePrefix[rel] = fld.Prefix.Replace('.','_'); // for inferred relations
								relatedProps.Add(relProp);
								continue;
							}
						}
					}
					var prop = dataClass.FindPropertyByID(fld.Name);
					if (prop==null)
						throw new Exception(String.Format("Unknown field {0}", fld));
					queryProps.Add(prop);
				}
				propsToLoad = queryProps;
			}


			// ensure property columns
			foreach (var p in propsToLoad) {
				var propType = p.DataType.ValueType;
				if (p.Multivalue)
					propType = propType.MakeArrayType();
				if (!tbl.Columns.Contains(p.ID) || tbl.Columns[p.ID].DataType != propType) {
					if (tbl.Columns.Contains(p.ID))
						tbl.Columns.Remove(p.ID);
					tbl.Columns.Add(p.ID, propType);
				}
			}
			foreach (var relEntry in relatedPropsToLoad)
				foreach (var relProp in relEntry.Value) {
					var propType = relProp.DataType.ValueType;
					if (relProp.Multivalue || relEntry.Key.Multiplicity)
						propType = propType.MakeArrayType();
					
					var relColName = String.Format("{0}_{1}", relationFieldNamePrefix[relEntry.Key], relProp.ID);

					if (!tbl.Columns.Contains(relColName) || tbl.Columns[relColName].DataType != propType) {
						if (tbl.Columns.Contains(relColName))
							tbl.Columns.Remove(relColName);
						tbl.Columns.Add(relColName, propType);
					}
				}

			var ids = ObjectContainerStorage.ObjectIds(query);
			var objects = ObjectContainerStorage.Load(ids, propsToLoad.ToArray());
			
			IDictionary<long,ObjectContainer> relatedObjects = null;
			var objIdToRelationData = new Dictionary<long,List<ObjectRelation>>();
			if (relatedPropsToLoad.Count>0) {
				var relationData = ObjectContainerStorage.LoadRelations( objects.Values.ToArray(), relatedPropsToLoad.Keys );
				var relObjIds = new List<long>();
				foreach (var r in relationData) {
					if (!relObjIds.Contains(r.ObjectID)) {
						relObjIds.Add(r.ObjectID);
					}
					if (!objIdToRelationData.ContainsKey(r.SubjectID))
						objIdToRelationData[r.SubjectID] = new List<ObjectRelation>();
					objIdToRelationData[r.SubjectID].Add( r );
				}
				relatedObjects = ObjectContainerStorage.Load(relObjIds.ToArray(), relatedProps.ToArray());
			}

			foreach (var id in ids) {
				if (objects.ContainsKey(id)) {
					var obj = objects[id];
					var r = tbl.NewRow();

					if (includeId)
						r["id"] = obj.ID.Value;

					foreach (var p in propsToLoad)
						r[p.ID] = obj[p] ?? DBNull.Value;

					if (relatedPropsToLoad.Count>0 && objIdToRelationData.ContainsKey(obj.ID.Value))
						foreach (var relEntry in relatedPropsToLoad) {
							foreach (var rel in objIdToRelationData[obj.ID.Value]) {
								if (rel.Relation.Equals(relEntry.Key) && relatedObjects.ContainsKey(rel.ObjectID)) {
									// process related props
									foreach (var relProp in relEntry.Value) {
										var relColName = String.Format("{0}_{1}", relationFieldNamePrefix[relEntry.Key], relProp.ID);
										if (relEntry.Key.Multiplicity) {
											//TBD: add merge values 
											r[relColName] = r.IsNull(relColName) ? 
												relatedObjects[rel.ObjectID][relProp] :
												MergeMultivalueColumns(r[relColName], relatedObjects[rel.ObjectID][relProp] );
										} else {
											r[relColName] = relatedObjects[rel.ObjectID][relProp];
										}
									}
								}
							}
						}

					tbl.Rows.Add(r);
					r.AcceptChanges();
				}
			}
			tbl.AcceptChanges();
			return tbl;			
		}

		protected object MergeMultivalueColumns(object o1, object o2) {
			var ar1 = o1 as Array;
			var ar2 = o2 as Array;
			if (ar1==null || ar2==null || ar1.GetType().GetElementType()!=ar2.GetType().GetElementType() ) return o1;
			var newArr = Array.CreateInstance( ar1.GetType().GetElementType(), ar1.Length+ar2.Length);
			Array.Copy( ar1, 0, newArr, 0, ar1.Length);
			Array.Copy(ar2, 0, newArr, ar1.Length, ar2.Length);
			return newArr;
		}


		public void ExecuteReader(Query q, Action<IDataReader> handler) {
			var ds = new DataSet();
			var internalQuery = new Query(q);
			internalQuery.StartRecord = 0;
			if (q.RecordCount<Int32.MaxValue)
				internalQuery.RecordCount = q.StartRecord + q.RecordCount;
			Load(internalQuery, ds);  // todo - avoid intermediate table?
			handler( new DataTableReader(ds.Tables[q.Table.Name]) );
		}

		public int Delete(Query query) {
			var srcName = new QTable(query.Table.Name);
			var dataClass = GetSchema().FindClassByID(query.Table.Name);
			if (dataClass != null) {
				var ids = ObjectContainerStorage.ObjectIds(query);
				return ObjectContainerStorage.Delete(ids);
			}
			return UnderlyingDalc.Delete(query);
		}

		public void Insert(string tableName, IDictionary<string, IQueryValue> data) {
			var schema = GetSchema();
			var dataClass = schema.FindClassByID(tableName);
			if (dataClass != null) {
				var objContainer = new ObjectContainer(dataClass);
				foreach (var changeEntry in data) {
					if (!(changeEntry.Value is QConst))
						throw new NotSupportedException(
							String.Format("{0} value type is not supported", changeEntry.Value.GetType() ) );

					objContainer[changeEntry.Key] = ((QConst)changeEntry.Value).Value;
				}
				ObjectContainerStorage.Insert(objContainer);
				return;
			}
			
			// check for relation table
			var relation = schema.FindRelationshipByID(tableName);
			if (relation!=null) {
				long? subjId = null;
				long? objId = null;
				foreach (var entry in data) {
					var valConst = entry.Value as QConst;
					if (valConst==null)
						throw new NotSupportedException(
							String.Format("Value {0} for {1} is not supported", entry.Value, entry.Key ) );
					if (entry.Key=="subject_id")
						subjId = Convert.ToInt64( ((QConst)entry.Value).Value );
					else if (entry.Key=="object_id")
						objId = Convert.ToInt64( ((QConst)entry.Value).Value );
					else {
						throw new ArgumentException(String.Format("{0} does not exist in {1}", entry.Key, tableName));
					}
				}
				if (!subjId.HasValue)
					throw new ArgumentException("subject_id is required");
				if (!objId.HasValue)
					throw new ArgumentException("object_id is required");
				ObjectContainerStorage.AddRelations(
					new ObjectRelation(subjId.Value, relation, objId.Value) );
				return;
			}

			UnderlyingDalc.Insert(tableName,data);
		}

		public int Update(Query query, IDictionary<string, IQueryValue> data) {
			var schema = GetSchema();
			var dataClass = schema.FindClassByID(query.Table.Name);
			if (dataClass!=null) {
				var affectedObjIds = ObjectContainerStorage.ObjectIds( query );
				foreach (var objId in affectedObjIds) {
					var obj = new ObjectContainer(dataClass, objId);
					foreach (var entry in data) {
						var valConst = entry.Value as QConst;
						if (valConst == null)
							throw new NotSupportedException(
								String.Format("Value {0} for {1} is not supported", entry.Value, entry.Key));						
						obj[entry.Key] = valConst.Value;
					}
					ObjectContainerStorage.Update(obj);
				}
				return affectedObjIds.Length;
			}
			var relation = schema.FindRelationshipByID(query.Table.Name);
			if (relation!=null) {
				throw new NotSupportedException(String.Format("Update is not allowed for relationship {0}", relation.ID ) );
			}			
			return UnderlyingDalc.Update(query, data);
		}

		public void Update(DataTable t) {
			var schema = GetSchema();
			var dataClass = schema.FindClassByID(t.TableName);
			if (dataClass!=null) {
				foreach (DataRow r in t.Rows) {
					switch (r.RowState) {
						case DataRowState.Added:
							InsertDataRow(dataClass, r);
							break;
						case DataRowState.Modified:
							UpdateDataRow(dataClass, r);
							break;
						case DataRowState.Deleted:
							DeleteDataRow(dataClass, r);
							break;
					}
				}
				t.AcceptChanges();
				return;
			}
			// check for relation table
			var relation = schema.FindRelationshipByID(t.TableName);
			if (relation!=null) {
				foreach (DataColumn c in t.Columns)
					if (c.ColumnName!="subject_id" && c.ColumnName!="object_id")
						throw new ArgumentException(String.Format("{0} does not exist in {1}", c.ColumnName, t.TableName));
				if (!t.Columns.Contains("subject_id"))
					throw new ArgumentException("subject_id column is required");
				if (!t.Columns.Contains("object_id"))
					throw new ArgumentException("object_id column is required");
				foreach (DataRow r in t.Rows) {
					switch (r.RowState) {
						case DataRowState.Added:
							ObjectContainerStorage.AddRelations(
								new ObjectRelation( 
									Convert.ToInt64(r["subject_id"]),
									relation,
									Convert.ToInt64(r["object_id"])));
							break;
						case DataRowState.Deleted:
							ObjectContainerStorage.RemoveRelations(
								new ObjectRelation(
									Convert.ToInt64(r["subject_id",DataRowVersion.Original]),
									relation,
									Convert.ToInt64(r["object_id",DataRowVersion.Original]))
							);
							break;
						default:
							throw new NotSupportedException(String.Format("Relation doesn't support row state {0}", r.RowState));
					}
				}
				t.AcceptChanges();
				return;
			}

			UnderlyingDalc.Update(t);
		}

		protected void InsertDataRow(Class dataClass, DataRow r) {
			var obj = new ObjectContainer(dataClass);
			foreach (DataColumn c in r.Table.Columns)
				if (!c.AutoIncrement) {
					var val = r[c];
					if (val==DBNull.Value)
						val = null;
					obj[ c.ColumnName ] = val;
				}
			ObjectContainerStorage.Insert(obj);
			r["id"] = obj.ID.Value;
		}

		protected void DeleteDataRow(Class dataClass, DataRow r) {
			var objId = Convert.ToInt64( r["id",DataRowVersion.Original] );
			var obj = new ObjectContainer(dataClass, objId );
			ObjectContainerStorage.Delete( obj );
		}

		protected void UpdateDataRow(Class dataClass, DataRow r) {
			var objId = Convert.ToInt64(r["id"]);
			var obj = new ObjectContainer(dataClass, objId);
			foreach (DataColumn c in r.Table.Columns)
				if (!c.AutoIncrement) {
					var val = r[c];
					if (val == DBNull.Value)
						val = null;
					obj[c.ColumnName] = val;
				}

			ObjectContainerStorage.Update(obj);
		}
	}
}
