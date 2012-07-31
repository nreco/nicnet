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

namespace NI.Common.Transaction {

	/// <summary>
	/// Transaction mediator interface
	/// </summary>
	public interface ITransactionEventsMediator {

		/// <summary>
		/// Occurs when the transaction is on starting ('OnBeforeBegin' method is called)
		/// </summary>
		event EventHandler BeforeBegin;

		/// <summary>
		/// Occurs when the transaction is on committing ('OnBeforeCommit' method is called)
		/// </summary>		
		event EventHandler BeforeCommit;
		
		/// <summary>
		/// Occurs when the transaction is on aborting ('OnBeforeAbort' method is called)
		/// </summary>
		event EventHandler BeforeAbort;

		/// <summary>
		/// Occurs when the transaction is aborted ('OnAfterBegin' method is called)
		/// </summary>		
		event EventHandler AfterBegin;
		
		/// <summary>
		/// Occurs when the transaction is committed  ('OnAfterCommit' method is called)
		/// </summary>	
		event EventHandler AfterCommit;

		/// <summary>
		/// Occurs when the transaction is committed  ('OnAfterAbort' method is called)
		/// </summary>
		event EventHandler AfterAbort;
		
		/// <summary>
		/// Raises <see cref="BeforeBegin"/> event
		/// </summary>
		void OnBeforeBegin(EventArgs e);

		/// <summary>
		/// Raises <see cref="AfterBegin"/> event
		/// </summary>
		void OnAfterBegin(EventArgs e);

		/// <summary>
		/// Raises <see cref="BeforeCommit"/> event
		/// </summary>
		void OnBeforeCommit(EventArgs e);

		/// <summary>
		/// Raises <see cref="AfterCommit"/> event
		/// </summary>
		void OnAfterCommit(EventArgs e);

		/// <summary>
		/// Raises <see cref="BeforeAbort"/> event
		/// </summary>
		void OnBeforeAbort(EventArgs e);

		/// <summary>
		/// Raises <see cref="AfterAbort"/> event
		/// </summary>
		void OnAfterAbort(EventArgs e);
	}
	
	
}
