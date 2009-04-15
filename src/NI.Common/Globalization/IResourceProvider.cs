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
using System.Collections;
using System.Text;
using System.Globalization;

namespace NI.Common.Globalization {
	
	/// <summary>
	/// Gets resource object for specific culture. If culture is not specified current context UI culture is used.
	/// </summary>
	public interface IResourceProvider {
		object GetResource(string id);
		object GetResource(string id, string placeId);
		object GetResource(string id, string placeId, CultureInfo culture);
		object GetResource(string id, CultureInfo culture);
	}
	
	
}
