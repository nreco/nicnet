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

namespace NI.Common.Transaction
{
	/// <summary>
	/// Base class for flat transactions.
	/// </summary>
	public abstract class FlatTransaction : ITransaction
	{
		protected bool IsInTransaction = false;
		protected int EmbeddedLevel = 0;

		ITransactionEventsMediator _TransactionEventsMediator;		

		/// <summary>
		/// Get or set transaction events mediator
		/// </summary>
		public ITransactionEventsMediator TransactionEventsMediator {
			get { return _TransactionEventsMediator; }
			set { _TransactionEventsMediator = value; }
		}	
	
		public FlatTransaction()
		{
		}
		
		public void Begin() {
			// nested transaction is not supported
			if (IsInTransaction) {
				EmbeddedLevel++;
				return;
			}
			DoBegin();
		}
		
		public void Commit() {
			if (EmbeddedLevel>0) {
				EmbeddedLevel--;
				return;
			}
			DoCommit();
		}

		public void Abort() {
			if (EmbeddedLevel>0) {
				EmbeddedLevel--;
				return;
			}
			DoAbort();
		}

		/// <summary>
		/// Begin operation entry point (raising events)
		/// </summary>
		protected virtual void DoBegin() {
			OnBeforeBegin();
			BeginInternal();
			OnAfterBegin();
		}
		
		/// <summary>
		/// Commit operation entry point (raising events)
		/// </summary>
		protected virtual void DoCommit() {
			OnBeforeCommit();
			CommitInternal();
			OnAfterCommit();
		}

		/// <summary>
		/// Abort operation entry point (raising events)
		/// </summary>
		protected virtual void DoAbort() {
			OnBeforeAbort();
			AbortInternal();
			OnAfterAbort();
		}

		protected virtual EventArgs ComposeEventArgs() {
			return EventArgs.Empty;
		}

		
		/// <summary>
		/// Begin operation internal logic
		/// </summary>
		protected virtual void BeginInternal() {
			IsInTransaction = true;
		}

		/// <summary>
		/// Commit operation internal logic
		/// </summary>
		protected virtual void CommitInternal() {
			IsInTransaction = false;
		}
		
		/// <summary>
		/// Abort operation internal logic
		/// </summary>
		protected virtual void AbortInternal() {
			IsInTransaction = false;
		}		


		
		#region Internal events 
		
		protected virtual void OnBeforeBegin() {
			EventArgs e = ComposeEventArgs();
			if (TransactionEventsMediator!=null) TransactionEventsMediator.OnBeforeBegin(e);
		}
		
		protected virtual void OnAfterBegin() {
			EventArgs e = ComposeEventArgs();
			if (TransactionEventsMediator!=null) TransactionEventsMediator.OnAfterBegin(e);
		}

		protected virtual void OnBeforeCommit() {
			EventArgs e = ComposeEventArgs();
			if (TransactionEventsMediator!=null) TransactionEventsMediator.OnBeforeCommit(e);
		}

		protected virtual void OnAfterCommit() {
			EventArgs e = ComposeEventArgs();
			if (TransactionEventsMediator!=null) TransactionEventsMediator.OnAfterCommit(e);
		}
		
		protected virtual void OnBeforeAbort() {
			EventArgs e = ComposeEventArgs();
			if (TransactionEventsMediator!=null) TransactionEventsMediator.OnBeforeAbort(e);
		}

		protected virtual void OnAfterAbort() {
			EventArgs e = ComposeEventArgs();
			if (TransactionEventsMediator!=null) TransactionEventsMediator.OnAfterAbort(e);
		}
		
		
		#endregion

		
	}
}
