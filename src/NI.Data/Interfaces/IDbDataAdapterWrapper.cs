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
using System.Data.Common;

namespace NI.Data
{

	public delegate void DbRowUpdatingEventHandler(object sender, RowUpdatingEventArgs args);
	public delegate void DbRowUpdatedEventHandler(object sender, RowUpdatedEventArgs args);

	/// <summary>
	/// </summary>
	public interface IDbDataAdapterWrapper
	{
		/// <summary>
		/// Adapter
		/// </summary>
		IDbDataAdapter Adapter { get; }

		IDbCommandWrapper SelectCommadWrapper { get; set; }
		IDbCommandWrapper InsertCommandWrapper { get; set; }
		IDbCommandWrapper DeleteCommandWrapper { get; set; }
		IDbCommandWrapper UpdateCommandWrapper { get; set; }

		/// <summary>
		/// Row updating event
		/// </summary>
		event DbRowUpdatingEventHandler RowUpdating;
		
		/// <summary>
		/// Row updated event
		/// </summary>
		event DbRowUpdatedEventHandler RowUpdated;

	}
}
