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
using System.Diagnostics;

namespace NI.Data.Dalc
{
	/// <summary>
	/// 'Failsafe' IDbDalc wrapper.
	/// </summary>
	public class FailsafeDalc : IDalc
	{
		IDalc _MainDalc;
		IDalc _BackupDalc;
		
		public IDalc MainDalc {
			get { return _MainDalc; }
			set { _MainDalc = value; }
		}
		
		public IDalc BackupDalc {
			get { return _BackupDalc; }
			set { _BackupDalc = value; }
		}
	
		public FailsafeDalc() {
		}

		public void Load(DataSet ds, IQuery query) {
			try {
				MainDalc.Load(ds, query);
			} catch (Exception ex) {
				Trace.WriteLine(ex.Message, ex.StackTrace);
				BackupDalc.Load(ds, query);
			}
		}
		
		public void Update(DataSet ds, string sourceName) {
			try {
				MainDalc.Update(ds, sourceName);
			} catch (Exception ex) {
				Trace.WriteLine(ex.Message, ex.StackTrace);
				BackupDalc.Update(ds, sourceName);
			}
		}

		public int Update(IDictionary data, IQuery query) {
			try {
				return MainDalc.Update(data, query);
			} catch (Exception ex) {
				Trace.WriteLine(ex.Message, ex.StackTrace);
				return BackupDalc.Update(data, query);
			}		
		}

		public void Insert(IDictionary data, string sourceName) {
			try {
				MainDalc.Insert(data, sourceName);
			} catch (Exception ex) {
				Trace.WriteLine(ex.Message, ex.StackTrace);
				BackupDalc.Insert(data, sourceName);
			}
		}
		
		public int Delete(IQuery query) {
			try {
				return MainDalc.Delete(query);
			} catch (Exception ex) {
				Trace.WriteLine(ex.Message, ex.StackTrace);
				return BackupDalc.Delete(query);
			}
		}
		
		public bool LoadRecord(IDictionary data, IQuery query) {
			try {
				return MainDalc.LoadRecord(data, query);
			} catch (Exception ex) {
				Trace.WriteLine(ex.Message, ex.StackTrace);
				return BackupDalc.LoadRecord(data, query);
			}
		}
		
		public int RecordsCount(string sourceName, IQueryNode conditions) {
			try {
				return MainDalc.RecordsCount(sourceName, conditions);
			} catch (Exception ex) {
				Trace.WriteLine(ex.Message, ex.StackTrace);
				return BackupDalc.RecordsCount(sourceName, conditions);
			}
		}



	}
}
