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
	/// </summary>
	public class TransactionEventsMediator : ITransactionEventsMediator
	{
		public event EventHandler BeforeBegin;
		public event EventHandler BeforeCommit;
		public event EventHandler BeforeAbort;

		public event EventHandler AfterBegin;
		public event EventHandler AfterCommit;
		public event EventHandler AfterAbort;
		
		
		public TransactionEventsMediator()
		{
		}
		
		
		public virtual void OnBeforeBegin(EventArgs e) {
			if (BeforeBegin!=null)
				BeforeBegin(this, e);
		}

		public virtual void OnAfterBegin(EventArgs e) {
			if (AfterBegin!=null)
				AfterBegin(this, e);
		}

		public virtual void OnBeforeCommit(EventArgs e) {
			if (BeforeCommit!=null)
				BeforeCommit(this, e);
		}

		public virtual void OnAfterCommit(EventArgs e) {
			if (AfterCommit!=null)
				AfterCommit(this, e);
		}

		public virtual void OnBeforeAbort(EventArgs e) {
			if (BeforeAbort!=null)
				BeforeAbort(this, e);
		}

		public void OnAfterAbort(EventArgs e) {
			if (AfterAbort!=null)
				AfterAbort(this, e);
		}

				
	}
}
