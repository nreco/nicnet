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
using System.Xml;

namespace NI.Common.Xml
{
	/// <summary>
	/// Modify XmlDocument operation interface.
	/// </summary>
	public interface IModifyXmlDocumentHandler
	{
		/// <summary>
		/// Modify xml document
		/// </summary>
		/// <param name="xmlDocument">Xml document that should be modified</param>
		void Modify(XmlDocument xmlDocument);
	}
}
