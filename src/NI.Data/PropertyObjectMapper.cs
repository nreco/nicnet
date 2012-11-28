using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using System.Data;

namespace NI.Data {

	public class PropertyObjectMapper : IObjectMapper {

		public IDictionary<string, string> FieldToProperty { get; private set; }

		public PropertyObjectMapper(IDictionary<string, string> fieldToProperty) {
			FieldToProperty = fieldToProperty;
		}

		public virtual object GetFieldValue(object o, DataColumn c) {
			var pInfo = o.GetType().GetProperty(GetPropertyName(c.ColumnName));
			if (pInfo == null)
				return null;
			return pInfo.GetValue(o, null);
		}

		public virtual void MapTo(DataRow r, object o) {
			foreach (DataColumn c in r.Table.Columns) {
				var pInfo =o.GetType().GetProperty(GetPropertyName(c.ColumnName));
				if (pInfo != null) {
					var rVal = r[c];
					if (rVal == null || DBNull.Value.Equals(rVal)) {
						rVal = null;
						if (Nullable.GetUnderlyingType(pInfo.PropertyType) == null && pInfo.PropertyType.IsValueType)
							rVal = Activator.CreateInstance(pInfo.PropertyType);
					} else {
						var propType = pInfo.PropertyType;
						if (Nullable.GetUnderlyingType(propType) != null)
							propType = Nullable.GetUnderlyingType(propType);

						rVal = Convert.ChangeType(rVal, propType, CultureInfo.InvariantCulture);
					}
					pInfo.SetValue(o, rVal, null);
				}
			}

		}

		public virtual void MapFrom(object o, DataRow r, bool skipPk) {
			foreach (DataColumn c in r.Table.Columns) {
				var pInfo = o.GetType().GetProperty(GetPropertyName(c.ColumnName));
				if (pInfo == null)
					continue;
				if (skipPk && Array.IndexOf(r.Table.PrimaryKey, c) >= 0)
					continue;

				var pVal = pInfo.GetValue(o, null);
				if (pVal == null) {
					pVal = DBNull.Value;
				} else {
					pVal = Convert.ChangeType(pVal, c.DataType, CultureInfo.InvariantCulture);
				}
				r[c] = pVal;
			}

		}

		protected string GetPropertyName(string fldName) {
			return FieldToProperty != null && FieldToProperty.ContainsKey(fldName) ? FieldToProperty[fldName] : fldName;
		}


	}

}
