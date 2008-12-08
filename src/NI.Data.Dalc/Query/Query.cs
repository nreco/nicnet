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
using System.Runtime.Serialization;

namespace NI.Data.Dalc
{

	/// <summary>
	/// Generic query implementation
	/// </summary>
	[Serializable]
	public class Query : IQuery
	{
		private IQueryNode _Root = null;
		private string[] _Sort = null;
		private string[] _Fields = null;
		private int _StartRecord = 0;
		private int _RecordCount = Int32.MaxValue;
		private string _SourceName = null;
		
		/// <summary>
		/// Filter expression root group. Can be null
		/// </summary>
		public IQueryNode Root {
			get { return _Root; }
			set { _Root = value; }
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

		public Query(string sourceName, IQueryNode root) {
			_SourceName = sourceName;
			Root = root;
		}

		public Query(string sourceName, IQueryNode root, string[] sort) {
			_SourceName = sourceName;
			Root = root;
			_Sort = sort;
		}
		
		public Query(string sourceName, IQueryNode root, string[] sort, int start_record, int record_count) {
			_SourceName = sourceName;
			_Sort = sort;
			_StartRecord = start_record;
			_RecordCount = record_count;
			Root = root;
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

		public Query(IQuery q) {
			_SourceName = q.SourceName;
			_Sort = q.Sort;
			_StartRecord = q.StartRecord;
			_RecordCount = q.RecordCount;
			_Root = q.Root;
			_Fields = q.Fields;
		}

		
		public override string ToString() {
			return (new QueryStringBuilder()).BuildQueryString(this);
		}
		
		class QueryStringBuilder : SqlBuilder {

			public string BuildQueryString(IQuery q) {
				string rootExpression = BuildExpression( q.Root );
				if (rootExpression!=null && rootExpression.Length>0)
					rootExpression = String.Format("({0})", rootExpression);
			
				string sortExpression = q.Sort!=null ? "; "+String.Join(",", q.Sort) : null;
				string fieldExpression = q.Fields!=null ? String.Join(",", q.Fields) : "*";
			
				return String.Format("{0}{1}[{2}{3}]{{{4},{5}}}", q.SourceName, rootExpression,
					fieldExpression, sortExpression, q.StartRecord, q.RecordCount);
			}
			
			protected override string BuildValue(IQueryValue value) {
				if (value is IQuery) 
					return BuildQueryString( (IQuery) value );
				return base.BuildValue (value);
			}

		}

		
	
	}
}
