using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NI.Data.Dalc.Linq
{
	public class DalcData<TData> : IOrderedQueryable<TData> {


		public DalcData(IDalc provider, Expression expression) {

		}

	}
}
