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
using System.Data;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.Text;

namespace NI.Data
{
	/// <summary>
	/// Database sql builder.
	/// </summary>
	public class DbSqlBuilder : SqlBuilder, IDbSqlBuilder
	{
		protected const string SelectFromPartFormatStr = "SELECT {0} FROM {1}";
		protected const string SelectWherePartFormatStr = " WHERE {0}";
		protected const string SelectOrderPartFormatStr = " ORDER BY {0}";
		
		protected IDbCommandWrapper CmdWrapper;
		
	
		public DbSqlBuilder(IDbCommandWrapper cmdWrapper) {
			CmdWrapper = cmdWrapper;
		}
		
		protected virtual string GetTableName(string sourceName) {
			QSourceName qSourceName = (QSourceName)sourceName;
			if (!String.IsNullOrEmpty(qSourceName.Alias))
				return qSourceName.Name + " " + qSourceName.Alias;
			return qSourceName.Name;
		}

		protected virtual string BuildSelectInternal(Query query, bool isNested) {
			string fields = BuildFields(query);
			string sort = BuildSort(query);
			string whereExpression = BuildExpression(query.Condition);
			
			// compose select sql
			StringBuilder cmdTextBuilder = new StringBuilder();
			cmdTextBuilder.AppendFormat(
				SelectFromPartFormatStr, fields, GetTableName(query.SourceName) );
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
					var sortFld = (QSortField)query.Sort[i];
					sortFields[i] = BuildValue( (IQueryValue)sortFld );
				}
				
				return String.Join(",", sortFields);
			}
			return null;
		}

		public virtual string BuildFields(Query query) {
			// Compose fields part
			string[] fields = (query.Fields==null || query.Fields.Length==0) ?
					new string[] {"*"} :
					query.Fields.Select(v=>(string)v).ToArray();
					
			for (int i=0; i<fields.Length; i++)
				fields[i] = this.BuildValue( new QField(fields[i]) );
			return String.Join(",", fields);
		}

		protected override string BuildValue(IQueryValue value) {
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
			
			string paramName = String.Format("@p{0}", CmdWrapper.Command.Parameters.Count);
			
			IDbDataParameter param = CmdWrapper.CreateCmdParameter(value);
			param.ParameterName = paramName;
			CmdWrapper.Command.Parameters.Add(param);
			
			return CmdWrapper.GetCmdParameterPlaceholder(paramName);
		}
		
		public string BuildCommandParameter(DataColumn column, DataRowVersion sourceVersion) {
			string paramName = String.Format("@p{0}", CmdWrapper.Command.Parameters.Count);

			IDbDataParameter param = CmdWrapper.CreateCmdParameter(column);
			param.ParameterName = paramName;
			param.SourceVersion = sourceVersion;
			
			CmdWrapper.Command.Parameters.Add(param);
			return CmdWrapper.GetCmdParameterPlaceholder(paramName);
		}
		
		public string BuildSetExpression(string[] fieldNames, string[] fieldValues) {
			if (fieldNames.Length!=fieldValues.Length)
				throw new ArgumentException();
			ArrayList parts = new ArrayList();
			for (int i=0; i<fieldNames.Length; i++) {
				string condition = String.Format("{0}={1}",
					BuildValue( new QField(fieldNames[i]) ), fieldValues[i] );
				parts.Add( condition );
			}
			return String.Join(",", (string[])parts.ToArray(typeof(string)));
		}

	
		
	}
}
