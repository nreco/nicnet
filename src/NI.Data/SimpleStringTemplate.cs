using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NI.Data {

	/// <summary>
	/// Simple string template that handles conditional formatting
	/// </summary>
	public class SimpleStringTemplate {
		internal readonly static string NotApplicable = "\0";

		protected string Template;

		public SimpleStringTemplate(string tpl) {
			Template = tpl;
		}

		/// <summary>
		/// Performs default SQL template parsing that can handle simple code snippets
		/// </summary>
		public string FormatTemplate(IDictionary<string,object> props) {

			var sb = new StringBuilder();
			var sqlTpl = Template;
			int pos = 0;
			while (pos < sqlTpl.Length) {
				var c = sqlTpl[pos];
				if (c == '@') {
					int endPos;
					var name = ReadName(sqlTpl, pos + 1, out endPos);
					if (name != null) {
						string[] formatOptions;
						try {
							formatOptions = ReadFormatOptions(sqlTpl, endPos, out endPos);
						} catch (Exception ex) {
							throw new Exception(String.Format("Parse error (format options of property {0}) at {1}: {2}",
								name, pos, ex.Message), ex);
						}
						object callRes;
						try {
							callRes = props.ContainsKey(name) ? props[name] : null;
						} catch (Exception ex) {
							throw new Exception(String.Format("Evaluation of property {0} at position {1} failed: {2}",
								name, pos, ex.Message), ex);
						}
						if (callRes != NotApplicable) {
							var fmtNotEmpty = formatOptions != null && formatOptions.Length > 0 ? formatOptions[0] : "{0}";
							var fmtEmpty = formatOptions != null && formatOptions.Length > 1 ? formatOptions[1] : "";
							try {
								sb.Append(callRes != null && Convert.ToString(callRes) != String.Empty ?
										String.Format(fmtNotEmpty, callRes) : String.Format(fmtEmpty, callRes)
								);
							} catch (Exception ex) {
								throw new Exception(String.Format("Format of property {0} at position {1} failed: {2}",
									name, pos, ex.Message), ex);
							}
						}
						pos = endPos;
						continue;
					}
				}
				sb.Append(c);
				pos++;
			}

			return sb.ToString();
		}

		protected string[] ReadFormatOptions(string s, int start, out int newStart) {
			newStart = start;
			if (start >= s.Length || s[start] != '[')
				return null;
			start++;
			var opts = new List<string>();
			var pSb = new StringBuilder();
			while (start < s.Length) {
				if (s[start] == ']') {
					break;
				} else if (s[start] == ';') {
					opts.Add(pSb.ToString());
					pSb.Clear();
				} else {
					pSb.Append(s[start]);
				}
				start++;
			}
			opts.Add(pSb.ToString());
			if (s[start] != ']')
				throw new FormatException("Invalid format options");
			if (opts.Count > 2)
				throw new FormatException("Too many format options");
			newStart = start + 1;
			return opts.ToArray();
		}

		protected string ReadName(string s, int start, out int newStart) {
			newStart = start;
			// should start with letter
			if (start >= s.Length || !Char.IsLetter(s[start]))
				return null;
			// rest of the name: letters or digits
			while (start < s.Length && Char.IsLetterOrDigit(s[start]))
				start++;

			var name = s.Substring(newStart, start - newStart);
			newStart = start;
			return name;
		}


	}
}
