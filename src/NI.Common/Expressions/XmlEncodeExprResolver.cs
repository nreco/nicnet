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
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;

namespace NI.Common.Expressions {
	
	/// <summary>
	/// XML context encoding expression resolver
	/// </summary>
	public class XmlEncodeExprResolver : IExpressionResolver {

		public enum EncodeModeType { NodeText, Attribute };

		EncodeModeType _EncodeMode = EncodeModeType.NodeText;

		public EncodeModeType EncodeMode {
			get { return _EncodeMode; }
			set { _EncodeMode = value; }
		}

		public XmlEncodeExprResolver() { }

		public XmlEncodeExprResolver(EncodeModeType encodeMode) {
			EncodeMode = encodeMode;
		}

		public object Evaluate(IDictionary context, string expression) {
			if (EncodeMode==EncodeModeType.NodeText)
				return XmlEncode(expression);
			if (EncodeMode==EncodeModeType.Attribute)
				return XmlAttrEncode(expression);
			return null;
		}

		protected string XmlEncode(string value) {
			StringWriter strWr = new StringWriter();
			XmlTextWriter xmlWr = new XmlTextWriter(strWr);
			xmlWr.WriteString( value );
			return strWr.ToString();			
		}
		
		protected string XmlAttrEncode(string value) {
			StringWriter strWr = new StringWriter();
			XmlTextWriter xmlWr = new XmlTextWriter(strWr);
			xmlWr.WriteAttributeString("a", value);
			string result = strWr.ToString();
			return result.Substring(3, result.Length-4);
		}


	}
}
