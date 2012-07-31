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
using System.Xml.Schema;
using System.Configuration;
using System.Reflection;
using System.ComponentModel;
using System.Text;

using NI.Common;
using NI.Common.Xml;

namespace NI.Ioc
{
	
	/// <summary>
	/// Extension of ComponentsConfig that supports configuration section handler
	/// (so you may place configuration in the app.config or web.config)
	/// </summary>
	/// <example><code>
	/// &lt;configSections&gt;
	///		&lt;section name="components" type="NI.Ioc.XmlComponentsConfig, NI.Ioc" /&gt;
	///	&lt;/configSections&gt;
	///	&lt;components&gt;
	///		&lt;!-- components definitions --&gt;
	///	&lt;/components&gt;
	/// </code></example>
	public class XmlComponentsConfig : ComponentsConfig, IConfigurationSectionHandler {

		IModifyXmlDocumentHandler _Preprocessor;
		string OriginalXml = null;

		/// <summary>
		/// Get or set preprocessor component
		/// </summary>
		public IModifyXmlDocumentHandler Preprocessor {
			get { return _Preprocessor; }
			set { _Preprocessor = value; }
		}

		/// <summary>
		/// Without parameters can be used only as IConfigurationSectionHandler
		/// </summary>
		public XmlComponentsConfig() {
		}

		public XmlComponentsConfig(string xml) : this(xml, null) { }

		
		public XmlComponentsConfig(string xml, IModifyXmlDocumentHandler preprocessor) {
			Preprocessor = preprocessor;
			if (Preprocessor!=null)
				OriginalXml = xml;
			Init(xml);
		}

		protected virtual void Init(string xml) {
			// load XML into XmlDocument
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.PreserveWhitespace = true; // whitespaces are significant
			XmlDocumentLoader xmlDocLoader = new XmlDocumentLoader( xml, xmlDoc );
			
			// perform preprocessing
			if (Preprocessor!=null)
				Preprocessor.Modify(xmlDoc);
			
            #if NET_1_1
            NI.Common.Xml.XmlSchemaValidator schemaValidator = new NI.Common.Xml.XmlSchemaValidator();
			schemaValidator.Namespaces = false;
			schemaValidator.Xml = xmlDoc.OuterXml;
			schemaValidator.Xsd = (new StringLoader(GetXsdStream()) ).Result;
			schemaValidator.Validate();
			#else
			XmlSchema schema = XmlSchema.Read( GetXsdStream(), null);
			xmlDoc.Schemas.Add( schema );
			xmlDoc.Validate(new ValidationEventHandler(ValidationCallBack));
			#endif
			
			this.Load( xmlDoc.DocumentElement );
		}


		public void Refresh() {
			if (OriginalXml!=null)
				Init(OriginalXml);
		}

		
		object IConfigurationSectionHandler.Create(object parent, object input, XmlNode section) {
			try {
				XmlComponentsConfig config = new XmlComponentsConfig(section.OuterXml, Preprocessor); 
				return config;
			} catch (Exception ex) {
				throw new ConfigurationException( ex.Message, ex);
			}
			
		}

		Stream GetXsdStream() {
			string name = this.GetType().Namespace + ".ComponentsConfigSchema.xsd";
			return Assembly.GetExecutingAssembly().GetManifestResourceStream( name ); 
		}
		
		protected void ValidationCallBack( object sender, ValidationEventArgs args ) {
			if (args.Exception is XmlSchemaValidationException) {
				XmlSchemaValidationException validationEx = (XmlSchemaValidationException)args.Exception;
				if (validationEx.SourceObject is XmlNode) {
					XmlNode xmlNode = (XmlNode)validationEx.SourceObject;
					throw new XmlSchemaValidationException(
						String.Format("{0}\nancestor axis:\n{1}",
							args.Message, FormatXmlValidationTrace(xmlNode)),
						 args.Exception, args.Exception.LineNumber, args.Exception.LinePosition);
				}
			}
			throw new XmlSchemaValidationException(String.Format("{0}", args.Message, args.Exception.LineNumber), args.Exception, args.Exception.LineNumber, args.Exception.LinePosition);
		}
		
		protected string FormatXmlValidationTrace(XmlNode node) {
			if (node==null) 
				return String.Empty;
			StringBuilder sb = new StringBuilder();
			if (node is XmlElement) {
				sb.Append("<");
				sb.Append(node.Name);
				foreach (XmlAttribute xmlAttr in node.Attributes)
					sb.AppendFormat(" {0}=\"{1}\"", xmlAttr.Name, xmlAttr.Value);
				sb.Append(">\n");
			}
			if (node is XmlText) {
				sb.Append("[");
				sb.Append( ((XmlText)node).Data );
				sb.Append("]\n");
			}
			return FormatXmlValidationTrace(node.ParentNode)+sb.ToString();
		}
		
		
	}


}
