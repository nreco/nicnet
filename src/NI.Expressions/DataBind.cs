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
using System.Reflection;

namespace NI.Expressions {

	
	/// <summary>
	/// Evaluates object path expressions (inspired by "databinding" asp.net expressions)
	/// </summary>
	public class DataBind {

		static readonly char charDot='.';
		static readonly char charComma=',';
		static readonly char charOpenBracket='[';
		static readonly char charCloseBracket=']';
		static readonly char[] delimiters = new char[] { charOpenBracket, charCloseBracket, charDot, charComma};
		static readonly LexemType[] delimetersLexemType=new LexemType[] {
			LexemType.Delimiter|LexemType.DelimiterOpenBracket,
			LexemType.Delimiter|LexemType.DelimiterCloseBracket,
			LexemType.Delimiter|LexemType.DelimiterDot,
			LexemType.Delimiter|LexemType.DelimiterComma
		};
		static readonly char charQuote='"';

		[Flags]
		protected enum LexemType {
			Unknown = 0,
			String = 1,
			QuotedString = 2,
			Delimiter = 4,
			DelimiterDot = 8,
			DelimiterComma = 16,
			DelimiterOpenBracket = 32,
			DelimiterCloseBracket = 64,
			Stop = 128
		}		

		public object Eval(object container, string expression) {
			object currentObj = container;
			int startIdx = 0;
			int endIdx = 0;
			try {
				do {
					object evalResult = EvaluateObject(currentObj, expression, startIdx, out endIdx);
					startIdx = endIdx;
					currentObj = evalResult;
					if (endIdx>=(expression.Length-1))
						break;
				} while (true);
			} catch (Exception ex) {
				throw new Exception( String.Format( "Cannot evaluate '{0}': {1}", expression, ex.Message), ex);
			}
			return currentObj;
		}

		protected object EvaluateObject(object currentObj, string s, int startIdx, out int endIdx) {
			LexemType lexemType = GetLexemType(s, startIdx, out endIdx);
			// stop
			if (lexemType==LexemType.Stop) {
				endIdx = s.Length-1;
				return null;
			}

			startIdx=endIdx;
			// indexer
			if ((lexemType&LexemType.DelimiterOpenBracket)==LexemType.DelimiterOpenBracket) {
				ArrayList indexerParamsList = new ArrayList();
				do {
					// read arg
					lexemType = GetLexemType(s, startIdx, out endIdx);
					if ((lexemType&LexemType.String)==LexemType.String) {
						indexerParamsList.Add( GetLexem(lexemType, s, startIdx, endIdx) );
						startIdx = endIdx;
						lexemType=GetLexemType(s, startIdx, out endIdx);
					} else {
						OnEvalError("expected indexer argument", s, startIdx);
					}

					// read delim
					startIdx = endIdx;
					// param list is finished ?
					if ((lexemType&LexemType.DelimiterCloseBracket)==LexemType.DelimiterCloseBracket)
						break;
					if ((lexemType&LexemType.DelimiterComma)!=LexemType.DelimiterComma)
						OnEvalError("expected ']' or ','", s, startIdx);
				} while (true);
				try {
					return EvaluateObjectIndexer(currentObj, indexerParamsList);
				} catch (Exception ex) {
					OnEvalError("cannot evaluate indexer", s, startIdx);
				}
			}
			// property
			if ((lexemType&LexemType.DelimiterDot)==LexemType.DelimiterDot) {
				lexemType = GetLexemType(s, startIdx, out endIdx);
				// 'this' object ?
				if (lexemType==LexemType.Stop)
					return currentObj;
				// expected property name
				if (lexemType!=LexemType.String)
					OnEvalError("expected property name", s, startIdx);
				string propertyName = GetLexem(lexemType, s, startIdx, endIdx);
				startIdx = endIdx;
				return EvaluateObjectProperty(currentObj, propertyName);
			}

			OnEvalError("expected property or indexer", s, startIdx);
			return null;
		}

		protected object EvaluateObjectIndexer(object obj, IList indexerParams) {
			IndexerProxy idxProxy = new IndexerProxy(obj);
			object[] idxParamsArray = new object[indexerParams.Count];
			for (int i=0; i<idxParamsArray.Length; i++)
				idxParamsArray[i] = IsNumber(indexerParams[i].ToString()) ? (object)Convert.ToInt32(indexerParams[i]) : (object)indexerParams[i].ToString();
			return idxProxy[idxParamsArray];
		}

		protected object EvaluateObjectProperty(object obj, string propertyName) {
			if (obj==null)
				throw new NullReferenceException("Object is null");
			PropertyInfo pInfo = obj.GetType().GetProperty(propertyName);
			if (pInfo==null)
				throw new NullReferenceException(obj.GetType().FullName + " does not contain property "+propertyName);
			return pInfo.GetValue(obj, null);
		}

		protected bool IsNumber(string s) {
			for (int i=0; i<s.Length; i++)
				if (!Char.IsDigit(s[i]))
					return false;
			return true;
		}


		protected void OnEvalError(string msg, string s, int startIdx) {
			throw new Exception(
					String.Format("Syntax error: {0} (position: {1}, expression: {2}", msg, startIdx, s));
		}

		protected LexemType GetLexemType(string s, int startIdx, out int endIdx) {
			LexemType lexemType=LexemType.Unknown;
			endIdx=startIdx;
			while (endIdx<s.Length) {
				int delimIdx = Array.IndexOf(delimiters, s[endIdx]);
				
				if (delimIdx>=0) {
					if (lexemType==LexemType.Unknown) {
						endIdx++;
						return delimetersLexemType[delimIdx];
					}
					if (lexemType!=LexemType.QuotedString)
						return lexemType;
				} else if (Char.IsLetterOrDigit(s[endIdx])) {
					if (lexemType==LexemType.Unknown)
						lexemType=LexemType.String;
				} else if (s[endIdx]==charQuote) {
					if (lexemType==LexemType.Unknown)
						lexemType=LexemType.QuotedString|LexemType.String;
					else {
						if ((lexemType&LexemType.QuotedString)==LexemType.QuotedString) {
							// check for "" combination
							if (((endIdx+1)<s.Length && s[endIdx+1]!=charQuote)) {
								endIdx++;
								return lexemType;
							} else
								if ((endIdx+1)<s.Length) endIdx++; // skip next quote
						}
					}
				} else if (Char.IsControl(s[endIdx]) &&
							lexemType!=LexemType.Unknown &&
							(lexemType&LexemType.QuotedString)!=LexemType.QuotedString) return lexemType;

				// goto next char
				endIdx++;
			}

			if (lexemType==LexemType.Unknown) return LexemType.Stop;
			if ((lexemType&LexemType.QuotedString)==LexemType.QuotedString)
				OnEvalError("Unterminated constant", s, startIdx);
			return lexemType;
		}

		protected string GetLexem(LexemType lexemType, string s, int startIdx, int endIdx) {
			string lexem = s.Substring(startIdx, endIdx-startIdx).Trim();
			if ((lexemType&LexemType.QuotedString)==LexemType.QuotedString) {
				lexem = lexem.Substring(1, lexem.Length-2); 
				// replace "" with "
				lexem = lexem.Replace( "\"\"", "\"" );
			}
			return lexem;
		}




	}


}
