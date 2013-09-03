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


namespace NI.Data
{
	/// <summary>
	/// Data view info.
	/// </summary>
	public class DbDataView : IDbDataView
	{
		string _SourceNameAlias;
		string _SourceNameOrigin;
		string _SqlCommandTextTemplate;
		string _SqlCountFields;
		string _SqlFields;
		IDictionary _FieldsMapping;

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

		public Func<IDictionary, string> SqlCommandTextProvider { get; set; }

		public string SqlCommandTextTemplate {
			get { return _SqlCommandTextTemplate; }
			set { _SqlCommandTextTemplate = value; }
		}

		public IDictionary FieldsMapping {
			get { return _FieldsMapping; }
			set { _FieldsMapping = value; }
		}

		public Func<IDictionary, string, string> ExprResolver { get; set; }

		public DbDataView()
		{
		}

		public virtual bool MatchSourceName(string sourceName) {
			return SourceNameAlias==sourceName;
		}
		
		public virtual string FormatSqlCommandText(IDictionary context) {
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
		}
		
	}
}
