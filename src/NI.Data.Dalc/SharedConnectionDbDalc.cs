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

namespace NI.Data.Dalc
{
	/// <summary>
	/// Composite DbDalc.
	/// </summary>
	public class SharedConnectionDbDalc : DbDalc
	{
		IDbDalc[] _Satellites = null;
		
		/// <summary>
		/// Get or set satellite DbDalc objects
		/// </summary>
		public IDbDalc[] Satellites {
			get { return _Satellites; }
			set { _Satellites = value; }
		}
	
		/// <summary>
		/// <see cref="DbDalc.Connection"/>
		/// </summary>
		public override IDbConnection Connection {
			get { return base.Connection; }
			set {
				base.Connection = value;
				if (Satellites!=null)
					for (int i=0; i<Satellites.Length; i++)
						Satellites[i].Connection = value;
			}
		}

		/// <summary>
		/// <see cref="DbDalc.Transaction"/>
		/// </summary>
		[Dependency(Required=false)]
		public override IDbTransaction Transaction {
			get {
				return base.Transaction;
			}
			set { 
				base.Transaction = value; 
				if (Satellites!=null)
					for (int i=0; i<Satellites.Length; i++)
						Satellites[i].Transaction = value;
				
			}
		}	
	
		public SharedConnectionDbDalc()
		{
		}
	}
}
