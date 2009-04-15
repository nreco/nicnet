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

	public class CompositeCacheEntryValidator : ICacheEntryValidator {

		private ICacheEntryValidator[] _Validators;
		
		public ICacheEntryValidator[] Validators {
			get { return _Validators; }
			set { _Validators = value; }
		}


		public CompositeCacheEntryValidator() {
			
		}
		
		public CompositeCacheEntryValidator(params ICacheEntryValidator[] validators) {
			Validators = validators;
		}

		public bool IsValid {
			get {
				foreach (ICacheEntryValidator val in Validators) {
					if (val != null && !val.IsValid) {
						return false;
					}
				}
				return true;
			}
		}

	}	
	
}
