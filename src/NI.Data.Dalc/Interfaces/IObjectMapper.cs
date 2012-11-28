using System;
using System.Collections.Generic;
using System.Text;

using System.Data;

namespace NI.Data.Dalc {
	
	public interface IObjectMapper {
		void MapTo(DataRow r, object o);
		void MapFrom(object o, DataRow r, bool skipPk);
		object GetFieldValue(object o, DataColumn c);
	}
}
