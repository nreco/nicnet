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

namespace NI.Common.Caching {

	public class CompositeCacheEntryValidatorProvider : ICacheEntryValidatorProvider {

		private ICacheEntryValidatorProvider[] _ValidatorProviders;
		
		public ICacheEntryValidatorProvider[] ValidatorProviders {
			get { return _ValidatorProviders; }
			set { _ValidatorProviders = value; }
		}


		public CompositeCacheEntryValidatorProvider() {
			
		}

		public ICacheEntryValidator GetValidator(object context) {
			ArrayList validatorsList = new ArrayList();
			for (int i=0; i<ValidatorProviders.Length; i++) {
				ICacheEntryValidator validator = ValidatorProviders[i].GetValidator(context);
				if (validator!=null)
					validatorsList.Add(validator);
			}
			ICacheEntryValidator[] validators = validatorsList.ToArray(typeof(ICacheEntryValidator)) as ICacheEntryValidator[];
			return new CompositeCacheEntryValidator(validators);
		}
	}	
	
}
