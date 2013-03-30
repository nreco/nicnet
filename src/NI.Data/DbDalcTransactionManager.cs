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

namespace NI.Data {

	/// <summary>
	/// Incapsulates classic ADO.NET explicit transactions mechanism with Begin/Commit/Abort methods
	/// </summary>
	public class DbDalcTransactionManager {
		
		ISqlDalc _Dalc;
		IsolationLevel _IsolationLevel = IsolationLevel.Unspecified;

		public ISqlDalc Dalc {
			get { return _Dalc; }
			set { _Dalc = value; }
		}

		public IsolationLevel IsolationLevel {
			get { return _IsolationLevel; }
			set { _IsolationLevel = value; }
		}
		
		
		/// <summary>
		/// Begin operation internal logic
		/// </summary>
		public virtual void Begin() {
			if (Dalc.Transaction != null)
				return;
			
			Dalc.Connection.Open();
			Dalc.Transaction = IsolationLevel==IsolationLevel.Unspecified ?
								Dalc.Connection.BeginTransaction() :
								Dalc.Connection.BeginTransaction(IsolationLevel);
		}

		/// <summary>
		/// Commit operation internal logic
		/// </summary>
		public virtual void Commit() {
			if (Dalc.Transaction!=null) {
				Dalc.Transaction.Commit();
				Dalc.Transaction = null;
				Dalc.Connection.Close();
			}
		}
		
		/// <summary>
		/// Abort operation internal logic
		/// </summary>
		public virtual void Abort() {
			if (Dalc.Transaction!=null) {
				Dalc.Transaction.Rollback();
				Dalc.Transaction = null;
				Dalc.Connection.Close();
			}
		}
		
		
	
	}



}