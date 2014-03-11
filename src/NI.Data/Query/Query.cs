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
	/// Represents DALC data query
	/// </summary>
	[Serializable]
	public class Query : QueryNode, IQueryValue
	{
		private QSort[] _Sort = null;
		private QField[] _Fields = null;
		private int _StartRecord = 0;
		private int _RecordCount = Int32.MaxValue;
		private QTable _Table = null;
		private IDictionary _ExtendedProperties = null;
		
		/// <summary>
		/// Query condition represented by QueryNode. Can be null.
		/// </summary>
		public QueryNode Condition { get; set; }

		/// <summary>
		/// List of child nodes
		/// </summary>
		public override IList<QueryNode> Nodes {
			get { return new QueryNode[] { Condition }; }
		}

		/// <summary>
		/// List of sort fields. Can be null.
		/// </summary>
		public QSort[] Sort {
			get { return _Sort; } 
			set { _Sort = value; }
		}
		
		/// <summary>
		/// List of fields to load. Null means all available fields.
		/// </summary>
		public QField[] Fields {
			get { return _Fields; }
			set { _Fields = value; }
		}
		
		/// <summary>
		/// Get or set starting record to load
		/// </summary>
		public int StartRecord {
			get { return _StartRecord; }
			set { _StartRecord = value; }
		}
		
		/// <summary>
		/// Get or set max records count to load
		/// </summary>
		public int RecordCount {
			get { return _RecordCount; }
			set { _RecordCount = value; }
		}
		
		/// <summary>
		/// Get or set target source name of this query
		/// </summary>
		public QTable Table { 
			get { return _Table; }
			set { _Table = value; }
		}
		
		/// <summary>
		/// Get or set query extended properties. 
		/// </summary>
		/// <remarks>Extended properties may be used by concrete implementations of DALC</remarks>
		public IDictionary ExtendedProperties {
			get {
				if (_ExtendedProperties==null)
					_ExtendedProperties = new Hashtable();
				return _ExtendedProperties;
			}
			set { _ExtendedProperties = value; }
		}

		/// <summary>
		/// Initializes a new instance of the Query with specified table name
		/// </summary>
		/// <param name="tableName">target table name</param>
		public Query(string tableName) {
			_Table = new QTable(tableName);
		}

		/// <summary>
		/// Initializes a new instance of the Query with specified table
		/// </summary>
		/// <param name="table">target table</param>
		public Query(QTable table) {
			_Table = table;
		}

		/// <summary>
		/// Initializes a new instance of the Query with specified table name and condition node
		/// </summary>
		/// <param name="tableName">target table name</param>
		/// <param name="condition">condition represented by QueryNode</param>
		public Query(string tableName, QueryNode condition) {
			_Table = tableName;
			Condition = condition;
		}

		/// <summary>
		/// Initializes a new instance of the Query with specified table and condition
		/// </summary>
		/// <param name="table">target table</param>
		/// <param name="condition">condition represented by QueryNode</param>
		public Query(QTable table, QueryNode condition) {
			_Table = table;
			Condition = condition;
		}

		/// <summary>
		/// Initializes a new instance of the Query with identical options of specified query
		/// </summary>
		/// <param name="q">query with options to copy</param>
		public Query(Query q) {
			_Table = q.Table;
			_Sort = q.Sort;
			_StartRecord = q.StartRecord;
			_RecordCount = q.RecordCount;
			Condition = q.Condition;
			_Fields = q.Fields;
			_ExtendedProperties = new Hashtable( q.ExtendedProperties );
		}
		
		/// <summary>
		/// Set query sort by specified fields
		/// </summary>
		/// <param name="sortFields">list of sort fields</param>
		public void SetSort(params string[] sortFields) {
			if (sortFields != null && sortFields.Length > 0) {
				_Sort = sortFields.Select(v => (QSort)v).ToArray();
			} else {
				_Sort = null;
			}
		}

		/// <summary>
		/// Set query sort by specified list of QSort
		/// </summary>
		/// <param name="sortFields"></param>
		public void SetSort(params QSort[] sortFields) {
			if (sortFields != null && sortFields.Length > 0) {
				_Sort = sortFields;
			} else {
				_Sort = null;
			}
		}

		/// <summary>
		/// Set query fields by specified list of field names
		/// </summary>
		/// <param name="fields">list of field names</param>
		public void SetFields(params string[] fields) {
			if (fields != null && fields.Length > 0) {
				_Fields = fields.Select(v => (QField)v).ToArray();
			} else {
				_Fields = null;
			}
		}

		/// <summary>
		/// Set query fields by specified list of QField
		/// </summary>
		/// <param name="fields"></param>
		public void SetFields(params QField[] fields) {
			if (fields != null && fields.Length > 0) {
				_Fields = fields;
			} else {
				_Fields = null;
			}
		}

		/// <summary>
		/// Returns a string that represents current query in relex format
		/// </summary>
		/// <returns>relex string</returns>
		public override string ToString() {
			return (new NI.Data.RelationalExpressions.RelexBuilder()).BuildRelex(this);
		}
	
	}
}
