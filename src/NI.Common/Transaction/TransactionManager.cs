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

namespace NI.Common.Transaction
{

	/// <summary>
	/// Composite ITransaction implementation based on one or more underlying ITransaction components
	/// </summary>
	public class TransactionManager : ITransaction
	{
		ITransaction[] _Transactions;
		
		/// <summary>
		/// Get or set transaction components
		/// </summary>
		public ITransaction[] Transactions {
			get { return _Transactions; }
			set { _Transactions = value; }
		}
				

		public TransactionManager()
		{
		}
		
		/// <summary>
		/// <see cref="ITransaction.Begin"/>
		/// </summary>
		public virtual void Begin() {
			foreach (ITransaction transaction in Transactions)
				transaction.Begin();	
		}
		
		/// <summary>
		/// <see cref="ITransaction.Abort"/>
		/// </summary>
		public virtual void Abort() {
			foreach (ITransaction transaction in Transactions)
				transaction.Abort();	
		}
		
		/// <summary>
		/// <see cref="ITransaction.Commit"/>
		/// </summary>
		public virtual void Commit() {
			foreach (ITransaction transaction in Transactions)
				transaction.Commit();	
		}
		
	}
}
