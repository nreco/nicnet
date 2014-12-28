#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013-2014 Vitalii Fedorchenko
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

using NI.Data;
using NI.Data.Storage.Model;

namespace NI.Data.Storage {
	
	public class ObjectContainerSqlDalcStorage : ObjectContainerDalcStorage, ISqlObjectContainerStorage {

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

		public virtual void LoadObjectReader(Query q, Action<IDataReader> handler) {
			var schema = GetSchema();
			var dataClass = schema.FindClassByID(q.Table.Name);
			var qTranslator = new DalcStorageQueryTranslator(schema, this );

			var translatedQuery = new Query( new QTable( ObjectTableName, q.Table.Alias ) );
			translatedQuery.StartRecord = q.StartRecord;
			translatedQuery.RecordCount = q.RecordCount;
			translatedQuery.Condition = TranslateQueryCondition(dataClass, schema, q.Condition);

			translatedQuery.Fields = q.Fields;
			LoadObjectReaderInternal(dataClass, translatedQuery, q, handler);
		}

		protected virtual void LoadObjectReaderInternal(Class dataClass, Query translatedQuery, Query originalQuery, Action<IDataReader> handler) {
			var sort = originalQuery.Sort;
			var fields = translatedQuery.Fields;

			translatedQuery.Table = (QTable)ObjectViewName;
			var joinSb = new StringBuilder();
			
			var joinFieldMap = new Dictionary<string, string>();
			var knownFieldTypes = new Dictionary<string,Type>();
			var objTableAlias = originalQuery.Table.Alias ?? ObjectTableName;

			Action<QField> ensureFieldJoin = (fld) => {
				if (joinFieldMap.ContainsKey(fld.ToString()))
					return;
				if (fld.Prefix!=null && fld.Prefix!=originalQuery.Table.Alias) {
					// related field?
					var relationship = dataClass.Schema.FindRelationshipByID(fld.Prefix);
					if (relationship==null)
						relationship = dataClass.Schema.InferRelationshipByID(fld.Prefix, dataClass);

					//TBD: prevent duplicate join

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
								
						var p = relationship.Object.FindPropertyByID(fld.Name);
						if (p==null)
							throw new ArgumentException(
								String.Format("Sort field {0} referenced by relationship {1} doesn't exist",
									fld.Name, fld.Prefix));
						if (p.Multivalue)
							throw new ArgumentException(
								String.Format("Cannot join multivalue property {0}", p.ID));

						// matched related object property
						if (relationship.Multiplicity)
							throw new ArgumentException(
								String.Format("Join by relationship {0} is not possible because of multiplicity", fld.Prefix));									

						var propJoinsPrefix = "rel_"+p.ID+"_"+joinFieldMap.Count.ToString();

						var lastRelObjIdFld = GenerateRelationshipJoins(joinSb, propJoinsPrefix, String.Format("{0}.id", objTableAlias),
								relationship.Inferred ? relationship.InferredByRelationships : new[]{ relationship } );

						var propLoc = p.GetLocation(relationship.Object);

						if (p.PrimaryKey) {
							joinFieldMap[ fld.ToString() ] = lastRelObjIdFld;
						} else {
							string fldExpr = propJoinsPrefix + ".value";
							if (propLoc.Location == PropertyValueLocationType.Derived) {
								fldExpr = ResolveDerivedProperty(propLoc, fldExpr).Expression;
								propLoc = propLoc.DerivedFrom;
							}

							if (propLoc.Location == PropertyValueLocationType.ValueTable) { 
								var propTblName = DataTypeTableNames[propLoc.Property.DataType.ID];
								joinFieldMap[ fld.ToString() ] = fldExpr;
								joinSb.AppendFormat("LEFT JOIN {0} {1} ON ({1}.object_id={2} and {1}.property_compact_id={3}) ",
									propTblName, propJoinsPrefix, lastRelObjIdFld, propLoc.Property.CompactID);
							} else {
								//TBD: column property
								throw new NotImplementedException();
							}
						}
						knownFieldTypes[ fld.ToString().Replace('.','_') ] = p.DataType.ValueType;

						return;
					}
				}
				if (fld.Prefix==null || fld.Prefix==originalQuery.Table.Alias) {
					var objProp = dataClass.FindPropertyByID(fld);
					if (objProp!=null) {
						if (objProp.Multivalue)
							throw new ArgumentException("Cannot join mulivalue property");

						var propLoc = objProp.GetLocation(dataClass);

						if (objProp.PrimaryKey) {
							joinFieldMap[ fld.ToString() ] = String.Format("{0}.id", objTableAlias);
						} else {

							var propTblAlias = "prop_"+objProp.ID+"_"+joinFieldMap.Count.ToString();

							string fldExpr = propTblAlias + ".value";

							if (propLoc.Location == PropertyValueLocationType.Derived) {
								fldExpr = ResolveDerivedProperty(propLoc, fldExpr).Expression;
								propLoc = propLoc.DerivedFrom;
							}

							if (propLoc.Location == PropertyValueLocationType.ValueTable) { 
								var propTblName = DataTypeTableNames[propLoc.Property.DataType.ID];
								joinFieldMap[ fld.ToString() ] = fldExpr;
								joinSb.AppendFormat("LEFT JOIN {0} {1} ON ({1}.object_id={2}.id and {1}.property_compact_id={3}) ",
									propTblName, propTblAlias, objTableAlias, propLoc.Property.CompactID);
							} else {
								//TBD: column property
								throw new NotImplementedException();
							}
						}
						knownFieldTypes[ fld.ToString().Replace('.','_') ] = objProp.DataType.ValueType;
						return;
					}
				}
			};


			var selectFields = new List<QField>();
			var sortFields = new List<QSort>();

			if (sort!=null && sort.Length>0) {
				foreach (var origSort in sort) {
					ensureFieldJoin(origSort.Field);
					if (joinFieldMap.ContainsKey(origSort.Field.ToString())) { 
						sortFields.Add(new QSort(joinFieldMap[origSort.Field.ToString()], origSort.SortDirection));
					} else {
						sortFields.Add(origSort);
					}
				}
				translatedQuery.Sort = sortFields.ToArray();
			}
			if (fields==null || fields.Length==0) {
				fields = dataClass.Properties.Select(p => new QField(p.ID) ).ToArray();
			}

			foreach (var f in fields) {
				ensureFieldJoin(f);
				var fldName = f.ToString();
				if (joinFieldMap.ContainsKey(fldName)) { 
					selectFields.Add(new QField(fldName.Replace('.','_'), joinFieldMap[fldName]));
				} else {
					selectFields.Add(f);
				}
			}

			translatedQuery.Fields = selectFields.ToArray();
			translatedQuery.ExtendedProperties = new Dictionary<string,object>();
			translatedQuery.ExtendedProperties["Joins"] = joinSb.ToString();
			translatedQuery.StartRecord = originalQuery.StartRecord;
			translatedQuery.RecordCount = originalQuery.RecordCount;

			DbMgr.Dalc.ExecuteReader(translatedQuery, (reader) => {
				var valueTypes = new Type[reader.FieldCount];
				for (int i = 0; i < reader.FieldCount; i++) {
					var fldName = reader.GetName(i);
					if (knownFieldTypes.ContainsKey(fldName))
						valueTypes[i] = knownFieldTypes[fldName];
					else
						valueTypes[i] = null;
				}
					
				handler( new DbReaderWrapper(reader,valueTypes) );	
			});
		}

		protected override long[] LoadTranslatedQueryInternal(Class dataClass, Query translatedQuery, Query originalQuery) {
			if (String.IsNullOrEmpty(ObjectViewName))
				return base.LoadTranslatedQueryInternal(dataClass, translatedQuery, originalQuery);

			var ids = new List<long>();
			LoadObjectReaderInternal(dataClass, translatedQuery, originalQuery, (reader) => {
				int index = 0;
				while (reader.Read() && ids.Count < translatedQuery.RecordCount ) {
					if (index>=translatedQuery.StartRecord) {
						ids.Add( Convert.ToInt64( reader.FieldCount>1 ? reader[translatedQuery.Fields[0].Name] : reader[0] ) );
					}
					index++;
				}
			});
			return ids.ToArray();
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

		internal class DbReaderWrapper : IDataReader {
			IDataReader DbReader;
			Type[] KnownTypes;

			internal DbReaderWrapper(IDataReader reader, Type[] knownTypes) {
				DbReader = reader;
				KnownTypes = knownTypes;
			}

			public void Close() {
				DbReader.Close();
			}

			public int Depth {
				get { return DbReader.Depth; }
			}

			public DataTable GetSchemaTable() {
				return DbReader.GetSchemaTable();
			}

			public bool IsClosed {
				get { return DbReader.IsClosed; }
			}

			public bool NextResult() {
				return DbReader.NextResult();
			}

			public bool Read() {
				return DbReader.Read();
			}

			public int RecordsAffected {
				get { return DbReader.RecordsAffected; }
			}

			public void Dispose() {
				DbReader.Dispose();
			}

			public int FieldCount {
				get { return DbReader.FieldCount; }
			}

			public bool GetBoolean(int i) {
				return DbReader.GetBoolean(i);
			}

			public byte GetByte(int i) {
				return DbReader.GetByte(i);
			}

			public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
				return DbReader.GetBytes(i,fieldOffset,buffer,bufferoffset,length);
			}

			public char GetChar(int i) {
				return DbReader.GetChar(i);
			}

			public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
				return DbReader.GetChars(i,fieldoffset, buffer, bufferoffset,length);
			}

			public IDataReader GetData(int i) {
				return DbReader.GetData(i);
			}

			public string GetDataTypeName(int i) {
				return DbReader.GetDataTypeName(i);
			}

			public DateTime GetDateTime(int i) {
				return DbReader.GetDateTime(i);
			}

			public decimal GetDecimal(int i) {
				return DbReader.GetDecimal(i);
			}

			public double GetDouble(int i) {
				return DbReader.GetDouble(i);
			}

			public Type GetFieldType(int i) {
				return DbReader.GetFieldType(i);
			}

			public float GetFloat(int i) {
				return DbReader.GetFloat(i);
			}

			public Guid GetGuid(int i) {
				return DbReader.GetGuid(i);
			}

			public short GetInt16(int i) {
				return DbReader.GetInt16(i);
			}

			public int GetInt32(int i) {
				return DbReader.GetInt32(i);
			}

			public long GetInt64(int i) {
				return DbReader.GetInt64(i);
			}

			public string GetName(int i) {
				return DbReader.GetName(i);
			}

			public int GetOrdinal(string name) {
				return DbReader.GetOrdinal(name);
			}

			public string GetString(int i) {
				return DbReader.GetString(i);
			}

			object PrepareValue(object v, int i) {
				if (v != null && !DBNull.Value.Equals(v) && KnownTypes[i] != null && !KnownTypes[i].IsInstanceOfType(v) ) {
					return Convert.ChangeType(v, KnownTypes[i]);
				} else {
					return v;
				}
			}

			public object GetValue(int i) {
				var v = DbReader.GetValue(i);
				return PrepareValue(v,i);
			}

			public int GetValues(object[] values) {
				var res = DbReader.GetValues(values);
				for (int i=0; i<res; i++)
					values[i] = PrepareValue(values[i], i);
				return res;
			}

			public bool IsDBNull(int i) {
				return DbReader.IsDBNull(i);
			}

			public object this[string name] {
				get { 
					var fldIdx = DbReader.GetOrdinal(name);
					return GetValue(fldIdx); 
				}
			}

			public object this[int i] {
				get { return GetValue(i); }
			}
		}
		

	}
}
