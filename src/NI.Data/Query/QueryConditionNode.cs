#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
 * Copyright 2008-2013 Vitalii Fedorchenko (changes and v.2)
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
using System.Collections.Generic;
using System.Diagnostics;


namespace NI.Data {

	[DebuggerDisplay("{LValue} {Condition} {RValue}")]
	[Serializable]
	public class QueryConditionNode : QueryNode {
		
		private IQueryValue _LValue;
		private Conditions _Condition;
		private IQueryValue _RValue;

		public IQueryValue LValue {
			get { return _LValue; }
		}

		public Conditions Condition {
			get { return _Condition; }
		} 
	
		public IQueryValue RValue {
			get { return _RValue; }
		}
		
		public override IList<QueryNode> Nodes { 
			get {
				var l = new List<QueryNode>();
				if (LValue is QueryNode)
					l.Add( (QueryNode)LValue );
				if (RValue is QueryNode)
					l.Add( (QueryNode)RValue );
				return l; 
			}
		}

	
		public QueryConditionNode(IQueryValue lvalue, Conditions conditions, IQueryValue rvalue) {
			_RValue = rvalue;
			_Condition = conditions;
			_LValue = lvalue;
		}

		public QueryConditionNode(string name, IQueryValue lvalue, Conditions conditions, IQueryValue rvalue) : 
			this(lvalue, conditions, rvalue) {
			Name = name;
		}

		public QueryConditionNode(QueryConditionNode node) {
			Name = node.Name;
			_LValue = node.LValue;
			_Condition = node.Condition;
			_RValue = node.RValue;
		}

	}

	[Flags]
	public enum Conditions {
		Equal = 1,
		LessThan = 2,
		GreaterThan = 4,
		Like = 8,
		In = 16,
		Null = 32,
		Not = 64
	}

}
