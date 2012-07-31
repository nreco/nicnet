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
using System.Xml;
using System.IO;

namespace NI.Common.Xml
{
	/// <summary>
	/// XmlDocument loader
	/// </summary>
	public class XmlDocumentLoader
	{
		XmlDocument _Result;
		XmlNamespaceManager _NsManager;
		string _DefaultNsPrefix = "ns";
		
		public string DefaultNsPrefix {
			get { return _DefaultNsPrefix; }
			set { _DefaultNsPrefix = value; }
		}
		
		public XmlDocument Result {
			get { return _Result; }
		}
		
		public XmlNamespaceManager NsManager {
			get { return _NsManager; }
		}

		public XmlDocumentLoader(string xml) : this(xml, new XmlDocument() ) {
		}
		
		public XmlDocumentLoader(string xml, XmlDocument xmlDoc) {
			_Result = xmlDoc;
			_NsManager = new XmlNamespaceManager(Result.NameTable);
			Result.LoadXml(xml);
			CollectNamespaces( Result );
			
			if (NsManager.DefaultNamespace!=null && NsManager.DefaultNamespace!=String.Empty)
				NsManager.AddNamespace( DefaultNsPrefix, NsManager.DefaultNamespace);
		}
		
		
		protected virtual void CollectNamespaces(XmlNode node) {
			//node.
			if (node.Prefix!=null && node.NamespaceURI!=null && node.NamespaceURI!=String.Empty)
				NsManager.AddNamespace( node.Prefix, node.NamespaceURI );
			foreach (XmlNode childNode in node.ChildNodes)
				CollectNamespaces(childNode);
		}
		
		
		
	}
}
