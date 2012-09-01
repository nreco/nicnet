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
using NI.Common.Providers;
using NI.Common;

namespace NI.Data
{
	/// <summary>
	/// Simple 'proxy' query provider implementation.
	/// </summary>
	public class QueryProvider : IQueryProvider
	{
		IObjectProvider _UnderlyingObjectProvider;
		IQueryModifier _QueryModifier = null;

		/// <summary>
		/// Get or set underlying object provider
		/// </summary>
		public IObjectProvider UnderlyingObjectProvider {
			get { return _UnderlyingObjectProvider; }
			set { _UnderlyingObjectProvider = value; }
		}

		/// <summary>
		/// Get or set query modifier
		/// </summary>
		public IQueryModifier QueryModifier {
			get { return _QueryModifier; }
			set { _QueryModifier = value; }
		}
		
		public QueryProvider() { }

		public QueryProvider(IObjectProvider prv) {
			UnderlyingObjectProvider = prv;
		}

		public Query GetQuery(object context) {
			Query q = (Query)UnderlyingObjectProvider.GetObject(context);
			if (QueryModifier!=null)
				q = QueryModifier.Modify(q);
			return q;
		}

	}
}
