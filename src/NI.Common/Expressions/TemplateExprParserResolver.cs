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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace NI.Common.Expressions {
    public class TemplateExprParserResolver : IExpressionResolver {

        IExpressionDescriptor[] _ExprDescriptors;
        Dictionary<string, IExpressionResolver> ResolversHashtable;
        const int MaxRecursiveSteps = 50;
        const int HashtableUsingThreshold = 5;
        bool _Recursive;
        string _ExprRegexBegin = @"[{]";
        string _ExprRegexEnd = @"[:](?<expr>((\\[}])|[^}])*?)[}]";
        bool _UseRegexImplementation;
        Hashtable DescriptorRegexCache;

        /// <summary>
        /// Get or set expression descriptors
        /// </summary>
        public IExpressionDescriptor[] ExprDescriptors {
            get { return _ExprDescriptors; }
            set { 
                _ExprDescriptors = value;
                if (value.Length > HashtableUsingThreshold) {
                    ResolversHashtable = new Dictionary<string, IExpressionResolver>();
                    foreach (IExpressionDescriptor exprDescriptor in ExprDescriptors)
                        ResolversHashtable[exprDescriptor.Marker] = exprDescriptor.ExprResolver;
                } else {
                    ResolversHashtable = null;
                }
            }
        }

        /// <summary>
        /// Get or set flag that indicates whether markers should be processed recursively
        /// </summary>
        public bool Recursive {
            get { return _Recursive; }
            set { _Recursive = value; }
        }

        /// <summary>
        /// Get or set sub-expression regex begin
        /// </summary>
        public string ExprRegexBegin {
            get { return _ExprRegexBegin; }
            set {
                UseRegexImplementation = true;
                _ExprRegexBegin = value;
            }
        }

        /// <summary>
        /// Get or set sub-expression regex end
        /// </summary>
        public string ExprRegexEnd {
            get { return _ExprRegexEnd; }
            set {
                UseRegexImplementation = true;
                _ExprRegexEnd = value;
            }
        }

        /// <summary>
        /// Get or set flag that indicates whether old implementation (with regexes) should be used
        /// (for compatibility)
        /// </summary>
        public bool UseRegexImplementation {
            get { return _UseRegexImplementation; }
            set { 
                if (value && DescriptorRegexCache == null)
                    DescriptorRegexCache = new Hashtable();
                _UseRegexImplementation = value;
            }
        }

        static Hashtable _EmptyHashtable;
        static Hashtable EmptyHashtable {
            get {
                if (_EmptyHashtable == null)
                    _EmptyHashtable = new Hashtable();
                return _EmptyHashtable;
            }
        }

		public TemplateExprParserResolver() {
		}

		public TemplateExprParserResolver(IExpressionDescriptor[] exprDescriptors) {
			ExprDescriptors = exprDescriptors;
		}

        public object Evaluate(IDictionary context, string expression) {
            if (context == null)
                context = EmptyHashtable;
            int i = 0;
            if (UseRegexImplementation) {
                while (ReplaceMarkers(context, ref expression) > 0 && Recursive && i < MaxRecursiveSteps)
                    i++;
            } else {
                /*
                int exprCount;
                StringBuilder expr;
                do {
                    int idx = 0;                    
                    exprCount = EvaluateInternalVersion(context, expression, ref idx, out expr);
                    expression = expr.Replace("\\}", "}").ToString();                    
                    i++;
                } while (Recursive && exprCount > 0 && i < MaxRecursiveSteps);                
                */
                bool exprFound;
                StringBuilder expr = new StringBuilder(expression);
                do {
                    int idx = 0;
                    i++;
                    exprFound = EvaluateInternal(context, ref expr, ref idx);
                    expr.Replace("\\}", "}");
                } while (Recursive && exprFound && i < MaxRecursiveSteps);
                expression = expr.ToString();
            }
            if (i >= MaxRecursiveSteps)
                throw new Exception(
                    String.Format("Cannot evaluate expression: too many recursive steps (max {0} allowed)", MaxRecursiveSteps));
            return expression;
        }

		IExpressionResolver GetExprResolverByMarker(string marker) {
			IExpressionResolver result = null;
            if (ResolversHashtable != null) {
                ResolversHashtable.TryGetValue(marker, out result);
            } else {
                foreach (IExpressionDescriptor exprDescriptor in ExprDescriptors) {
                    if (exprDescriptor.Marker == marker) {
                        result = exprDescriptor.ExprResolver;
                        break;
                    }
                }
            }
			return result;
		}

        bool EvaluateInternal(IDictionary context, ref StringBuilder expression, ref int idx) {
            bool exprFound = false;
            int startIdx = idx, markerIdx = -1, len = expression.Length;
            while (idx < len) {
                char c = expression[idx++];
                if (markerIdx != -1) {
                    if (c >= 'a' && c <= 'z' || c >= '0' && c <= '9' || c == '-' || c >= 'A' && c <= 'Z' || c == '#' || c=='_') {
                        continue;
                    } else if (c == ':') {
                        string marker = expression.ToString(markerIdx, idx - markerIdx - 1);
                        IExpressionResolver properExprResolver = GetExprResolverByMarker(marker);
                        if (properExprResolver != null) {
                            int exprIdx = idx;
                            bool rc = EvaluateInternal(context, ref expression, ref idx);
                            len = expression.Length;
                            if (!rc) continue;
                            expression.Replace(@"\}", "}", exprIdx, idx - 1 - exprIdx);
                            idx += expression.Length - len;
                            len = expression.Length;
                            string evalResult = ConvertToString(properExprResolver.Evaluate(context, expression.ToString(exprIdx, idx - 1 - exprIdx)));
                            expression.Remove(markerIdx - 1, idx - markerIdx + 1).Insert(markerIdx - 1, evalResult).Replace("}", @"\}", markerIdx - 1, evalResult.Length);
                            idx += expression.Length - len;
                            len = expression.Length;
                            exprFound = true;
                            markerIdx = -1;
                            continue;
                        }
                    }
                    markerIdx = -1;
                }
                if (c == '{') markerIdx = idx;
                else if (c == '\\' && idx < len && expression[idx] == '}') idx++;
                else if (c == '}' && startIdx != 0) return true;
            }
            return startIdx == 0 ? exprFound : false;
        }

        #region version

        int EvaluateInternalVersion(IDictionary context, string expression, ref int idx, out StringBuilder result) {
            int startIdx = idx;
            int exprCount = 0;
            bool markerMode = false;
            result = new StringBuilder();
            StringBuilder marker = null;
            while (idx < expression.Length) {
                char c = expression[idx++];
                if (markerMode) {
                    if (c >= 'a' && c <= 'z' || c >= '0' && c <= '9' || c >= 'A' && c <= 'Z' || c == '#' || c == '-') {
                        marker.Append(c);
                        continue;
                    } else if (c == ':') {
                        markerMode = false;
                        IExpressionResolver properExprResolver = null;
                        string markerStr = marker.ToString();
                        if (ResolversHashtable != null) {
                            ResolversHashtable.TryGetValue(markerStr, out properExprResolver);
                        } else {
                            foreach (IExpressionDescriptor exprDescriptor in ExprDescriptors) {
                                if (exprDescriptor.Marker == markerStr) {
                                    properExprResolver = exprDescriptor.ExprResolver;
                                    break;
                                }
                            }
                        }
                        if (properExprResolver != null) {
                            StringBuilder expr;
                            if (EvaluateInternalVersion(context, expression, ref idx, out expr) == -1) {
                                result.Append('{').Append(marker).Append(':').Append(expr);
                                continue;
                            }
                            expr.Replace(@"\}", "}");
                            result.Append(ConvertToString(properExprResolver.Evaluate(context, expr.ToString())).Replace("}", @"\}"));
                            exprCount++;
                            continue;
                        } else {
                            result.Append('{').Append(marker);
                        }
                    } else {
                        markerMode = false;
                        result.Append('{').Append(marker);
                    }
                }
                if (c == '{') {
                    markerMode = true;
                    if (marker == null)
                        marker = new StringBuilder();
                    else
                        marker.Length = 0;
                } else if (c == '\\' && idx < expression.Length && expression[idx] == '}') {
                    result.Append("\\}");
                    idx++;
                } else if (c == '}' && startIdx != 0) {
                    return 1;
                } else {
                    result.Append(c);
                }
            }
            if (markerMode) {
                result.Append('{').Append(marker);
            }
            if (startIdx != 0) {
                return -1;
            }
            return exprCount;
        }

        #endregion

        #region RegexImplementation
        
        protected virtual int ReplaceMarkers(IDictionary context, ref string expression) {
            int matchesCount = 0;
            foreach (IExpressionDescriptor exprDescriptor in ExprDescriptors)
                matchesCount += ProcessDescriptor(exprDescriptor, context, ref expression);

            // specially process '}' symbol
            expression = expression.Replace("\\}", "}");

            return matchesCount;
        }

        protected virtual int ProcessDescriptor(IExpressionDescriptor exprDescriptor, IDictionary context, ref string expression) {
            string exprRegex = ExprRegexBegin + exprDescriptor.Marker + ExprRegexEnd;
            if (DescriptorRegexCache[exprRegex] == null)
                DescriptorRegexCache[exprRegex] = new Regex(exprRegex, RegexOptions.Multiline);
            Regex markerRegex = DescriptorRegexCache[exprRegex] as Regex;

            MatchCollection matches = markerRegex.Matches(expression);

            int indexShift = 0;
            foreach (Match m in matches) {
                string expr = m.Groups["expr"].Value.Replace("\\}", "}");
                string exprValue = ConvertToString(exprDescriptor.ExprResolver.Evaluate(context, expr));
                // specially process '}' symbol
                exprValue = exprValue.Replace("}", "\\}");

                expression = expression.Remove(m.Index + indexShift, m.Length).Insert(m.Index + indexShift, exprValue);
                indexShift += exprValue.Length - m.Length;
            }

            return matches.Count;
        }

        #endregion

        static string ConvertToString(object o) {
            IList oList = o as IList;
            if (oList != null) {
                // format as array
                string[] strArray = new string[oList.Count];
                bool commaDetected = false;
                bool altCommaDetected = false;
                for (int i = 0; i < strArray.Length; i++) {
					strArray[i] = Convert.ToString(oList[i]);
                    if (strArray[i].IndexOf(',') >= 0)
                        commaDetected = true;
					if (strArray[i].IndexOf(';') >= 0)
						altCommaDetected = true;
				}
				char separator = commaDetected ? (altCommaDetected ? '\0' : ';') : ',';
				return String.Join( separator.ToString(), strArray);
            }

            return Convert.ToString(o);
        }

    }
}
