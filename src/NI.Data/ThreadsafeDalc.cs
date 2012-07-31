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
using System.Data;
using System.Collections;
using System.Diagnostics;

namespace NI.Data
{
	/// <summary>
	/// 'Threadsafe' IDbDalc wrapper.
	/// </summary>
	public class ThreadsafeDalc : IDalc
	{
		IDalc _Dalc;
		
		public IDalc Dalc {
			get { return _Dalc; }
			set { _Dalc = value; }
		}
	
		public ThreadsafeDalc() {
		}

		public void Load(DataSet ds, IQuery query) {
			lock (Dalc) {
				Dalc.Load(ds, query);
			}
		}
		
		public void Update(DataSet ds, string sourceName) {
			lock (Dalc) {
				Dalc.Update(ds, sourceName);
			} 
		}

		public int Update(IDictionary data, IQuery query) {
			lock (Dalc) {
				return Dalc.Update(data, query);
			}		
		}

		public void Insert(IDictionary data, string sourceName) {
			lock (Dalc) {
				Dalc.Insert(data, sourceName);
			}		
		}
		
		public int Delete(IQuery query) {
			lock (Dalc) {
				return Dalc.Delete(query);
			}
		}
		
		public bool LoadRecord(IDictionary data, IQuery query) {
			lock (Dalc) {
				return Dalc.LoadRecord(data, query);
			}
		}
		
		public int RecordsCount(string sourceName, IQueryNode conditions) {
			lock (Dalc) {
				return Dalc.RecordsCount(sourceName, conditions);
			}
		}



	}
}
