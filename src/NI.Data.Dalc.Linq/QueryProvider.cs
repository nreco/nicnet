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
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NI.Data.Dalc.Linq
{
	public class QueryProvider : System.Linq.IQueryProvider {
		string SourceName;
		IDalc Dalc;

		public QueryProvider(string sourceName, IDalc dalc)
		{
			SourceName = sourceName;
			Dalc = dalc;
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			return new DalcData<TElement>(this, expression);
		}

		public IQueryable CreateQuery(Expression expression)
		{
			var type = expression.Type;
			if (!type.IsGenericType)
				throw new Exception("Unknown expression type");
			var genericType = type.GetGenericTypeDefinition();
			if (genericType == typeof(IQueryable<>) || genericType == typeof(IOrderedQueryable<>))
				type = type.GetGenericArguments()[0];

			try {
				return (IQueryable)Activator.CreateInstance(
						typeof(DalcData<>).MakeGenericType(type),
						new object[] { this, expression });
			} catch (System.Reflection.TargetInvocationException tie) {
				throw tie.InnerException;
			}
		}

		public TResult Execute<TResult>(Expression expression)
		{
			Query q = new Query(SourceName);
			BuildDalcQuery(q, expression);
			
			Console.WriteLine(q.ToString());
			DataSet ds = new DataSet();
			Dalc.Load(ds, q);
			Type resT = typeof(TResult);
			DataRowCollection dataRows = ds.Tables[q.SourceName].Rows;
			if (resT == typeof(IEnumerable))
				return (TResult)((object)dataRows);
			if (resT.IsGenericType && resT.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
				Type[] genArgs = resT.GetGenericArguments();
				Array resArr = Array.CreateInstance(genArgs[0], dataRows.Count);
				for (int i = 0; i < dataRows.Count; i++) {
					if (genArgs[0] == typeof(DalcRecord)) {
						resArr.SetValue(new DalcRecord( new NI.Common.Collections.DataRowDictionary(dataRows[i]) ), i);
					} else if (genArgs[0] == typeof(IDictionary))
						resArr.SetValue(new NI.Common.Collections.DataRowDictionary(dataRows[i]), i);
					else if (genArgs[0] == typeof(DataRow))
						resArr.SetValue(dataRows[i], i);
					else
						throw new InvalidCastException();
				}
				return (TResult)((object)resArr);
			}
			throw new InvalidCastException();
		}

		public object Execute(Expression expression)
		{
			return Execute<IEnumerable>(expression);
		}


		protected void BuildDalcQuery(Query q, Expression expression)
		{
			if (expression is MethodCallExpression) {
				MethodCallExpression call = (MethodCallExpression)expression;
				if (call.Arguments.Count != 2)
					throw new NotSupportedException();
				Expression sourceExpr = call.Arguments[0];
				BuildDalcQuery(q, sourceExpr);
				switch (call.Method.Name)
				{
					case "Linq":
						ConstantExpression sourceNameConst = (ConstantExpression)call.Arguments[1];
						q.SourceName = sourceNameConst.Value.ToString();
						break;
					case "Where":
						q.Root = ComposeCondition(call.Arguments[1]);
						if (call.Arguments[1] is UnaryExpression) {
							UnaryExpression unExpr = (UnaryExpression)call.Arguments[1];
							if (unExpr.Operand is LambdaExpression) {
								LambdaExpression lambdaExpr = (LambdaExpression)unExpr.Operand;
								if (lambdaExpr.Parameters.Count == 1)
									q.SourceName += "." + lambdaExpr.Parameters[0].Name;
							}
						}
						break;
					case "Select":
						q.Fields = new string[] { ComposeFieldValue(call.Arguments[1]).Name };
						break;
					case "OrderBy":
						AddQuerySort(q, ComposeFieldValue(call.Arguments[1]).Name );
						break;
					case "OrderByDescending":
						AddQuerySort(q, ComposeFieldValue(call.Arguments[1]).Name + " " + QSortField.Desc);
						break;	
					case "ThenBy":
						AddQuerySort(q, ComposeFieldValue(call.Arguments[1]).Name );
						break;
					case "ThenByDescending":
						AddQuerySort(q, ComposeFieldValue(call.Arguments[1]).Name + " " + QSortField.Desc);
						break;
				}
			}
		}

		protected void AddQuerySort(Query q, string sortFld) {
			if (q.Sort != null) {
				string[] newSort = new string[q.Sort.Length + 1];
				Array.Copy(q.Sort, newSort, q.Sort.Length);
				newSort[q.Sort.Length] = sortFld;
				q.Sort = newSort;
			} else {
				q.Sort = new string[] { sortFld };
			}
		}

		protected IQueryNode ComposeCondition(Expression expression) {
			if (expression is UnaryExpression) {
				UnaryExpression unExpr = (UnaryExpression)expression;
				IQueryNode qNode = ComposeCondition(unExpr.Operand);
				return (unExpr.NodeType==ExpressionType.Not ? new QueryNegationNode(qNode) : qNode );
			}
			if (expression is LambdaExpression) {
				LambdaExpression lambdaExpr = (LambdaExpression)expression;
				return ComposeCondition(lambdaExpr.Body);
			}
			if (expression is BinaryExpression) {
				BinaryExpression binExpr = (BinaryExpression)expression;
				if (binExpr.NodeType == ExpressionType.AndAlso || binExpr.NodeType == ExpressionType.OrElse) {
					QueryGroupNode qGroup = new QueryGroupNode(binExpr.NodeType == ExpressionType.AndAlso ? GroupType.And : GroupType.Or);
					qGroup.Nodes.Add(ComposeCondition(binExpr.Left));
					qGroup.Nodes.Add(ComposeCondition(binExpr.Right));
					return qGroup;
				}
				if (conditionMapping.ContainsKey(binExpr.NodeType)) {
					Conditions qCond = conditionMapping[binExpr.NodeType];
					QueryConditionNode qCondNode = new QueryConditionNode(
							ComposeValue(binExpr.Left), qCond, ComposeValue(binExpr.Right) );
					return qCondNode;
				}
			}
			else if (expression is MethodCallExpression) {
				MethodCallExpression methodExpr = (MethodCallExpression)expression;
				if (methodExpr.Method.Name == "In") { // check for special method call like 'In' or 'Like'
					IQueryFieldValue fldValue = ComposeFieldValue(methodExpr.Object);
					IQueryValue inValue = ComposeValue(methodExpr.Arguments[0]);
					// possible conversion to IList
					if (inValue is IQueryConstantValue) {
						IQueryConstantValue inConstValue = (IQueryConstantValue)inValue;
						if (!(inConstValue.Value is IList)) {
							IList constList = new ArrayList();
							foreach (object o in ((IEnumerable)inConstValue.Value))
								constList.Add(o);
							inValue = new QConst(constList);
						}
					}
					return new QueryConditionNode(fldValue, Conditions.In, inValue);
				}
			}

			throw new NotSupportedException();
		}

		static IDictionary<ExpressionType, Conditions> conditionMapping;
		
		static QueryProvider() {
			conditionMapping = new Dictionary<ExpressionType,Conditions>();
			conditionMapping[ExpressionType.Equal] = Conditions.Equal;
			conditionMapping[ExpressionType.NotEqual] = Conditions.Equal|Conditions.Not;
			conditionMapping[ExpressionType.GreaterThan] = Conditions.GreaterThan;
			conditionMapping[ExpressionType.GreaterThanOrEqual] = Conditions.GreaterThan|Conditions.Equal;
			conditionMapping[ExpressionType.LessThan] = Conditions.LessThan;
			conditionMapping[ExpressionType.LessThanOrEqual] = Conditions.LessThan | Conditions.Equal;
		}

		protected IQueryFieldValue ComposeFieldValue(Expression expression) {
			IQueryValue fldValue = ComposeValue(expression);
			if (fldValue is IQueryFieldValue)
				return (IQueryFieldValue)fldValue;
			else
				throw new NotSupportedException();
		}

		protected IQueryValue ComposeValue(Expression expression) {
			if (expression is UnaryExpression) {
				UnaryExpression unExpr = (UnaryExpression)expression;
				return ComposeValue(unExpr.Operand);
			}
			if (expression is LambdaExpression) {
				LambdaExpression lambdaExpr = (LambdaExpression)expression;
				return ComposeValue(lambdaExpr.Body);
			}
			if (expression is MethodCallExpression) {
				MethodCallExpression methodExpr = (MethodCallExpression)expression;
				if (methodExpr.Method.Name == "get_Item") {
					if (methodExpr.Arguments.Count == 1 && 
						methodExpr.Arguments[0] is ConstantExpression &&
						methodExpr.Object is ParameterExpression) {
						ConstantExpression fldNameExpr = (ConstantExpression)methodExpr.Arguments[0];
						// lets extract prefix
						ParameterExpression paramExpr = (ParameterExpression)methodExpr.Object;
						return new QField(paramExpr.Name+"."+fldNameExpr.Value.ToString());
					}
				} else if (methodExpr.Method.Name == "Select") {
					Query nestedQ = new Query(String.Empty);
					BuildDalcQuery(nestedQ, methodExpr);
					if (!String.IsNullOrEmpty(nestedQ.SourceName))
						return nestedQ;
				}

				throw new NotSupportedException();
			}

			if (expression is ConstantExpression) {
				ConstantExpression constExpr = (ConstantExpression)expression;
				return new QConst(constExpr.Value);
			}

			// just try to eval
			LambdaExpression lExpr = Expression.Lambda(expression);
			return new QConst(lExpr.Compile().DynamicInvoke(null));
		}

    }
}
