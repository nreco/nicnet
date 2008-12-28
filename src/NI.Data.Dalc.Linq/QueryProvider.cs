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
					if (genArgs[0] == typeof(IDictionary))
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
				Console.WriteLine(call.Method.Name);
				switch (call.Method.Name)
				{
					case "Where":
						q.Root = ComposeCondition(call.Arguments[1]);
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
				MethodCallExpression getItemMethodExpr = (MethodCallExpression)expression;
				if (getItemMethodExpr.Method.Name == "get_Item") {
					if (getItemMethodExpr.Arguments.Count == 1 && getItemMethodExpr.Arguments[0] is ConstantExpression) {
						ConstantExpression fldNameExpr = (ConstantExpression)getItemMethodExpr.Arguments[0];
						return new QField(fldNameExpr.Value.ToString());
					}
				}
			}

			if (expression is ConstantExpression) {
				ConstantExpression constExpr = (ConstantExpression)expression;
				return new QConst(constExpr.Value);
			}
			throw new NotSupportedException();
		}

    }
}
