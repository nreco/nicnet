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
using System.Linq;
using System.Runtime.Serialization;

namespace NI.Data
{

	/// <summary>
	/// Abstract data query structure
	/// </summary>
	[Serializable]
	public class Query : QueryNode, IQueryValue
	{
		private QSort[] _Sort = null;
		private QField[] _Fields = null;
		private int _StartRecord = 0;
		private int _RecordCount = Int32.MaxValue;
		private QSource _SourceName = null;
		private IDictionary _ExtendedProperties = null;
		
		/// <summary>
		/// Query condition. Can be null
		/// </summary>
		public QueryNode Condition { get; set; }

		public override IList<QueryNode> Nodes {
			get { return new QueryNode[] { Condition }; }
		}

		/// <summary>
		/// Sort fields list. Can be null.
		/// </summary>
		public QSort[] Sort {
			get { return _Sort; } 
			set { _Sort = value; }
		}
		
		/// <summary>
		/// Fields to load. Null means all available fields.
		/// </summary>
		public QField[] Fields {
			get { return _Fields; }
			set { _Fields = value; }
		}
		
		/// <summary>
		/// Start record
		/// </summary>
		public int StartRecord {
			get { return _StartRecord; }
			set { _StartRecord = value; }
		}
		
		/// <summary>
		/// Max records count
		/// </summary>
		public int RecordCount {
			get { return _RecordCount; }
			set { _RecordCount = value; }
		}
		
		public QSource SourceName { 
			get { return _SourceName; }
			set { _SourceName = value; }
		}
		
		public IDictionary ExtendedProperties {
			get {
				if (_ExtendedProperties==null)
					_ExtendedProperties = new Hashtable();
				return _ExtendedProperties;
			}
			set { _ExtendedProperties = value; }
		}

		public Query(string sourceName) {
			_SourceName = new QSource(sourceName);
		}

		public Query(string sourceName, QueryNode condition) {
			_SourceName = sourceName;
			Condition = condition;
		}

		public Query(string sourceName, QueryNode condition, string[] sort) {
			_SourceName = sourceName;
			Condition = condition;
			_Sort = sort.Select(s => new QSort(s) ).ToArray();
		}
		
		public Query(string sourceName, QueryNode condition, string[] sort, int startRecord, int recordCount) {
			_SourceName = sourceName;
			SetSort(sort);
			_StartRecord = startRecord;
			_RecordCount = recordCount;
			Condition = condition;
		}
		
		public Query(string sourceName, int startRecord, int recordCount) {
			_SourceName = sourceName;
			_StartRecord = startRecord;
			_RecordCount = recordCount;
		}

		public Query(string sourceName, string[] sort, int startRecord, int recordCount) {
			_SourceName = sourceName;
			SetSort(sort);
			_StartRecord = startRecord;
			_RecordCount = recordCount;
		}

		public Query(Query q) {
			_SourceName = q.SourceName;
			_Sort = q.Sort;
			_StartRecord = q.StartRecord;
			_RecordCount = q.RecordCount;
			Condition = q.Condition;
			_Fields = q.Fields;
			_ExtendedProperties = new Hashtable( q.ExtendedProperties );
		}

		public void SetSort(params string[] sortFields) {
			if (sortFields != null && sortFields.Length > 0) {
				_Sort = sortFields.Select(v => (QSort)v).ToArray();
			} else {
				_Sort = null;
			}
		}
		public void SetSort(params QSort[] sortFields) {
			if (sortFields != null && sortFields.Length > 0) {
				_Sort = sortFields;
			} else {
				_Sort = null;
			}
		}

		public void SetFields(params string[] fields) {
			if (fields != null && fields.Length > 0) {
				_Fields = fields.Select(v => (QField)v).ToArray();
			} else {
				_Fields = null;
			}
		}
		public void SetFields(params QField[] fields) {
			if (fields != null && fields.Length > 0) {
				_Fields = fields;
			} else {
				_Fields = null;
			}
		}

		
		public override string ToString() {
			return (new QueryStringBuilder()).BuildQueryString(this);
		}
		
		class QueryStringBuilder : SqlBuilder {

			public string BuildQueryString(Query q) {
				string rootExpression = BuildExpression( q.Condition );
				if (rootExpression!=null && rootExpression.Length>0)
					rootExpression = String.Format("({0})", rootExpression);
			
				string sortExpression = q.Sort!=null ? "; "+String.Join(",", q.Sort.Select(v=>v.ToString()).ToArray() ) : null;
				string fieldExpression = q.Fields!=null ? String.Join(",", q.Fields.Select(v=>v.ToString()).ToArray() ) : "*";
			
				return String.Format("{0}{1}[{2}{3}]{{{4},{5}}}", q.SourceName, rootExpression,
					fieldExpression, sortExpression, q.StartRecord, q.RecordCount);
			}
			
			public override string BuildValue(IQueryValue value) {
				if (value is Query) 
					return BuildQueryString( (Query) value );
				return base.BuildValue (value);
			}

		}

		
	
	}
}
