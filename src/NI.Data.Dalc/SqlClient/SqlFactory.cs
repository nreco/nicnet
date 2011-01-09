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
using System.Data.Common;
using System.Data.SqlClient;
using System.ComponentModel;

namespace NI.Data.Dalc.SqlClient
{

	public class SqlFactory : Component, IDbCommandWrapperFactory, IDbDataAdapterWrapperFactory
	{
		int _CommantTimeout = -1;
		DbTypeResolver _DbTypeResolver = new DbTypeResolver();
		bool _TopOptimizationEnabled = false;
		bool _ConstOptimizationEnabled = false;
		bool _NameBracketsEnabled = false;

		public DbTypeResolver DbTypeResolver {
			get { return _DbTypeResolver; }
			set { _DbTypeResolver = value; }
		}
		
		public int CommandTimeout {
			get { return _CommantTimeout; }
			set { _CommantTimeout = value; }
		}

		public bool TopOptimizationEnabled {
			get { return _TopOptimizationEnabled; }
			set { _TopOptimizationEnabled = value; }
		}

		public bool ConstOptimizationEnabled {
			get { return _ConstOptimizationEnabled; }
			set { _ConstOptimizationEnabled = value; }
		}

		public bool NameBracketsEnabled {
			get { return _NameBracketsEnabled; }
			set { _NameBracketsEnabled = value; }
		}
		
		IDbCommandWrapper IDbCommandWrapperFactory.CreateInstance() {
			SqlCommand sqlCmd = new SqlCommand();
			if (CommandTimeout>=0)
				sqlCmd.CommandTimeout = CommandTimeout;
			return new SqlCommandWrapper(sqlCmd, DbTypeResolver, TopOptimizationEnabled, ConstOptimizationEnabled, NameBracketsEnabled);
		}

		IDbDataAdapterWrapper IDbDataAdapterWrapperFactory.CreateInstance() {
			return new SqlAdapterWrapper( new SqlDataAdapter() );
		}
		

	}
}
