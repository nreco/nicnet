using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NI.Data.Dalc.Linq
{
	public class DalcData<TData> : IOrderedQueryable<TData> {
        QueryProvider QueryPrv;
        Expression Expr;

        internal DalcData(QueryProvider provider)
        {
            QueryPrv = provider;
            Expr = Expression.Constant(this);
        }

        internal DalcData(QueryProvider provider, Expression expression) {
            QueryPrv = provider;
            Expr = expression;
		}

        public IEnumerator<TData> GetEnumerator() {
            return (QueryPrv.Execute<IEnumerable<TData>>(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
             return (QueryPrv.Execute<IEnumerable>(Expression)).GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof(TData); }
        }

        public Expression Expression
        {
            get { return Expr; }
        }

        public System.Linq.IQueryProvider Provider
        {
            get { return QueryPrv; }
        }

    }
}
