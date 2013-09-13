#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
 * Copyright 2008-2013 Vitalii Fedorchenko (changes and v.2)
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
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;

namespace NI.Data
{

	/// <summary>
	/// DB Command generator interface
	/// </summary>
	public interface IDbCommandGenerator 
	{
		IDbCommand ComposeSelect(Query query);

		IDbCommand ComposeInsert(DataTable table);
		IDbCommand ComposeInsert(IDictionary<string,IQueryValue> data, string sourceName);

		IDbCommand ComposeDelete(DataTable table);
		IDbCommand ComposeDelete(Query query);

		IDbCommand ComposeUpdate(DataTable table);
		IDbCommand ComposeUpdate(IDictionary<string, IQueryValue> data, Query query);
	}
}
