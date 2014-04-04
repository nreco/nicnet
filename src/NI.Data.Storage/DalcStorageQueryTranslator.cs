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

		protected QueryNode TranslateConditionNode(Class dataClass, QueryConditionNode node) {
			var lValProperty = node.LValue is QField ? dataClass.FindPropertyByID( ((QField)node.LValue).Name ) : null;
			var rValProperty = node.RValue is QField ? dataClass.FindPropertyByID( ((QField)node.RValue).Name ) : null;

			if (lValProperty != null && rValProperty!=null)
				throw new NotSupportedException("Cannot compare 2 class properties");

			if (lValProperty!=null) {
				return ObjStorage.ComposePropertyCondition(dataClass, lValProperty, node.Condition, 
					TranslateQueryValue( node.RValue ) );
			}
			if (rValProperty!=null) {
				var cnd = node.Condition;
				if ( (cnd & Conditions.GreaterThan)==Conditions.GreaterThan ) {
					cnd = (cnd & ~Conditions.GreaterThan) | Conditions.LessThan;
				} else if ((cnd & Conditions.LessThan) == Conditions.LessThan) {
					cnd = (cnd & ~Conditions.LessThan) | Conditions.GreaterThan;
				}
				return ObjStorage.ComposePropertyCondition(dataClass, rValProperty, cnd, TranslateQueryValue(node.LValue));
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
