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
using System.Collections;

using NI.Common;
using NI.Common.Expressions;
using NI.Common.Providers;

using NI.Data;

namespace NI.Data
{
	/// <summary>
	/// Load dataset using query providers.
	/// </summary>
	public class DalcDataSetProvider : IDataSetProvider, IObjectProvider
	{
		IQueryProvider[] _QueryProviders;
		IDalc _Dalc;
				
		/// <summary>
		/// Get or set relational expressions used to load data
		/// </summary>
		public IQueryProvider[] QueryProviders {
			get { return _QueryProviders; }
			set { _QueryProviders = value; }
		}
		
		/// <summary>
		/// Get or set DALC component to load data from
		/// </summary>
		public IDalc Dalc {
			get { return _Dalc; }
			set { _Dalc = value; }
		}
	
		public DalcDataSetProvider()
		{
		}
		
		public DataSet GetDataSet(object contextObj) {
			DataSet ds = new DataSet();
			foreach (IQueryProvider queryProvider in QueryProviders) {
				Query q = queryProvider.GetQuery(contextObj);
				Dalc.Load( ds, q );
			}
		
			return ds;
		}
		
		public object GetObject(object context) {
			return GetDataSet(context);
		}
		
	}
}
