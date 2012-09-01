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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NI.Data
{

	/// <summary>
	/// Generic query implementation
	/// </summary>
	[Serializable]
	public class Query : QueryNode, IQueryValue
	{
		private string[] _Sort = null;
		private string[] _Fields = null;
		private int _StartRecord = 0;
		private int _RecordCount = Int32.MaxValue;
		private string _SourceName = null;
		private IDictionary _ExtendedProperties = null;
		
		/// <summary>
		/// Filter expression root group. Can be null
		/// </summary>
		public QueryNode Condition { get; set; }

		public override IList<QueryNode> Nodes {
			get { return new QueryNode[] { Condition }; }
		}

		/// <summary>
		/// Sort expression. Can be null.
		/// </summary>
		public string[] Sort {
			get { return _Sort; } 
			set { _Sort = value; }
		}
		
		/// <summary>
		/// Fields to load through filter. Null for all
		/// </summary>
		public string[] Fields {
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
		
		public int RecordCount {
			get { return _RecordCount; }
			set { _RecordCount = value; }
		}
		
		public string SourceName { 
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
		
		/*private string _Prefix = String.Empty;
		public string Prefix {
			get { return _Prefix; }
			set {
				_Prefix = value;
			}
		}*/

		public Query(string sourceName) {
			_SourceName = sourceName;
		}

		public Query(string sourceName, QueryNode root) {
			_SourceName = sourceName;
			Condition = root;
		}

		public Query(string sourceName, QueryNode root, string[] sort) {
			_SourceName = sourceName;
			Condition = root;
			_Sort = sort;
		}
		
		public Query(string sourceName, QueryNode root, string[] sort, int start_record, int record_count) {
			_SourceName = sourceName;
			_Sort = sort;
			_StartRecord = start_record;
			_RecordCount = record_count;
			Condition = root;
		}
		
		public Query(string sourceName, int start_record, int record_count) {
			_SourceName = sourceName;
			_StartRecord = start_record;
			_RecordCount = record_count;
		}

		public Query(string sourceName, string[] sort, int start_record, int record_count) {
			_SourceName = sourceName;
			_Sort = sort;
			_StartRecord = start_record;
			_RecordCount = record_count;
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

		
		public override string ToString() {
			return (new QueryStringBuilder()).BuildQueryString(this);
		}
		
		class QueryStringBuilder : SqlBuilder {

			public string BuildQueryString(Query q) {
				string rootExpression = BuildExpression( q.Condition );
				if (rootExpression!=null && rootExpression.Length>0)
					rootExpression = String.Format("({0})", rootExpression);
			
				string sortExpression = q.Sort!=null ? "; "+String.Join(",", q.Sort) : null;
				string fieldExpression = q.Fields!=null ? String.Join(",", q.Fields) : "*";
			
				return String.Format("{0}{1}[{2}{3}]{{{4},{5}}}", q.SourceName, rootExpression,
					fieldExpression, sortExpression, q.StartRecord, q.RecordCount);
			}
			
			protected override string BuildValue(IQueryValue value) {
				if (value is Query) 
					return BuildQueryString( (Query) value );
				return base.BuildValue (value);
			}

		}

		
	
	}
}
