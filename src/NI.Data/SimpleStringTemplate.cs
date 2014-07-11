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

		protected char[] ExtraNameChars = new [] {'_','-'};

		public int RecursionLevel { get; private set; }

		public SimpleStringTemplate(string tpl) {
			Template = tpl;
			RecursionLevel = 1;
		}

		public SimpleStringTemplate(string tpl, int recursionLevel) {
			Template = tpl;
			RecursionLevel = recursionLevel;
		}

		/// <summary>
		/// Performs default SQL template parsing that can handle simple code snippets
		/// </summary>
		public string FormatTemplate(IDictionary<string,object> props) {
			var sb = new StringBuilder();
			string tpl = Template;
			for (int i = 0; i < RecursionLevel; i++) {
				sb.Clear();
				if (ReplaceTokens(tpl, props, sb) == 0)
					break;
				tpl = sb.ToString();
			}

			return sb.ToString();
		}

		protected int ReplaceTokens(string tpl, IDictionary<string, object> props, StringBuilder sb) {
			int pos = 0;
			int matchedTokensCount = 0;
			while (pos < tpl.Length) {
				var c = tpl[pos];
				if (c == '@') {
					int endPos;
					var name = ReadName(tpl, pos + 1, out endPos);
					if (name != null) {
						string[] formatOptions;
						try {
							formatOptions = ReadFormatOptions(tpl, endPos, out endPos);
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
						matchedTokensCount++;
						continue;
					}
				}
				sb.Append(c);
				pos++;
			}
			return matchedTokensCount;
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
			while (start < s.Length && (Char.IsLetterOrDigit(s[start]) || Array.IndexOf( ExtraNameChars, s[start])>=0 ) )
				start++;

			var name = s.Substring(newStart, start - newStart);
			newStart = start;
			return name;
		}


	}
}
