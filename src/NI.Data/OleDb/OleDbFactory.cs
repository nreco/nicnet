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
using System.Data.OleDb;
using System.ComponentModel;

namespace NI.Data.OleDb
{

	public class OleDbFactory : IDbCommandWrapperFactory, IDbDataAdapterWrapperFactory
	{
		IQueryFieldValueFormatter _QueryFieldValueFormatter = null;

		/// <summary>
		/// Get or set default query field value formatter
		/// </summary>
		public IQueryFieldValueFormatter QueryFieldValueFormatter {
			get { return _QueryFieldValueFormatter; }
			set { _QueryFieldValueFormatter = value; }
		}
	
		IDbCommandWrapper IDbCommandWrapperFactory.CreateInstance() {
			OleDbCommandWrapper cmdWrapper = new OleDbCommandWrapper( new OleDbCommand() );
			cmdWrapper.QueryFieldValueFormatter = QueryFieldValueFormatter;
			return cmdWrapper;
		}

		IDbDataAdapterWrapper IDbDataAdapterWrapperFactory.CreateInstance() {
			return new OleDbAdapterWrapper( new OleDbDataAdapter() );
		}
		
		
		
		


	}
}
