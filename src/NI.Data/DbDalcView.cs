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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace NI.Data
{
	/// <summary>
	/// Data view info.
	/// </summary>
	public class DbDalcView : IDbDalcView
	{
		public string SourceName { get; set; }

		public QSource[] OriginSourceNames { get; set; }

		public IDictionary<string,string> FieldMapping { get; set; }

		protected string CountFields { get; set; }
		protected string SelectFields { get; set; }

		protected string SqlCommandTextTemplate { get; set; }
		protected Func<ViewContext, string> GetSqlCommandText { get; set; }

		public DbDalcView(string sourceName, Func<ViewContext, string> getSqlCommandText) {
			SourceName = sourceName;
			GetSqlCommandText = getSqlCommandText;
		}

		public DbDalcView(string sourceName, string sqlCommandTextTemplate) {
			SourceName = sourceName;
			SqlCommandTextTemplate = sqlCommandTextTemplate;
		}

		public DbDalcView(string sourceName, Func<ViewContext, string> getSqlCommandText, string sqlFields, string sqlCountFields) {
			SourceName = sourceName;
			GetSqlCommandText = getSqlCommandText;
			SelectFields = sqlFields;
			CountFields = sqlCountFields;
		}

		public DbDalcView(string sourceName, string sqlCommandTextTemplate, string sqlFields, string sqlCountFields) {
			SourceName = sourceName;
			SqlCommandTextTemplate = sqlCommandTextTemplate;
			SelectFields = sqlFields;
			CountFields = sqlCountFields;
		}



		public virtual bool MatchSourceName(QSource source) {
			return SourceName==source.Name;
		}

		public string ComposeSelect(Query q, IDbSqlBuilder sqlBuilder) {
			var viewContext = new ViewContext(this, q, sqlBuilder);
			if (GetSqlCommandText != null) {
				return GetSqlCommandText(viewContext);
			}
			if (SqlCommandTextTemplate!=null) {
				return FormatSelectSql(viewContext);
			}

			throw new Exception("Invalid DbDalcView configuration (missed SqlCommandTextTemplate)");
		}

		/// <summary>
		/// Performs default SQL template parsing that can handle simple code snippets
		/// </summary>
		protected string FormatSelectSql(ViewContext viewContext) {
			var strTpl = new SimpleStringTemplate(SqlCommandTextTemplate);
			return strTpl.FormatTemplate(viewContext);
		}

		public class ViewContext {
			string _SqlOrderBy = null;
			string _SqlWhere = null;
			string _SqlFields = null;
			string _SqlCountFields = null;

			public Query Query { get; private set; }

			public IDbSqlBuilder SqlBuilder { get; private set; }

			protected DbDalcView View { get; private set; }

			public ViewContext(DbDalcView view, Query q, IDbSqlBuilder sqlBuilder) {
				Query = q;
				View = view;
				SqlBuilder = sqlBuilder;
			}

			public string SqlOrderBy {
				get {
					return (_SqlOrderBy ?? (_SqlOrderBy = BuildSort()));
				}
			}

			public string SqlWhere {
				get {
					return (_SqlWhere ?? (_SqlWhere = BuildWhere() ));
				}
			}

			public string SqlFields {
				get {
					return (_SqlFields ?? (_SqlFields = BuildFields()));
				}
			}

			public int StartRecord {
				get { return Query.StartRecord; }
			}

			public int RecordCount {
				get { return Query.RecordCount; }
			}

			public int RecordLimit {
				get { return (StartRecord+RecordCount); }
			}

			protected string IsolateWhereExpression(string expression) {
				return expression != null && expression.Length > 0 ? "(" + expression + ")" : expression;
			}

			public bool IsCountQuery {
				get {
					return Query.Fields != null &&
						Query.Fields.Length == 1 &&
						Query.Fields[0].Expression!=null &&
						Query.Fields[0].Expression.ToLower() == "count(*)";
				}
			}

			protected string BuildFields() {
				if (!String.IsNullOrEmpty(View.CountFields))
					if (IsCountQuery) {
						return View.CountFields;
					}
				if (!String.IsNullOrEmpty(View.SelectFields)) {
					return View.SelectFields;
				}
				return SqlBuilder.BuildFields(Query);
			}

			protected string BuildWhere() {
				var condition = DataHelper.MapQValue(Query.Condition, ApplyFieldMapping);
				return IsolateWhereExpression(
						SqlBuilder.BuildExpression(condition));
			}

			protected virtual IQueryValue ApplyFieldMapping(IQueryValue qValue) {
				if (qValue is QField) {
					var qFld = (QField)qValue;
					if (View.FieldMapping.ContainsKey(qFld.Name)) {
						return new QField(qFld.Name, View.FieldMapping[qFld.Name]);
					}
				}
				return qValue;
			}

			protected string BuildSort() {
				if (IsCountQuery) // order by is not applicable for count queries
					return SimpleStringTemplate.NotApplicable;
				if (Query.Sort==null || Query.Sort.Length==0)
					return null;

				var sortParts = new List<string>();
				foreach (var sf in Query.Sort) {
					if (View.FieldMapping.ContainsKey(sf.Field.Name)) {
						var fldMapping = View.FieldMapping[sf.Field.Name];
						sortParts.Add(sf.SortDirection == ListSortDirection.Ascending ?
							fldMapping : String.Format("{0} {1}", fldMapping, QSort.Desc));
					} else {
						sortParts.Add(SqlBuilder.BuildSort(sf));
					}
				}
				return String.Join(",", sortParts.ToArray());
			}

		}
		
	}
}
