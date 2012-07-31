#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2009 NewtonIdeas
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
using MySql.Data.MySqlClient;

namespace NI.Data.MySql
{
	/// <summary>
	/// MySql data adapter wrapper
	/// </summary>
	public class MySqlAdapterWrapper : IDbDataAdapterWrapper
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

		public MySqlAdapterWrapper(MySqlDataAdapter adapter)
		{
			_Adapter = adapter;
			// Catch adapter events
			adapter.RowUpdating += new MySqlRowUpdatingEventHandler(this.rowUpdating);
			adapter.RowUpdated += new MySqlRowUpdatedEventHandler(this.rowUpdated);
		}

		private void rowUpdating(object sender, MySqlRowUpdatingEventArgs e) {
			if (this.RowUpdating!=null)
				RowUpdating(this, e);
		}

		private void rowUpdated(object sender, MySqlRowUpdatedEventArgs e) {
			if (this.RowUpdated!=null)
				RowUpdated(this, e);
		}
		
		

		
		
	}
}
