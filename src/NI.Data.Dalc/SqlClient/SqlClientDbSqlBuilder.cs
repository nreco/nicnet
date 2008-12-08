#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2008 NewtonIdeas
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

namespace NI.Data.Dalc.SqlClient {
	
	/// <summary>
	/// MS SQL optimized SQL builder
	/// </summary>
	public class SqlClientDbSqlBuilder : DbSqlBuilder {

		protected const string SelectTopFromPartFormatStr = "SELECT TOP {2} {0} FROM {1}";
		protected const string AsciiConstFormatStr = "'{0}'";
		bool TopOptimization = true;
		bool ConstOptimization = true;
		protected static Regex asciiConstRegex = new Regex("^[-_0-9A-Za-z ,.%]*$", RegexOptions.Singleline|RegexOptions.Compiled);
		
		public SqlClientDbSqlBuilder(IDbCommandWrapper cmdWrapper, bool enableTopOpt, bool enableConstOpt) : base(cmdWrapper) {
			TopOptimization = enableTopOpt;
			ConstOptimization = enableConstOpt;
		}
		
		protected override string BuildValue(IQueryConstantValue value) {
			if (ConstOptimization) {
				if (value.Type==TypeCode.String && (value.Value is string) && IsAsciiConst( (string)value.Value ))
					return String.Format(AsciiConstFormatStr, value.Value);
			}
			return base.BuildValue(value);
		}
		
		protected bool IsAsciiConst(string str) {
			if (str.Length>=50)
				return false; // skip opt for long strings
			return asciiConstRegex.IsMatch(str);
		}
		
		
		protected override string BuildSelectInternal(IQuery query, bool isNested) {
			if (!TopOptimization)
				return base.BuildSelectInternal(query, isNested);
			
			string fields = BuildFields(query);
			string sort = BuildSort(query);
			string whereExpression = BuildExpression(query.Root);
			
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
