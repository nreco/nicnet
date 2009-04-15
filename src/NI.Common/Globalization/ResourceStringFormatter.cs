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

using NI.Common.Providers;

namespace NI.Common.Globalization {
	
	public class ResourceStringFormatter {
		IStringProvider _PrefixProvider;
		IResourceProvider _ResourceProvider;
		string _PlaceId = null;
		
		public IResourceProvider ResourceProvider {
			get { return _ResourceProvider; }
			set { _ResourceProvider = value; }
		}
		
		public IStringProvider PrefixProvider {
			get { return _PrefixProvider; }
			set { _PrefixProvider = value; }
		}
		
		public string PlaceId {
			get { return _PlaceId; }
			set { _PlaceId = value; }
		}
		
		public ResourceStringFormatter() {
			
		}
		public ResourceStringFormatter(IResourceProvider resourceProvider, IStringProvider prefixProvider) {
			ResourceProvider = resourceProvider;
			PrefixProvider = prefixProvider;
		}
		public ResourceStringFormatter(IResourceProvider resourceProvider, IStringProvider prefixProvider,string placeId) :
			this(resourceProvider,prefixProvider) {
			PlaceId = placeId;
		}

		public string Format(string resourceIdFormat, params object[] args) {
			string origResourceIdFormat = resourceIdFormat;
			// TODO: this is simplest variant that may incorrectly work with complex format strings!
			if (PrefixProvider != null) {
				for (int i = 0; i < args.Length; i++) {
					string paramStr = "{" + i.ToString();
					if (resourceIdFormat.IndexOf(paramStr) >= 0) {
						string prefix = PrefixProvider.GetString(args[i]);
						if (prefix != null)
							resourceIdFormat = resourceIdFormat.Replace(paramStr, prefix + paramStr);
					}
				}
			}
			string resolvedResourceFormat = ResourceProvider!=null ?
				ResourceProvider.GetResource(resourceIdFormat, PlaceId) as string : resourceIdFormat;
			string fmtStr = resolvedResourceFormat != resourceIdFormat ? resolvedResourceFormat : origResourceIdFormat;

			// also take case about 'masked' params, like {!0} (means that prefix should not be applied)
			for (int i = 0; i < args.Length; i++) {
				string paramStr = "{" + i.ToString();
				string maskedParamStr = "{!" + i.ToString();
				if (fmtStr.IndexOf(maskedParamStr) >= 0)
					fmtStr = fmtStr.Replace(maskedParamStr, paramStr);
			}

			return String.Format(fmtStr, args);
		}
		
	}
}
