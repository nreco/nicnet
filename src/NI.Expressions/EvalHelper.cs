using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.IO;

namespace NI.Expressions {
	
	/// <summary>
	/// Helper class that includes typical evaluation functions used in StringTemplate
	/// </summary>
	public static class EvalHelper {

		public static string IsInRole(IDictionary context, string role) {
			return Thread.CurrentPrincipal.IsInRole(role).ToString();
		}

		public static string Variable(IDictionary context, string contextKey) {
			int commaIdx = contextKey.IndexOfAny(new[]{',',':'});
			object value;
			if (commaIdx != -1) {
				value = context[contextKey.Substring(0, commaIdx)];
				IFormattable formattable = value as IFormattable;
				if (formattable != null)
					value = formattable.ToString(contextKey.Substring(commaIdx + 1), null);
			} else
				value = context[contextKey];

			return Convert.ToString(value);
		}

		public static string XmlEncode(IDictionary context, string s) {
			StringWriter strWr = new StringWriter();
			XmlTextWriter xmlWr = new XmlTextWriter(strWr);
			xmlWr.WriteString(s);
			return strWr.ToString();					
		}

		public static string XmlAttributeEncode(IDictionary context, string s) {
			StringWriter strWr = new StringWriter();
			XmlTextWriter xmlWr = new XmlTextWriter(strWr);
			xmlWr.WriteAttributeString("a", s);
			string result = strWr.ToString();
			return result.Substring(3, result.Length - 4);
		}


	}

}
