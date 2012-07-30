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

using NI.Common;
using NI.Common.Transaction;

namespace NI.Data.Dalc {

	public class DbDalcTransaction : NI.Common.Transaction.FlatTransaction {
		
		IDbDalc _Dalc;
		IDbTransaction _DbTransaction;
		IsolationLevel _IsolationLevel = IsolationLevel.Unspecified;

		public IDbDalc Dalc {
			get { return _Dalc; }
			set { _Dalc = value; }
		}

		public IsolationLevel IsolationLevel {
			get { return _IsolationLevel; }
			set { _IsolationLevel = value; }
		}
		
		protected IDbTransaction DbTransaction {
			get { return _DbTransaction; }
			set { _DbTransaction = value; }
		}
		
		
		/// <summary>
		/// Begin operation internal logic
		/// </summary>
		protected override void BeginInternal() {
			if (IsInTransaction) return;
			
			Dalc.Connection.Open();
			DbTransaction = IsolationLevel==IsolationLevel.Unspecified ?
								Dalc.Connection.BeginTransaction() :
								Dalc.Connection.BeginTransaction(IsolationLevel);
			Dalc.Transaction = DbTransaction;
			
			base.BeginInternal();
		}
		

		/// <summary>
		/// Commit operation internal logic
		/// </summary>
		protected override void CommitInternal() {
			if (IsInTransaction) {
				DbTransaction.Commit();
				DbTransaction = null;
				Dalc.Transaction = null;
				Dalc.Connection.Close();
				
				base.CommitInternal();
			}
		}
		
		/// <summary>
		/// Abort operation internal logic
		/// </summary>
		protected override void AbortInternal() {
			if (IsInTransaction) {
				DbTransaction.Rollback();
				DbTransaction = null;
				Dalc.Transaction = null;
				Dalc.Connection.Close();

				base.AbortInternal();
			}
		}
		
		
	
	}



}