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
using System.Xml;

using NI.Common.Providers;
using NI.Common.Caching;

namespace NI.Common.Xml {
	
	public class CacheModifyXmlDocumentHandler : IModifyXmlDocumentHandler {
		ICache _Cache;
		IModifyXmlDocumentHandler _UnderlyingHandler;
		IStringProvider _XmlDocKeyProvider;

		public IStringProvider XmlDocKeyProvider {
			get { return _XmlDocKeyProvider; }
			set { _XmlDocKeyProvider = value; }
		}

		public IModifyXmlDocumentHandler UnderlyingHandler {
			get { return _UnderlyingHandler; }
			set { _UnderlyingHandler = value; }
		}

		public ICache Cache {
			get { return _Cache; }
			set { _Cache = value; }
		}

		public CacheModifyXmlDocumentHandler() { }

		public void Modify(XmlDocument xmlDocument) {
			string cacheKey = XmlDocKeyProvider.GetString( xmlDocument.InnerXml );
			string cachedXml = Cache.Get(cacheKey) as string;
			if (cachedXml==null) {
				UnderlyingHandler.Modify( xmlDocument );
				cachedXml = xmlDocument.InnerXml;
				Cache.Put(cacheKey, cachedXml);
			} else {
				xmlDocument.InnerXml = cachedXml;
			}

		}

	}
}
