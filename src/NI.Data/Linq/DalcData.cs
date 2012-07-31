#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NI.Data.Linq
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
