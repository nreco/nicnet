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

namespace NI.Data
{
	/// <summary>
	/// Inteface for filter condition object
	/// </summary>
	public interface IQueryConditionNode : IQueryNode
	{
		IQueryValue LValue { get; }
		Conditions Condition { get; }
		IQueryValue RValue { get; }
	}
	
	[Flags]
	public enum Conditions {
		Equal = 1,
		LessThan = 2,
		GreaterThan = 4,
		Like = 8,
		In = 16,
		Null = 32,
		Not = 64
	}
		
}
