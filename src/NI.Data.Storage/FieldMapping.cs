using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NI.Data.Storage {
	public class FieldMapping {
		QField[] Fields;
		QField[] CompactFields;
		IDictionary<string, string> DirectMapping;
		IDictionary<string, string> RevMapping;
		public FieldMapping(QField[] fields) {
			Fields = fields;
			DirectMapping = new Dictionary<string, string>();
			RevMapping = new Dictionary<string, string>();
			if (fields != null && fields.Length > 0) {
				CompactFields = new QField[Fields.Length];
				for (int i = 0; i < CompactFields.Length; i++) {
					var f = Fields[i];
					if (f.Prefix != null && f.Expression == null) {
						var originalFieldName = f.ToString().Replace('.', '_');
						var compactName = "f_" + i.ToString() + "_" + f.Name;
						RevMapping[compactName] = originalFieldName;
						DirectMapping[originalFieldName] = compactName;
						CompactFields[i] = new QField(compactName, f.ToString());
					} else {
						CompactFields[i] = f;
					}
				}
			} else {
				CompactFields = Fields;
			}
		}

		public QField[] GetCompactFields() {
			return CompactFields;
		}

		public string GetOriginalFieldName(string compactName) {
			return RevMapping.ContainsKey(compactName) ? RevMapping[compactName] : compactName;
		}

		public string GetCompactFieldName(string originalName) {
			return DirectMapping.ContainsKey(originalName) ? DirectMapping[originalName] : originalName;
		}
	}
}
