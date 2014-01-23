#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2008-2013 Vitalii Fedorchenko
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

namespace NI.Data
{
	/// <summary>
	/// Represents abstract event stream (message bus) for pushing data events
	/// </summary>
	public interface IEventStream
	{
		void Push(object sender, object eventData);
	}
	
	
	
}
