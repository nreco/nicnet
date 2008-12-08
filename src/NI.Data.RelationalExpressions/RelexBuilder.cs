#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2008 NewtonIdeas
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
using System.Globalization;
using System.Text;
using NI.Data.Dalc;

using NI.Common.Providers;

namespace NI.Data.RelationalExpressions {
	
	public class RelexBuilder : IStringProvider, IObjectProvider {
		
		public string BuildRelex(IQueryNode node) {
			InternalBuilder builder = new InternalBuilder();
			return builder.BuildExpression(node);
		}
		
		public string GetString(object context) {
			if (!(context is IQueryNode))
				throw new Exception("Expected IQueryNode object");
			return BuildRelex( (IQueryNode)context );
		}

		public object GetObject(object context) {
			return GetString(context);
		}

		class InternalBuilder : SqlBuilder {

			public override string BuildExpression(IQueryNode node) {
				if (node is IQuery)
					return BuildQueryString((IQuery)node, false);
				return base.BuildExpression(node);
			}

			public string BuildQueryString(IQuery q, bool isNested) {
				string rootExpression = BuildExpression(q.Root);
				if (rootExpression != null && rootExpression.Length > 0)
					rootExpression = String.Format("({0})", rootExpression);
				string fieldExpression = q.Fields != null ? String.Join(",", q.Fields) : "*";
				string limitExpression = isNested || (q.StartRecord==0 && q.RecordCount==Int32.MaxValue) ? 
					String.Empty : String.Format("{{{0},{1}}}", q.StartRecord, q.RecordCount);
				return String.Format("{0}{1}[{2}]{3}", q.SourceName, rootExpression,
					fieldExpression, limitExpression);
			}

			static readonly string[] stringConditions = new string[] {
					"=", ">", ">=", "<", "<=", " in ", " like ", "="
			};
			static readonly Conditions[] enumConditions = new Conditions[] {
					Conditions.Equal, Conditions.GreaterThan, 
					Conditions.GreaterThan|Conditions.Equal,
					Conditions.LessThan, Conditions.LessThan|Conditions.Equal,
					Conditions.In, Conditions.Like, Conditions.Null
			};

			protected override string BuildCondition(IQueryConditionNode node) {
				string lvalue = BuildValue(node.LValue);
				string rvalue = BuildValue(node.RValue);
				Conditions condition = (node.Condition | Conditions.Not) ^ Conditions.Not;
				string res = null;
				for (int i=0; i<enumConditions.Length; i++)
					if (enumConditions[i]==condition) {
						res = stringConditions[i];
						break; // first match
					}
				if (res==null)
					throw new ArgumentException("Invalid conditions set", condition.ToString());
				if ((node.Condition & Conditions.Not)==Conditions.Not)
					res = "!" + res;
				if ((node.Condition & Conditions.Null) == Conditions.Null)
					rvalue = "null";
				return String.Format("{0}{1}{2}", lvalue, res, rvalue);
			}
			

			protected override string BuildValue(IQueryValue value) {
				if (value is IQuery)
					return BuildQueryString((IQuery)value, true);
				return base.BuildValue(value);
			}

			protected override string BuildValue(IQueryConstantValue value) {
				IQueryConstantValue qConst = (IQueryConstantValue)value;
				object constValue = qConst.Value;
				if (constValue == null)
					return "null";
				
				// special processing for arrays
				if (constValue is IList)
					return BuildValue((IList)constValue);
				if (constValue is string && qConst.Type==TypeCode.String)
					return BuildValue((string)constValue);

				TypeCode constTypeCode = ((IQueryConstantValue)value).Type;
				string typeSuffix = constTypeCode!=TypeCode.Empty && constTypeCode!=TypeCode.DBNull ? ":"+constTypeCode.ToString() : String.Empty;
				return BuildValue( Convert.ToString(constValue, CultureInfo.InvariantCulture ) ) + typeSuffix;
			}

			protected virtual string BuildValue(IList list) {
				string[] paramNames = new string[list.Count];
				// in relexes only supported arrays that can be represented as comma-delimeted string 
				for (int i = 0; i < list.Count; i++)
					paramNames[i] = Convert.ToString(list[i]);
				return BuildValue( String.Join(",", paramNames) ) + ":string[]"; // TODO: array type suggestion logic!
			}			
			
			protected override string BuildValue(string str) {
				return "\""+str.Replace("\"", "\"\"")+"\"";
			}			

		}		
		
	}
}
