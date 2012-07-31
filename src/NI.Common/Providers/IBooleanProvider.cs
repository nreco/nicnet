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

namespace NI.Common.Providers
{
	/// <summary>
	/// Boolean value provider.
	/// </summary>
	public interface IBooleanProvider
	{
		/// <summary>
		/// Returns boolean value
		/// </summary>
		/// <exception cref="ArgumentException">when value cannot be provided in given context</exception>
		bool GetBoolean(object contextObj);
	}
}
