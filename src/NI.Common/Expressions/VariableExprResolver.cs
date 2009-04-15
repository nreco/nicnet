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
using System.ComponentModel;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;

namespace NI.Common.Expressions
{
	/// <summary>
	/// Variable expression resolver is used for obtaining variable from context dictionary (expression treated as key). Optionally, format string can be defined after comma.
	/// </summary>
	public class VariableExprResolver : IExpressionResolver
	{
		bool _Escape = false;
		bool _EscapeQuote = false;
		XmlEncodeExprResolver EncodeExprResolver = new XmlEncodeExprResolver();

		/// <summary>
		/// Get or set output XML escaping flag
		/// </summary>
		public bool Escape {
			get { return _Escape; }
			set { _Escape = value; }
		}

		/// <summary>
		/// Get or set output XML quotes escaping flag
		/// </summary>
		public bool EscapeQuote {
			get { return _EscapeQuote; }
			set { _EscapeQuote = value; }
		}

		public VariableExprResolver()
		{
		}

		public virtual object Evaluate(IDictionary context, string expression) {
			int commaIdx = expression.IndexOf(',');
			object value;
			if (commaIdx != -1) {
				value = context[expression.Substring(0, commaIdx)];
				IFormattable formattable = value as IFormattable;
				if (formattable != null)
					value = formattable.ToString(expression.Substring(commaIdx + 1), null);
			} else
				value = context[expression];
			
			return PrepareValue(value);
		}
		
		protected virtual object PrepareValue(object value) {
			if (!Escape)
				return value;
			else {
				return EscapeQuote ? XmlEscapeQuote( Convert.ToString(value) ) : XmlEscape( Convert.ToString(value) );
			}
		}
		
		protected string XmlEscape(string value) {
			EncodeExprResolver.EncodeMode = XmlEncodeExprResolver.EncodeModeType.NodeText;
			return Convert.ToString( EncodeExprResolver.Evaluate(null, value) );
		}
		
		protected string XmlEscapeQuote(string value) {
			EncodeExprResolver.EncodeMode = XmlEncodeExprResolver.EncodeModeType.Attribute;
			return Convert.ToString( EncodeExprResolver.Evaluate(null, value) );
		}
		
	}
}
