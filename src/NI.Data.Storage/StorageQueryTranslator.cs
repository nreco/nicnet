using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NI.Data.Storage.Model;

namespace NI.Data.Storage {
	
	public class StorageQueryTranslator {
		
		protected DataSchema Schema;

		public StorageQueryTranslator(DataSchema schema) {
			Schema = schema;
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

		protected QueryConditionNode TranslateConditionNode(Class dataClass, QueryConditionNode node) {
			if (node.LValue is QField && node.RValue is QField)
				throw new NotSupportedException("Cannot compare 2 fields");

			//var cond = new QueryConditionNode( (QField)"id", Conditions.In, 

			//cond.LValue = TranslateQueryValue( cond.LValue );
			//cond.RValue = TranslateQueryValue( cond.RValue );

			return null;
		}


	}
}
