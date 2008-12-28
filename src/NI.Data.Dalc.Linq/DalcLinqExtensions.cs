using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NI.Data.Dalc.Linq
{
    public static class DalcLinqExtensions
    {
        public static IQueryable<T> Linq<T>(this IDalc dalc, string sourceName)
        {
            return new DalcData<T>(new QueryProvider(sourceName, dalc));
        }

		
    }
}
