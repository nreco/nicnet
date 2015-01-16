#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013-2014 Vitalii Fedorchenko
 * Copyright 2014 NewtonIdeas
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using NI.Data;
using NI.Data.Storage.Model;

namespace NI.Data.Storage {
	
	/// <summary>
	/// Represents abstract SQL-specific storage for <see cref="ObjectContainer"/>
	/// </summary>
	public interface ISqlObjectContainerStorage : IObjectContainerStorage {

		/// <summary>
		/// Execute object query and pass result as <see cref="IDataReader"/> for specified handler
		/// </summary>
		/// <param name="q">object query to execute</param>
		/// <param name="handler">delegate that accepts <see cref="IDataReader"/> with query result</param>
		void LoadObjectReader(Query q, Action<IDataReader> handler);
	}
}
