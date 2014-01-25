#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
 * Copyright 2008-2013 Vitalii Fedorchenko (changes and v.2)
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
using System.Data;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NI.Data
{
	/// <summary>
	/// Database sql builder (default implementation).
	/// </summary>
	public class DbSqlBuilder : SqlBuilder, IDbSqlBuilder
	{
		protected const string SelectFromPartFormatStr = "SELECT {0} FROM {1}";
		protected const string SelectWherePartFormatStr = " WHERE {0}";
		protected const string SelectOrderPartFormatStr = " ORDER BY {0}";

		protected IDbCommand Command;
		protected IDbProviderFactory DalcFactory;

		public DbSqlBuilder(IDbCommand cmd, IDbProviderFactory dalcFactory) {
			Command = cmd;
			DalcFactory = dalcFactory;
		}
		
		protected virtual string GetTableName(string tableName) {
			QTable table = (QTable)tableName;
			if (!String.IsNullOrEmpty(table.Alias))
				return table.Name + " " + table.Alias;
			return table.Name;
		}

		protected virtual string BuildSelectInternal(Query query, bool isNested) {
			string fields = BuildFields(query);
			string sort = BuildSort(query);
			string whereExpression = BuildExpression(query.Condition);
			
			// compose select sql
			StringBuilder cmdTextBuilder = new StringBuilder();
			cmdTextBuilder.AppendFormat(
				SelectFromPartFormatStr, fields, GetTableName(query.Table) );
			if (whereExpression!=null && whereExpression.Length>0)
				cmdTextBuilder.AppendFormat(SelectWherePartFormatStr, whereExpression);
			if (sort!=null)
				cmdTextBuilder.AppendFormat(SelectOrderPartFormatStr, sort);
			
			return cmdTextBuilder.ToString();
		}

		public virtual string BuildSelect(Query query) {
			return BuildSelectInternal(query, false);
		}


		public virtual string BuildSort(Query query) {
			// Compose 'order by' part
			if (query.Sort!=null && query.Sort.Length>0) {
				string[] sortFields = new string[query.Sort.Length];
				for (int i=0; i<sortFields.Length; i++) {
					var sortFld = (QSort)query.Sort[i];
					sortFields[i] = BuildSort( sortFld );
				}
				
				return String.Join(",", sortFields);
			}
			return null;
		}

		public virtual string BuildFields(Query query) {
			// Compose fields part
			if (query.Fields == null || query.Fields.Length == 0)
				return "*";

			var joinFields = new List<string>();
			foreach (var f in query.Fields) {
				var fld = BuildValue((IQueryValue)f);
				if (fld != f.Name) { //skip "as" for usual fields
					fld = fld + " as " + f.Name;
				}
				joinFields.Add(fld);
			}
			return String.Join(",", joinFields.ToArray() );
		}

		public override string BuildValue(IQueryValue value) {
			if (value is Query)
				return "("+BuildSelectInternal( (Query)value, true )+")";

			return base.BuildValue(value);
		}
		
		protected override string BuildValue(QConst value) {
			object constValue = value.Value;
				
			// special processing for arrays
			if (constValue is IList)
				return BuildValue( (IList)constValue );
									
			return BuildCommandParameter(constValue);
		}		
		
		protected override string BuildValue(string str) {
			return BuildCommandParameter( str );
		}
		
		public string BuildCommandParameter(object value) {
			if (value is DataColumn) return ((DataColumn)value).ColumnName;
			return DalcFactory.AddCommandParameter(Command,value);
		}

		public string BuildCommandParameter(DataColumn column, DataRowVersion sourceVersion) {
			return DalcFactory.AddCommandParameter(Command, column, sourceVersion);
		}

	
		
	}
}
