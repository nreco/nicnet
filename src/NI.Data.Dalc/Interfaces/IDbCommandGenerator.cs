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
using System.ComponentModel;
using System.Collections;

namespace NI.Data.Dalc
{

	/// <summary>
	/// DB Command generator interface
	/// </summary>
	public interface IDbCommandGenerator 
	{
		IDbCommandWrapperFactory CommandWrapperFactory { get; set; }
	
		IDbCommandWrapper ComposeSelect(IQuery query);
		
		IDbCommandWrapper ComposeInsert(DataTable table);
		IDbCommandWrapper ComposeInsert(IDictionary data, string sourceName);

		IDbCommandWrapper ComposeDelete(DataTable table);
		IDbCommandWrapper ComposeDelete(IQuery query);

		IDbCommandWrapper ComposeUpdate(DataTable table);
		IDbCommandWrapper ComposeUpdate(IDictionary changesData, IQuery query);
	}
}
