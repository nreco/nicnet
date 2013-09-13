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

namespace NI.Data
{
	
	/// <summary>
	/// DB command wrapper interface
	/// </summary>
	/// <remarks>
	/// In most cases IDbCommand implementations cannot be extended (for instance, SqlCommand is sealed class).
	/// </remarks>
	public interface IDbCommandWrapper {

		/// <summary>
		/// Get wrapped IDbCommand
		/// </summary>
		IDbCommand Command { get; }
		
		/// <summary>
		/// Set context transaction
		/// </summary>
		/// <remarks>
		/// At least SqlCommand MONO implementations is buggy so you cannot
		/// directly set SqlCommand.Transaction=null
		/// </remarks>
		void SetTransaction(IDbTransaction transaction);

		/// <summary>
		/// Gets DB-specific Command Parameter Placeholder
		/// </summary>
		/// <remarks>
		/// In some cases IDbCommand implementation requires special handling for
		/// command parameters placeholder in the command text (example: OleDbCommand
		/// works with positional parameters, so '?' should be used instead of
		/// parameter name).
		/// </remarks>
		string GetCmdParameterPlaceholder(string paramName);
		
		
		/// <summary>
		/// Create command parameter by source data column
		/// </summary>
		/// <remarks>
		/// In some cases IDbDataParameter should be initialized in special manner
		/// (depends on implementation, example: OleDbParameter+Access database)
		/// </remarks>
		IDbDataParameter CreateCmdParameter(DataColumn sourceColumn);

		/// <summary>
		/// Create command parameter by source data column
		/// </summary>
		/// <remarks>
		/// In some cases IDbDataParameter should be initialized in special manner
		/// (depends on implementation, example: OleDbParameter+Access database)
		/// </remarks>
		IDbDataParameter CreateCmdParameter(object constantValue);
		
		
		/// <summary>
		/// Create DB sql builder for this command wrapper
		/// </summary>
		IDbSqlBuilder CreateSqlBuilder();

		/// <summary>
		/// Returns inserted row id
		/// </summary>
		object GetInsertId();

	}
	
	
}
