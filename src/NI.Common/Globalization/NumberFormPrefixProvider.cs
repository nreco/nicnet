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
using System.Collections;
using System.Text;
using System.Threading;

using NI.Common.Providers;

namespace NI.Common.Globalization {
	
	public class NumberFormPrefixProvider : IStringProvider {

		string _DefaultLanguageName = null;
		NumberDescriptor[] _Descriptors;

		public string DefaultLanguageName {
			get { return _DefaultLanguageName; }
			set { _DefaultLanguageName = value; }
		}		
		
		public NumberDescriptor[] Descriptors {
			get { return _Descriptors; }
			set { _Descriptors = value; }
		}
		
		public string GetString(object context) {
			string langId = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
			long intPart = 0;
			if (context is int) {
				intPart = (int)context;
			} else if (context is Decimal) {
				intPart = Convert.ToInt64(  Math.Truncate( (Decimal)context ) );
			} else if (context is Double) {
				intPart = Convert.ToInt64(  Math.Truncate( (Double)context ) );
			} else {
				throw new Exception("Cannot provide suffix: available only for decimal,double or int");	
			}
			
			NumberDescriptor langNumberDescriptor = FindNumberDescriptor(langId);
			if (langNumberDescriptor!=null) {
				string prefix = langNumberDescriptor.GetPrefix(intPart);
				if (prefix!=null)
					return prefix;
			} else if (DefaultLanguageName!=null) {
				NumberDescriptor defaultLangNumberDescriptor = FindNumberDescriptor(DefaultLanguageName);
				if (defaultLangNumberDescriptor!=null) {
					string prefix = defaultLangNumberDescriptor.GetPrefix(intPart);
					if (prefix != null)
						return prefix;				
				}
			}
			
			return null;
		}
		
		protected NumberDescriptor FindNumberDescriptor(string langId) {
			for (int i=0; i<Descriptors.Length; i++)
				if (Descriptors[i].LanguageId==langId)
					return Descriptors[i];
			return null;
		}
		
		public abstract class NumberDescriptor {
			string _LanguageId;
			
			public string LanguageId {
				get { return _LanguageId; }
				set { _LanguageId = value; }
			}
			
			public abstract string GetPrefix(long integerPart);
		}

		public class NumberSingularPluralDescriptor : NumberDescriptor {

			string _PluralPrefix = "plural";
			string _SingularPrefix = "singular";
			string _ZeroPrefix = null;
			
			public string PluralPrefix {
				get { return _PluralPrefix; }
				set { _PluralPrefix = value; }
			}
			public string SingularPrefix {
				get { return _SingularPrefix; }
				set { _SingularPrefix = value; }
			}
			public string ZeroPrefix {
				get { return _ZeroPrefix; }
				set { _ZeroPrefix = value; }
			}
			
			public override string GetPrefix(long integerPart) {
				if (ZeroPrefix!=null && integerPart==0)
					return ZeroPrefix;
				if (integerPart==1) return SingularPrefix;
				return PluralPrefix;
			}
		}

		public class NumberCaseDescriptor : NumberDescriptor {
			string _DefaultCasePrefix = "default_case";
			NumberCasePattern[] _Patterns;
			
			public string DefaultCasePrefix {
				get { return _DefaultCasePrefix; }
				set { _DefaultCasePrefix = value; }
			}
			
			public NumberCasePattern[] Patterns {
				get { return _Patterns; }
				set { _Patterns = value; }
			}
			
			public override string GetPrefix(long integerPart) {
				string intPartStr = Convert.ToString(integerPart);
				for (int i = 0; i < Patterns.Length; i++)
					if (intPartStr.EndsWith(Patterns[i].Ending))
						return Patterns[i].CasePrefix;
				return DefaultCasePrefix;
			}
		}
		
		public class NumberCasePattern {
			string _Ending;
			string _CasePrefix;
			
			public string Ending {
				get { return _Ending; } 
				set { _Ending = value; }
			}
			
			public string CasePrefix {
				get { return _CasePrefix; }
				set { _CasePrefix = value; }
			}
			public NumberCasePattern() { }
			public NumberCasePattern(string ending, string casePrefix) {
				Ending = ending;
				CasePrefix = casePrefix;
			}
		}
		
		
	}
	
}
