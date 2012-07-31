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
using System.Data.SqlClient;

namespace NI.Data.SqlClient
{
	/// <summary>
	/// SqlAdapterWrapper.
	/// </summary>
	public class SqlAdapterWrapper : IDbDataAdapterWrapper
	{
		IDbDataAdapter _Adapter;
		IDbCommandWrapper _SelectCommandWrapper;
		IDbCommandWrapper _InsertCommandWrapper;
		IDbCommandWrapper _DeleteCommandWrapper;
		IDbCommandWrapper _UpdateCommandWrapper;
		
		public IDbDataAdapter Adapter {	get { return _Adapter; } }

		public IDbCommandWrapper SelectCommadWrapper {
			get { return _SelectCommandWrapper; }
			set {
				_SelectCommandWrapper = value;
				Adapter.SelectCommand = SelectCommadWrapper.Command;
			}
		}

		public IDbCommandWrapper InsertCommandWrapper {
			get { return _InsertCommandWrapper; }
			set {
				_InsertCommandWrapper = value;
				Adapter.InsertCommand = InsertCommandWrapper.Command;
			}
		}
		
		public IDbCommandWrapper DeleteCommandWrapper { 
			get { return _DeleteCommandWrapper; }
			set {
				_DeleteCommandWrapper = value;
				Adapter.DeleteCommand = DeleteCommandWrapper.Command;
			}
		}
		
		public IDbCommandWrapper UpdateCommandWrapper {
			get { return _UpdateCommandWrapper; }
			set {
				_UpdateCommandWrapper = value;
				Adapter.UpdateCommand = UpdateCommandWrapper.Command;
			}
		}
				
		/// <summary>
		/// Row updating event
		/// </summary>
		public event DbRowUpdatingEventHandler RowUpdating;
		
		/// <summary>
		/// Row updated event
		/// </summary>
		public event DbRowUpdatedEventHandler RowUpdated;
	
		public SqlAdapterWrapper(SqlDataAdapter adapter)
		{
			_Adapter = adapter;
			// Catch adapter events
			adapter.RowUpdating += new SqlRowUpdatingEventHandler(this.rowUpdating);
			adapter.RowUpdated  += new SqlRowUpdatedEventHandler(this.rowUpdated);
		}
		
		private void rowUpdating(object sender, SqlRowUpdatingEventArgs e) {
			if (this.RowUpdating!=null)
				RowUpdating(this, e);
		}

		private void rowUpdated(object sender, SqlRowUpdatedEventArgs e) {
			if (this.RowUpdated!=null)
				RowUpdated(this, e);
		}
		
		

		
		
	}
}
