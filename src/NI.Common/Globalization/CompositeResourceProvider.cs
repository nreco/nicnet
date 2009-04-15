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

using NI.Common;
using NI.Common.Providers;
using NI.Common.Expressions;

namespace NI.Common.Globalization {

	public class CompositeResourceProvider : IObjectProvider, IStringProvider, IResourceProvider, IExpressionResolver {
		
		IResourceProvider[] _ResourceProviders;
		
		public IResourceProvider[] ResourceProviders {
			get { return _ResourceProviders; }
			set { _ResourceProviders = value; }
		}
		
		public CompositeResourceProvider() {
			
		}

		public object Evaluate(IDictionary context, string expression) {
			return GetResource(expression);
		}		
		
		public object GetObject(object context) {
			return GetString(context);
		}

		public string GetString(object context) {
			if (!(context is string))
				throw new ArgumentException("context object should be string");
			return Convert.ToString(GetResource((string)context));
		}


		public object GetResource(string id) {
			for (int i=0; i<ResourceProviders.Length; i++) {
				object res = ResourceProviders[i].GetResource(id);
				if (res!=id) return res;
			}
			return id;
		}

		public object GetResource(string id, string placeId) {
			for (int i = 0; i < ResourceProviders.Length; i++) {
				object res = ResourceProviders[i].GetResource(id, placeId);
				if (res != id) return res;
			}
			return id;		
		}

		public object GetResource(string id, string placeId, System.Globalization.CultureInfo culture) {
			for (int i = 0; i < ResourceProviders.Length; i++) {
				object res = ResourceProviders[i].GetResource(id, placeId, culture);
				if (res != id) return res;
			}
			return id;			
		}

		public object GetResource(string id, System.Globalization.CultureInfo culture) {
			for (int i = 0; i < ResourceProviders.Length; i++) {
				object res = ResourceProviders[i].GetResource(id, culture);
				if (res != id) return res;
			}
			return id;			
		}

	}
	
	
}
