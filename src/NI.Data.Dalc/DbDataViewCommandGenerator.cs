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

using NI.Common;

namespace NI.Data.Dalc
{
	/// <summary>
	/// Database Command Generator with data view support.
	/// </summary>
	public class DbDataViewCommandGenerator : DbCommandGenerator
	{
		IDbDataView[] _DataViews = new IDbDataView[0];
	
		[Dependency]
		public IDbDataView[] DataViews {
			get { return _DataViews; }
			set { _DataViews = value; }
		}
	
	
		public DbDataViewCommandGenerator()
		{
		}
		
		public override IDbCommandWrapper ComposeSelect(IQuery query) {
			string sourceName = query.SourceName.IndexOf('.')>=0 ? query.SourceName.Split('.')[0] : query.SourceName;
			for (int i=0; i<DataViews.Length; i++)
				if (DataViews[i].MatchSourceName(sourceName))
					return ComposeDataViewSelect(DataViews[i], query);
			
			return base.ComposeSelect(query);
		}
		
		/// <summary>
		/// </summary>
		protected virtual IDbCommandWrapper ComposeDataViewSelect(IDbDataView dataView, IQuery query) {
			IDbCommandWrapper cmdWrapper = CommandWrapperFactory.CreateInstance();
			
			IDictionary context = BuildSqlCommandContext(cmdWrapper, dataView, query);
			cmdWrapper.Command.CommandText = dataView.FormatSqlCommandText(context);
			
			//System.Diagnostics.Trace.WriteLine( cmdWrapper.Command.CommandText );
			
			return cmdWrapper;
		}
		
		protected IQueryFieldValueFormatter InsertFormatter(IQueryFieldValueFormatter original, IQueryFieldValueFormatter additional) {
			if (original==null) return additional;
			
			IQueryFieldValueFormatter[] origFormatters = original is ChainQueryFieldValueFormatter ?
				((ChainQueryFieldValueFormatter)original).Formatters :
				new IQueryFieldValueFormatter[] { original };
						
			IQueryFieldValueFormatter[] newFormatters = new IQueryFieldValueFormatter[origFormatters.Length+1];
			Array.Copy(origFormatters, 0, newFormatters, 1, origFormatters.Length);
			newFormatters[0] = additional;
			return new ChainQueryFieldValueFormatter(newFormatters);
				
		}
		
		protected virtual IDictionary BuildSqlCommandContext(IDbCommandWrapper cmdWrapper, IDbDataView dataView, IQuery query) {
			IDbSqlBuilder dbSqlBuilder = cmdWrapper.CreateSqlBuilder();
			
			// add dataview field formatter in the formatting chain
			dbSqlBuilder.QueryFieldValueFormatter = InsertFormatter(
				dbSqlBuilder.QueryFieldValueFormatter,
				dataView.GetQueryFieldValueFormatter(query) );
					
			string sort = dbSqlBuilder.BuildSort(query);
			string whereExpression = BuildWhereExpression( dbSqlBuilder, dataView, query);
			string fields = dbSqlBuilder.BuildFields(query);

			Hashtable context = new Hashtable();
			
			BuildNamedQueryNodeContext(context, query.Root, dbSqlBuilder);
			
			context["whereExpression"] = IsolateWhereExpression( whereExpression );
			context["sortExpression"] = sort;
			context["fields"] = fields;
			context["query"] = query;
			context["sourcename"] = query.SourceName;
			context["startRecord"] = query.StartRecord;
			context["recordCount"] = query.RecordCount;
			context["recordLimit"] = query.StartRecord+query.RecordCount;
			return context;
		}
		
		/// <summary>
		/// Isolates 'where expression' from context where it will be inserted
		/// </summary>
		protected string IsolateWhereExpression(string expression) {
			return expression!=null && expression.Length>0 ? "("+expression+")" : expression;
		}
		
		protected void BuildNamedQueryNodeContext(IDictionary context, IQueryNode node, IDbSqlBuilder dbSqlBuilder) {
			if (node==null) return;
			if (node is INamedQueryNode) {
				string name = ((INamedQueryNode)node).Name;
				if (name!=null)
					context[name] = dbSqlBuilder.BuildExpression( node );
			}
			if (node.Nodes!=null)
				foreach (IQueryNode childNode in node.Nodes)
					BuildNamedQueryNodeContext(context, childNode, dbSqlBuilder);
		}
		
		protected virtual string BuildWhereExpression(IDbSqlBuilder dbSqlBuilder, IDbDataView dataView, IQuery query) {
			return dbSqlBuilder.BuildExpression(query.Root);
		}
		
		
		

		
	}
}
