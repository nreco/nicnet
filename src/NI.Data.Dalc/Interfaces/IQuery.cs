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

namespace NI.Data.Dalc
{
	/// <summary>
	/// Inteface for Query
	/// </summary>
	public interface IQuery : IQueryValue
	{
		string SourceName { get; }
		
		string[] Sort { get; }
		
		IQueryNode Root { get; }
		
		string[] Fields { get; }
		
		int StartRecord { get; }
		int RecordCount { get; }
		
		
	}
}
