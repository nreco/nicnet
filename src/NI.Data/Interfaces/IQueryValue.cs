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
	/// Interface for query left or right value
	/// </summary>
	public interface IQueryValue
	{
	}
	
	/// <summary>
	/// Interface for query field name value
	/// </summary>
	public interface IQueryFieldValue : IQueryValue {
		string Name { get; }
	}
	
	/// <summary>
	/// Interface for query constant value
	/// </summary>
	public interface IQueryConstantValue : IQueryValue {
		object Value { get; }
		TypeCode Type { get; }
	}

	/// <summary>
	/// Interface for query raw value
	/// </summary>
	public interface IQueryRawValue : IQueryValue {
		string Value { get; }
	}

	
}
