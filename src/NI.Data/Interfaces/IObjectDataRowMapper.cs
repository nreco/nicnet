using System;
using System.Collections.Generic;
using System.Text;

using System.Data;

namespace NI.Data {
	
	/// <summary>
	/// Represents set of methods for two-directional mapping between DataRow and object
	/// </summary>
	public interface IObjectDataRowMapper {

		/// <summary>
		/// Map DataRow values to specified object
		/// </summary>
		/// <param name="r">source DataRow</param>
		/// <param name="o">target object</param>
		void MapTo(DataRow r, object o);

		/// <summary>
		/// Map object data to DataRow
		/// </summary>
		/// <param name="o">source object</param>
		/// <param name="r">target DataRow</param>
		/// <param name="skipPk">skip mapping for PK values</param>
		void MapFrom(object o, DataRow r, bool skipPk);
		
		/// <summary>
		/// Get object's value by specified DataColumn
		/// </summary>
		/// <param name="o">object</param>
		/// <param name="c">DataColumn</param>
		/// <returns>value which corresponds to specified DataColumn</returns>
		object GetFieldValue(object o, DataColumn c);
	}
}
