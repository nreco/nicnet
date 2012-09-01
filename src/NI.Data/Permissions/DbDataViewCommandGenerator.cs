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
using System.Threading;
using System.Security.Principal;
using System.Collections;
using System.Text.RegularExpressions;

using NI.Common;

namespace NI.Data.Permissions
{
	/// <summary>
	/// </summary>
	public class DbDataViewCommandGenerator : NI.Data.DbDataViewCommandGenerator
	{
		static string SourceNameOriginsRegexPattern = @"^(\s*(?<sourceName>[^\s,]+)(\s*(?<alias>[^\s,]*?))\s*(,|$))+";
		static Regex SourceNameOriginsRegex = new Regex(SourceNameOriginsRegexPattern, RegexOptions.Compiled|RegexOptions.Singleline);
		
		IDalcConditionComposer _DalcConditionComposer;
		
		public IDalcConditionComposer DalcConditionComposer {
			get { return _DalcConditionComposer; }
			set { _DalcConditionComposer = value; }
		}
		
		public DbDataViewCommandGenerator()
		{
		}
		
		protected IPrincipal ContextUser {
			get {
				return Thread.CurrentPrincipal;
			}
		}		
		
		protected override IDictionary BuildSqlCommandContext(IDbCommandWrapper cmdWrapper, IDbDataView dataView, Query query) {
			IDictionary context = base.BuildSqlCommandContext (cmdWrapper, dataView, query);
			// if origin does not specified, skip permission-conditions generation
			if (dataView.SourceNameOrigin==null)
				return context;
			
			// if origin more than one, or alias specified - generate '<table-alias>-whereExpression' tokens
			Match m = SourceNameOriginsRegex.Match(dataView.SourceNameOrigin);
			for (int i=0; i<m.Groups["sourceName"].Captures.Count; i++) {
				string sourceName = m.Groups["sourceName"].Captures[i].Value;
				string alias = m.Groups["alias"].Captures[i].Value;
				string whereExpressionPrefix = alias.Length>0 ? alias : sourceName;

				QueryNode permissionCondition = DalcConditionComposer.Compose(ContextUser, DalcOperation.Retrieve, sourceName);
				IDbSqlBuilder dbSqlBuilder = cmdWrapper.CreateSqlBuilder();
				if (alias.Length>0)
					dbSqlBuilder.QueryFieldValueFormatter = InsertFormatter(
						dbSqlBuilder.QueryFieldValueFormatter,
						new PrefixQueryFieldValueFormatter(sourceName, alias) );
				context[whereExpressionPrefix+"-permissionWhereExpression"] = 
					IsolateWhereExpression( dbSqlBuilder.BuildExpression(permissionCondition) );
			}
			
			return context;
		}

		
		protected override string BuildWhereExpression(IDbSqlBuilder dbSqlBuilder, IDbDataView dataView, Query query) {
			// if origin does not specified, skip permission-conditions generation
			if (dataView.SourceNameOrigin==null)
				return base.BuildWhereExpression(dbSqlBuilder, dataView, query);

			// if origin more than one, or alias specified - skip permission-conditions generation
			Match m = SourceNameOriginsRegex.Match(dataView.SourceNameOrigin);
			if (!m.Success || m.Groups["sourceName"].Captures.Count>1)
				return base.BuildWhereExpression(dbSqlBuilder, dataView, query);

			// add one more field-formatter to the formatters chain
			// if source name alias specified
			if (m.Groups["alias"].Captures[0].Length>0)
				dbSqlBuilder.QueryFieldValueFormatter = InsertFormatter(
					dbSqlBuilder.QueryFieldValueFormatter,
					new PrefixQueryFieldValueFormatter(
						m.Groups["sourceName"].Captures[0].Value,
						m.Groups["alias"].Captures[0].Value) );
			
			// compose permission condition
			QueryNode permissionCondition = DalcConditionComposer.Compose(ContextUser, DalcOperation.Retrieve, m.Groups["sourceName"].Captures[0].Value);
			QueryNode condition = query.Condition;
			QueryGroupNode groupAnd = new QueryGroupNode(GroupType.And);
			if (condition != null)
				groupAnd.Nodes.Add(condition);
			if (permissionCondition != null)
				groupAnd.Nodes.Add( permissionCondition );
			
			return dbSqlBuilder.BuildExpression( groupAnd );
		}
		
		class PrefixQueryFieldValueFormatter : IQueryFieldValueFormatter {
			string Prefix;
			string SourceName;
			
			public PrefixQueryFieldValueFormatter(string sourceName, string prefix) {
				Prefix = prefix;
				SourceName = sourceName;
			}
			
			public string Format(IQueryFieldValue fieldValue) {
				string[] parts = fieldValue.Name.Split('.');
				if (parts.Length>1 && parts[0]==SourceName)
					return Prefix+"."+parts[1];
				return fieldValue.Name;
			}
		}
		
		
		
	}
}
