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

		public QueryNode TranslateQueryNode(Class targetClass, QueryNode condition) {
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
			if (node.LValue is QField && node.RValue is QField)
				throw new NotSupportedException("Cannot compare 2 fields");

			if (node.LValue is QField) {
				return ObjStorage.ComposeFieldCondition(dataClass, (QField)node.LValue, node.Condition, node.RValue );
			}
			if (node.RValue is QField) {
				var cnd = node.Condition;
				if ( (cnd & Conditions.GreaterThan)==Conditions.GreaterThan ) {
					cnd = (cnd & ~Conditions.GreaterThan) | Conditions.LessThan;
				} else if ((cnd & Conditions.LessThan) == Conditions.LessThan) {
					cnd = (cnd & ~Conditions.LessThan) | Conditions.GreaterThan;
				}
				return ObjStorage.ComposeFieldCondition(dataClass, (QField)node.RValue, cnd, node.RValue );
			}
			return node;
		}



	}
}
