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
using System.Text;
using System.Text.RegularExpressions;

namespace NI.Data.SqlClient {
	
	/// <summary>
	/// MS SQL optimized SQL builder
	/// </summary>
	public class SqlClientDbSqlBuilder : DbSqlBuilder {

		protected const string SelectTopFromPartFormatStr = "SELECT TOP {2} {0} FROM {1}";
		protected const string AsciiConstFormatStr = "'{0}'";
		bool TopOptimization = true;
		bool ConstOptimization = true;
		bool UseNameBrackets = false;
		protected static Regex asciiConstRegex = new Regex("^[-_0-9A-Za-z ,.%]*$", RegexOptions.Singleline|RegexOptions.Compiled);
		protected const string BracketFormatStr = "[{0}]";

		public SqlClientDbSqlBuilder(IDbCommandWrapper cmdWrapper, bool enableTopOpt, bool enableConstOpt, bool enableNameBrackets) : base(cmdWrapper) {
			TopOptimization = enableTopOpt;
			ConstOptimization = enableConstOpt;
			UseNameBrackets = enableNameBrackets;
		}

		protected string FormatInBrackets(string s) {
			string[] parts = s.Split( new[]{'.'}, StringSplitOptions.RemoveEmptyEntries);
			for (int i=0; i<parts.Length; i++)
				if (parts[i][0] != '[' && !Char.IsDigit(parts[i][0]))
					parts[i] = String.Format(BracketFormatStr, parts[i]);
			return String.Join(".", parts);
		}

		protected override string GetTableName(string sourceName) {
			if (UseNameBrackets) {
				QSourceName qSourceName = (QSourceName)sourceName;
				if (!String.IsNullOrEmpty(qSourceName.Alias))
					return FormatInBrackets(qSourceName.Name) + " " + qSourceName.Alias;
				return FormatInBrackets(qSourceName.Name);
			}
			return base.GetTableName(sourceName);
		}		

		protected override string BuildValue(IQueryConstantValue value) {
			if (ConstOptimization) {
				if (value.Type==TypeCode.String && (value.Value is string) && IsAsciiConst( (string)value.Value ))
					return String.Format(AsciiConstFormatStr, value.Value);
			}
			return base.BuildValue(value);
		}

		protected override string BuildValue(IQueryFieldValue fieldValue) {
			string fldName = base.BuildValue(fieldValue);
			if (UseNameBrackets) {
				// additional check: base method may return SQL code for "virtual" field names
				if (fldName == fieldValue.Name && !IsSqlExpression(fldName))
					return FormatInBrackets(fldName);
			}
			return fldName;
		}
		
		protected bool IsSqlExpression(string fieldName) {
			return fieldName.IndexOfAny(new[] {'(', ')', '+', '-', '*', '/', ' '})>=0;
		}
		
		
		protected bool IsAsciiConst(string str) {
			if (str.Length>=50)
				return false; // skip opt for long strings
			return asciiConstRegex.IsMatch(str);
		}
		
		
		protected override string BuildSelectInternal(Query query, bool isNested) {
			if (!TopOptimization)
				return base.BuildSelectInternal(query, isNested);
			
			string fields = BuildFields(query);
			string sort = BuildSort(query);
			string whereExpression = BuildExpression(query.Condition);
			
			// compose select sql
			StringBuilder cmdTextBuilder = new StringBuilder();
			if (query.RecordCount<Int32.MaxValue) {
				// MS SQL optimized select with TOP
				cmdTextBuilder.AppendFormat(
					SelectTopFromPartFormatStr,
					fields, 
					GetTableName(query.SourceName),
					query.RecordCount+query.StartRecord );
			} else {
				// standard SQL
				cmdTextBuilder.AppendFormat(
					SelectFromPartFormatStr, fields, GetTableName(query.SourceName) );
			}
			if (whereExpression!=null && whereExpression.Length>0)
				cmdTextBuilder.AppendFormat(SelectWherePartFormatStr, whereExpression);
			if (sort!=null)
				cmdTextBuilder.AppendFormat(SelectOrderPartFormatStr, sort);
			
			return cmdTextBuilder.ToString();
		}


	}
}
