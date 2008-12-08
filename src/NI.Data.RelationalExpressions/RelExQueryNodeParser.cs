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
using System.Collections.Specialized;

using NI.Data.Dalc;
using NI.Common;
using NI.Common.Expressions;
using NI.Common.Providers;

namespace NI.Data.RelationalExpressions
{
	/// <summary>
	/// Relex-based query node provider.
	/// </summary>
	public class RelExQueryNodeParser : IRelExQueryNodeParser
	{
		IRelExQueryParser _RelExQueryParser = new RelExQueryParser(false);
		
		/// <summary>
		/// Get or set relational expression parser used to build query node
		/// </summary>
		[Dependency(Required=false)]
		public IRelExQueryParser RelExQueryParser {
			get { return _RelExQueryParser; }
			set { _RelExQueryParser = value; }
		}
		
		public RelExQueryNodeParser()
		{
		}
		

		public IQueryNode Parse(string relExCondition) {
			string relEx = String.Format("sourcename({0})[*]", relExCondition);
			IQuery q = RelExQueryParser.Parse(relEx);
			return q.Root;
		}
	}
}
