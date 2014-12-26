using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NI.Data.Storage.Model;

namespace NI.Data.Storage {
	
	public class DalcStorageQueryTranslator {
		
		protected DataSchema Schema { get; private set; }

		protected ObjectContainerDalcStorage ObjStorage { get; private set; }

		protected Func<DerivedClassPropertyLocation,string,QField> GetDerivedField { get ;private set; }

		public DalcStorageQueryTranslator(DataSchema schema, ObjectContainerDalcStorage objStorage) {
			Schema = schema;
			ObjStorage = objStorage;
			GetDerivedField = objStorage.ResolveDerivedProperty;
		}

		public Query TranslateSubQuery(Query query) {
			// is class query?
			var dataClass = Schema.FindClassByID(query.Table.Name);
			if (dataClass!=null) {
				var tableName = ObjStorage.ObjectTableName;
				if (dataClass.ObjectLocation==ObjectLocationType.SeparateTable) {
					tableName = dataClass.ID;
				}
				var dataClassQuery = new Query(new QTable(tableName, query.Table.Alias));
				dataClassQuery.Condition = 
					(QField)"compact_class_id"==new QConst(dataClass.CompactID)
					&
					TranslateQueryNode( dataClass, query.Condition );
				// TBD: add support for any field
				CheckFieldsConstraint(query, dataClass.FindPrimaryKeyProperty().ID );
				dataClassQuery.Fields = query.Fields;
				return dataClassQuery;
			}
			var relationship = Schema.FindRelationshipByID(query.Table.Name);
			if (relationship!=null) {
				var relQuery = new Query( new QTable( ObjStorage.ObjectRelationTableName, query.Table.Alias) );
				var relQueryCondition = QueryGroupNode.And( (QField)"predicate_class_compact_id" == new QConst(relationship.Predicate.CompactID) );
				relQueryCondition.Nodes.Add( TranslateQueryNode( relationship.Predicate, query.Condition) );
				relQuery.Condition = relQueryCondition;
				
				if (query.Fields!=null)
					CheckFieldsConstraint(query, "subject_id", "object_id");
				
				relQuery.Fields = query.Fields==null ? 
					new[] { new QField( query.Table.Alias, "subject_id", null), new QField( query.Table.Alias, "object_id", null) }
					: query.Fields;

				return relQuery;
			}

			// if nothing matched keep query as is
			return query;
		}

		void CheckFieldsConstraint(Query query, params string[] allowedFieldNames) {
			if (query.Fields == null || query.Fields.Length == 0)
				throw new NotSupportedException("Subquery requires explicit list of fields to load"); 
			foreach (var f in query.Fields)
				if (!allowedFieldNames.Contains(f.Name))
					throw new NotSupportedException(String.Format("Subquery to {0} doesn't support field: {1}",
						query.Table.Name, f.Name ) );
		}


		public QueryNode TranslateQueryNode(Class targetClass, QueryNode condition) {
			if (condition==null)
				return null;

			if (condition is QueryGroupNode) {
				return TranslateGroupNode(targetClass, (QueryGroupNode)condition );
			} else if (condition is QueryConditionNode) {
				return TranslateConditionNode( targetClass, (QueryConditionNode)condition );
			}
			throw new NotSupportedException("Unknown QueryNode");
		}

		protected QueryGroupNode TranslateGroupNode(Class dataClass, QueryGroupNode node) {
			var group = new QueryGroupNode(node);
			for (int i=0; i<group.Nodes.Count; i++) {
				group.Nodes[i] = TranslateQueryNode(dataClass, group.Nodes[i]);
			}
			return group;
		}

		protected QueryNode ComposeRelatedPropertyCondition(Class dataClass, Relationship rel, QField fld, Conditions cnd, IQueryValue val) {
			var relationship = rel;
			if (!rel.Inferred && rel.Object == dataClass) {
				var revRelationship = dataClass.FindRelationship(rel.Predicate, rel.Subject, true);
				if (revRelationship == null)
					throw new ArgumentException(
						String.Format("Relationship {0} cannot be used in reverse direction", fld.Prefix));
				relationship = revRelationship;
			}
			if (relationship.Subject!=dataClass)
				throw new ArgumentException(String.Format("Relationship {0} cannot be used with {1}",fld.Prefix,dataClass.ID)); 

			var p = relationship.Object.FindPropertyByID(fld.Name);
			if (p == null)
				throw new ArgumentException(
					String.Format("Related field {0} referenced by relationship {1} doesn't exist",
						fld.Name, fld.Prefix));

			var pLoc = p.GetLocation(relationship.Object);
			QField pFld = null;
			Query propQuery = null;

			// filter by derived property handling
			if (pLoc.Location == PropertyValueLocationType.Derived) {
				if (GetDerivedField==null)
					throw new NotSupportedException(String.Format("Derived property {0} is not supported",pLoc.ToString() ));
				var derivedPropLoc = (DerivedClassPropertyLocation)pLoc;

				if (derivedPropLoc.DerivedFrom.Location == PropertyValueLocationType.TableColumn) {
					fld = GetDerivedField(derivedPropLoc, derivedPropLoc.TableColumnName);
				} else if (derivedPropLoc.DerivedFrom.Location == PropertyValueLocationType.ValueTable) {
					fld = GetDerivedField(derivedPropLoc,"value");
				} else {
					throw new NotSupportedException("DerivedFrom cannot be derived property");
				}
				pLoc = derivedPropLoc.DerivedFrom;
			}

			if (pLoc.Location == PropertyValueLocationType.ValueTable) { 
				var pSrcName = ObjStorage.DataTypeTableNames[pLoc.Property.DataType.ID];
				propQuery = new Query(pSrcName,
								(QField)"property_compact_id" == new QConst(pLoc.Property.CompactID)
								&
								new QueryConditionNode( pFld??(QField)"value", cnd, val)							
							) {
								Fields = new[] { (QField)"object_id" }
							};
			} else if (pLoc.Location==PropertyValueLocationType.TableColumn) {
				//TBD: handle separate table location
				propQuery = new Query(ObjStorage.ObjectTableName, 
					new QueryConditionNode( pFld??(QField)pLoc.TableColumnName, cnd, val) ) {
					Fields = new[] { (QField)"id" }
				};
			}

			var reverseRelSequence = relationship.Inferred ? relationship.InferredByRelationships.Reverse() : new[] { relationship };

			foreach (var r in reverseRelSequence) {
				propQuery = new Query(
					new QTable(ObjStorage.ObjectRelationTableName),
					(QField)"predicate_class_compact_id" == new QConst(r.Predicate.CompactID) 
					&
					new QueryConditionNode( new QField(r.Reversed ? "subject_id" : "object_id"), Conditions.In,
						propQuery
					)
				) {
					Fields = new[] { new QField(r.Reversed ? "object_id" : "subject_id") }
				};
			}

			return new QueryConditionNode( (QField)dataClass.FindPrimaryKeyProperty().ID, Conditions.In, propQuery );
		}

		protected QueryNode ComposePropertyCondition(Class dataClass, Property prop, Conditions cnd, IQueryValue val) {
			var propLocation = prop.GetLocation( dataClass );

			QField fld = null;
			if (propLocation.Location == PropertyValueLocationType.Derived) {
				if (GetDerivedField==null)
					throw new NotSupportedException(String.Format("Derived property {0}.{1} is not supported",propLocation.Class.ID,propLocation.Property.ID));
				var derivedPropLoc = (DerivedClassPropertyLocation)propLocation;

				if (derivedPropLoc.DerivedFrom.Location == PropertyValueLocationType.TableColumn) {
					fld = GetDerivedField(derivedPropLoc, derivedPropLoc.TableColumnName);
				} else if (derivedPropLoc.DerivedFrom.Location == PropertyValueLocationType.ValueTable) {
					fld = GetDerivedField(derivedPropLoc,"value");
				} else {
					throw new NotSupportedException("DerivedFrom cannot be derived property");
				}
				propLocation = derivedPropLoc.DerivedFrom;
			}

			if (propLocation.Location == PropertyValueLocationType.TableColumn) {
				return new QueryConditionNode(  fld ?? (QField)propLocation.TableColumnName, cnd, val);
			} else if (propLocation.Location == PropertyValueLocationType.ValueTable) {
				return ComposeValueTableCondition(propLocation.Class,propLocation.Property, fld ?? (QField)"value",cnd,val);
			} else {
				throw new NotSupportedException(String.Format("Unsupported location of class property {0}.{1}: {2}",
					propLocation.Class.ID, propLocation.Property.ID,
					propLocation.Location));
			}

		}

		private QueryNode ComposeValueTableCondition(Class dataClass, Property prop, QField fld, Conditions cnd, IQueryValue val) {
			var pSrcName = ObjStorage.DataTypeTableNames[prop.DataType.ID];
			return new QueryConditionNode(
					new QField(null, dataClass.FindPrimaryKeyProperty().ID, null),
					Conditions.In,
					new Query(pSrcName,
						(QField)"property_compact_id" == new QConst(prop.CompactID)
						&
						new QueryConditionNode(fld, cnd, val)
					) {
						Fields = new[] { (QField)"object_id" }
					}
				);
		}


		protected QueryNode TranslateConditionNode(Class dataClass, QueryConditionNode node) {
			if (node.LValue is QField && node.RValue is QField)
				throw new NotSupportedException("Cannot compare 2 class properties");

			if (node.LValue is QField) {
				var lFld = (QField)node.LValue;

				// check for related property
				if (lFld.Prefix!=null) {
					var rel = dataClass.Schema.FindRelationshipByID(lFld.Prefix);
					if (rel==null) {
						rel = dataClass.Schema.InferRelationshipByID(lFld.Prefix, dataClass);
					}
					if (rel!=null) {
						return ComposeRelatedPropertyCondition(dataClass, rel, lFld, node.Condition, TranslateQueryValue(node.RValue));
					}
				}

				var lValProperty = dataClass.FindPropertyByID(lFld.Name);
				if (lValProperty!=null) {
					return ComposePropertyCondition(dataClass, lValProperty, node.Condition, 
						TranslateQueryValue( node.RValue ) );
				}
			}
			if (node.RValue is QField) {
				var rFld = (QField)node.RValue;

				var cnd = node.Condition;
				if ((cnd & Conditions.GreaterThan) == Conditions.GreaterThan) {
					cnd = (cnd & ~Conditions.GreaterThan) | Conditions.LessThan;
				} else if ((cnd & Conditions.LessThan) == Conditions.LessThan) {
					cnd = (cnd & ~Conditions.LessThan) | Conditions.GreaterThan;
				}

				if (rFld.Prefix!=null) {
					var rel = dataClass.Schema.FindRelationshipByID(rFld.Prefix);
					if (rel != null) {
						return ComposeRelatedPropertyCondition(dataClass, rel, rFld, cnd, TranslateQueryValue(node.LValue));
					}					
				}

				var rValProperty = dataClass.FindPropertyByID(rFld.Name);
				if (rValProperty!=null) {
					return ComposePropertyCondition(dataClass, rValProperty, cnd, TranslateQueryValue(node.LValue));
				}
			}
			
			var translatedNode = new QueryConditionNode( 
				TranslateQueryValue(node.LValue), 
				node.Condition,
				TranslateQueryValue(node.RValue) );
			return translatedNode;
		}

		protected IQueryValue TranslateQueryValue(IQueryValue qVal) {
			if (qVal is Query)
				return TranslateSubQuery((Query)qVal);
			return qVal;
		}

		bool IsClassPropertyField(Class dataClass, QField fld) {
			return dataClass.FindPropertyByID(fld.Name)!=null;
		}


	}
}
