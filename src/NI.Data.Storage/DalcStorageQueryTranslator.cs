using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NI.Data.Storage.Model;

namespace NI.Data.Storage {
	
	public class DalcStorageQueryTranslator {
		
		protected DataSchema Schema;

		protected ObjectContainerDalcStorage ObjStorage;

		public DalcStorageQueryTranslator(DataSchema schema, ObjectContainerDalcStorage objStorage) {
			Schema = schema;
			ObjStorage = objStorage;
		}

		public Query TranslateSubQuery(Query query) {
			// is class query?
			var dataClass = Schema.FindClassByID(query.Table.Name);
			if (dataClass!=null) {
				var dataClassQuery = new Query( new QTable( ObjStorage.ObjectTableName, query.Table.Alias) );
				dataClassQuery.Condition = TranslateQueryNode( dataClass, query.Condition );
				// TBD: add support for any field
				CheckFieldsConstraint(query, "id");
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

			var pSrcName = ObjStorage.DataTypeTableNames[p.DataType.ID];

			var propQuery = new Query(pSrcName,
							(QField)"property_compact_id" == (QConst)p.CompactID
							&
							new QueryConditionNode((QField)"value", cnd, val)							
						) {
							Fields = new[] { (QField)"object_id" }
						};
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

			return new QueryConditionNode( (QField)"id", Conditions.In, propQuery );
		}

		protected QueryNode ComposePropertyCondition(Class dataClass, Property prop, Conditions cnd, IQueryValue val) {
			var pSrcName = ObjStorage.DataTypeTableNames[prop.DataType.ID];
			return new QueryConditionNode(
				new QField(null, "id", null),
				Conditions.In,
				new Query(pSrcName,
					(QField)"property_compact_id" == (QConst)prop.CompactID
					&
					new QueryConditionNode((QField)"value", cnd, val)
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
