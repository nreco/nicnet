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

using NI.Common;
using NI.Common.Expressions;
using NI.Common.Providers;

namespace NI.Data
{
	/// <summary>
	/// Data view info.
	/// </summary>
	public class DbDataView : IDbDataView, IQueryFieldValueFormatter
	{
		string _SourceNameAlias;
		string _SourceNameOrigin;
		string _SqlCommandTextTemplate;
		string _SqlCountFields;
		string _SqlFields;
		IDictionary _FieldsMapping;
		IExpressionResolver _ExprResolver;

		public string SourceNameAlias {
			get { return _SourceNameAlias; }
			set { _SourceNameAlias = value; }
		}

		public string SourceNameOrigin {
			get { return _SourceNameOrigin; }
			set { _SourceNameOrigin = value; }
		}


		public string SqlCountFields {
			get { return _SqlCountFields; }
			set { _SqlCountFields = value; }
		}
		
		public string SqlFields {
			get { return _SqlFields; }
			set { _SqlFields = value; }
		}

		IStringProvider _SqlCommandTextProvider;
		public IStringProvider SqlCommandTextProvider
		{
			get { return _SqlCommandTextProvider; }
			set { _SqlCommandTextProvider = value; }
		}

		public string SqlCommandTextTemplate {
			get { return _SqlCommandTextTemplate; }
			set { _SqlCommandTextTemplate = value; }
		}

		public IDictionary FieldsMapping {
			get { return _FieldsMapping; }
			set { _FieldsMapping = value; }
		}
		
		public IExpressionResolver ExprResolver {
			get { return _ExprResolver; }
			set { _ExprResolver = value; }
		}


		public DbDataView()
		{
		}

		public virtual bool MatchSourceName(string sourceName) {
			return SourceNameAlias==sourceName;
		}
		
		public virtual string FormatSqlCommandText(IDictionary context) {
			// legacy
			context["fields"] = Convert.ToString( ExprResolver.Evaluate(context, 
				Convert.ToString(context["fields"])=="count(*)" ? SqlCountFields : SqlFields) );
			
			// format SQL text
			return Convert.ToString( ExprResolver.Evaluate(context,SqlCommandTextProvider!=null?SqlCommandTextProvider.GetString(context):SqlCommandTextTemplate) );
		}
		
		public virtual IQueryFieldValueFormatter GetQueryFieldValueFormatter(IQuery q) {
			return this;
		}
		
		protected virtual string ApplyFieldNameMapping(string fldName) {
			return FieldsMapping != null && FieldsMapping.Contains(fldName) ? (string)FieldsMapping[fldName] : fldName;
		}
		
		string IQueryFieldValueFormatter.Format(IQueryFieldValue fieldValue) {
			return ApplyFieldNameMapping(fieldValue.Name);
		}
		
	}
}
