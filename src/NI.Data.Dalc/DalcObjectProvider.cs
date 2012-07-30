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
using System.Data;
using System.Collections;

using NI.Common;
using NI.Common.Providers;

namespace NI.Data.Dalc
{
	/// <summary>
	/// DALC IObjectProvider implementation.
	/// </summary>
	public class DalcObjectProvider : IObjectProvider
	{
		IQueryProvider _QueryProvider;
		IDalc _Dalc;
		
		/// <summary>
		/// Get or set relational expressions used to load data
		/// </summary>
		public IQueryProvider QueryProvider {
			get { return _QueryProvider; }
			set { _QueryProvider = value; }
		}
		
		/// <summary>
		/// Get or set DALC component to load data from
		/// </summary>
		public IDalc Dalc {
			get { return _Dalc; }
			set { _Dalc = value; }
		}				
		
		public DalcObjectProvider()
		{
		}
		
		public object GetObject(object context) {
			IQuery q = QueryProvider.GetQuery(context);
			
			Hashtable record = new Hashtable();
			if (Dalc.LoadRecord(record, q)) {
				if (q.Fields!=null && q.Fields.Length>0 && record.ContainsKey(q.Fields[0]) )
					return record[q.Fields[0]];
				else
					// return any available field...
					foreach (DictionaryEntry entry in record)
						return entry.Value;
			}
			
			return null;
		}
		
	}
}
