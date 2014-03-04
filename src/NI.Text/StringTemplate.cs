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

namespace NI.Text {
	
	/// <summary>
	/// Simple string template processor 
	/// </summary>
    public class StringTemplate {

        const int MaxRecursiveSteps = 50;
        bool _Recursive = false;

        /// <summary>
        /// Get or set known expression markers with appropriate functions
        /// </summary>
		public IDictionary<string, Func<IDictionary, string, string>> Markers {
			get; set;
		}

        /// <summary>
        /// Get or set flag that indicates whether markers should be processed recursively
        /// </summary>
        public bool Recursive {
            get { return _Recursive; }
            set { _Recursive = value; }
        }

        static Hashtable _EmptyHashtable;
        static Hashtable EmptyHashtable {
            get {
                return (_EmptyHashtable??(_EmptyHashtable=new Hashtable()));
            }
        }

		public StringTemplate() {
			Markers = new Dictionary<string, Func<IDictionary, string, string>>() {
				{"var", EvalHelper.Variable },
				{"xml", EvalHelper.XmlEncode},
				{"xml-attr", EvalHelper.XmlAttributeEncode},
				{"is-in-role", EvalHelper.IsInRole},
				{"databind", (context,s) => { return Convert.ToString(new DataBind().Eval(context,s)); } }
			};
		}

		public StringTemplate(IDictionary<string, Func<IDictionary, string, string>> markers) {
			Markers = markers;
		}

        public string Eval(IDictionary context, string expression) {
            if (context == null)
                context = EmptyHashtable;
            int i = 0;
            bool exprFound;
            StringBuilder expr = new StringBuilder(expression);
            do {
                int idx = 0;
                i++;
                exprFound = EvaluateInternal(context, ref expr, ref idx);
                expr.Replace("\\}", "}");
            } while (Recursive && exprFound && i < MaxRecursiveSteps);
            expression = expr.ToString();
         
            if (i >= MaxRecursiveSteps)
                throw new Exception(
                    String.Format("Cannot evaluate expression: too many recursive steps (max {0})", MaxRecursiveSteps));
            return expression;
        }

		Func<IDictionary,string,object> GetEvaluator(string marker) {
			if (Markers == null || !Markers.ContainsKey(marker))
				return null;
			return Markers[marker];
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
						Func<IDictionary, string, object> properExprResolver = GetEvaluator(marker);
                        if (properExprResolver != null) {
                            int exprIdx = idx;
                            bool rc = EvaluateInternal(context, ref expression, ref idx);
                            len = expression.Length;
                            if (!rc) continue;
                            expression.Replace(@"\}", "}", exprIdx, idx - 1 - exprIdx);
                            idx += expression.Length - len;
                            len = expression.Length;
                            string evalResult = ConvertToString(properExprResolver(context, expression.ToString(exprIdx, idx - 1 - exprIdx)));
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
