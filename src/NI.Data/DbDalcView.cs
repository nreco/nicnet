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
			var callProps = new Dictionary<string, PropertyInfo>();
			foreach (var p in viewContext.GetType().GetProperties()) {
				callProps[p.Name] = p;
			}

			var sb = new StringBuilder();
			var sqlTpl = SqlCommandTextTemplate;
			int pos = 0;
			while (pos < sqlTpl.Length) {
				var c = sqlTpl[pos];
				if (c == '@') {
					int endPos;
					var name = ReadName(sqlTpl, pos + 1, out endPos);
					if (name != null && callProps.ContainsKey(name)) {
						string[] formatOptions;
						try {
							formatOptions = ReadFormatOptions(sqlTpl, endPos, out endPos);
						} catch (Exception ex) {
							throw new Exception(String.Format("Parse error (format options of property {0}) at {1}: {2}",
								name, pos, ex.Message), ex);
						}
						object callRes;
						try {
							callRes = callProps[name].GetValue(viewContext, null);
						} catch (Exception ex) {
							throw new Exception(String.Format("Evaluation of property {0} at position {1} failed: {2}",
								name, pos, ex.Message), ex);
						}
						if (callRes != ViewContext.NotApplicable) {
							var fmtNotEmpty = formatOptions != null && formatOptions.Length > 0 ? formatOptions[0] : "{0}";
							var fmtEmpty = formatOptions != null && formatOptions.Length > 1 ? formatOptions[1] : "";
							try {
								sb.Append( callRes != null && Convert.ToString(callRes) != String.Empty ?
										String.Format(fmtNotEmpty, callRes) : String.Format(fmtEmpty, callRes)
								);
							} catch (Exception ex) {
								throw new Exception(String.Format("Format of property {0} at position {1} failed: {2}",
									name, pos, ex.Message), ex);
							}
						}
						pos = endPos;
					}
				} else {
					sb.Append(c);
					pos++;
				}
			}

			return sb.ToString();
		}

		protected string[] ReadFormatOptions(string s, int start, out int newStart) {
			newStart = start;
			if (start >= s.Length || s[start] != '[')
				return null;
			start++;
			var opts = new List<string>();
			var pSb = new StringBuilder();
			while (start < s.Length) {
				if (s[start] == ']') {
					break;
				} else if (s[start] == ';') {
					opts.Add(pSb.ToString());
					pSb.Clear();
				} else {
					pSb.Append(s[start]);
				}
				start++;
			}
			opts.Add(pSb.ToString());
			if (s[start] != ']')
				throw new FormatException("Invalid format options");
			if (opts.Count > 2)
				throw new FormatException("Too many format options");
			newStart = start + 1;
			return opts.ToArray();
		}

		protected string ReadName(string s, int start, out int newStart) {
			newStart = start;
			// should start with letter
			if (start >= s.Length || !Char.IsLetter(s[start]))
				return null;
			// rest of the name: letters or digits
			while (start < s.Length && Char.IsLetterOrDigit(s[start]))
				start++;

			var name = s.Substring(newStart, start-newStart);
			newStart = start;
			return name;
		}

		public class ViewContext {
			internal readonly static string NotApplicable = "\0";

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
				var condition = ApplyFieldMapping(Query.Condition);
				return IsolateWhereExpression(
						SqlBuilder.BuildExpression(condition));
			}

			protected QueryNode ApplyFieldMapping(QueryNode qNode) {
				if (qNode is QueryGroupNode) {
					var group = new QueryGroupNode((QueryGroupNode)qNode);
					for (int i = 0; i < group.Nodes.Count; i++)
						group.Nodes[i] = ApplyFieldMapping(group.Nodes[i]);
					return group;
				}
				if (qNode is QueryConditionNode) {
					var cndNode = new QueryConditionNode((QueryConditionNode)qNode);
					cndNode.LValue = ApplyFieldMapping(cndNode.LValue);
					cndNode.RValue = ApplyFieldMapping(cndNode.RValue);
					return cndNode;
				}
				if (qNode is QueryNegationNode) {
					var negNode = new QueryNegationNode((QueryNegationNode)qNode);
					for (int i = 0; i < negNode.Nodes.Count; i++)
						negNode.Nodes[i] = ApplyFieldMapping(negNode.Nodes[i]);
					return negNode;
				}
				return qNode;
			}
			protected virtual IQueryValue ApplyFieldMapping(IQueryValue qValue) {
				if (qValue is QField) {
					var qFld = (QField)qValue;
					if (View.FieldMapping.ContainsKey(qFld.Name)) {
						return new QRawSql(View.FieldMapping[qFld.Name]);
					}
				}
				return qValue;
			}

			protected string BuildSort() {
				if (IsCountQuery) // order by is not applicable for count queries
					return NotApplicable;
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

		/*public virtual string FormatSqlCommandText(IDictionary context) {

			// legacy
			context["fields"] = ExprResolver(context, 
				Convert.ToString(context["fields"])=="count(*)" ? SqlCountFields : SqlFields );
			
			// format SQL text
			return Convert.ToString( ExprResolver(context,SqlCommandTextProvider!=null?SqlCommandTextProvider(context):SqlCommandTextTemplate) );
		}
		
		public virtual Func<QField,string> GetQueryFieldValueFormatter(Query q) {
			return FormatDataViewField;
		}
		
		protected virtual string ApplyFieldNameMapping(string fldName) {
			return FieldsMapping != null && FieldsMapping.Contains(fldName) ? (string)FieldsMapping[fldName] : fldName;
		}
		
		string FormatDataViewField(QField fieldValue) {
			return ApplyFieldNameMapping(fieldValue.Name);
		}*/
		
	}
}
