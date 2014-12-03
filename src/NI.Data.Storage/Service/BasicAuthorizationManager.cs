#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013-2014 Vitalii Fedorchenko
 * Copyright 2014 NewtonIdeas
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
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Net;
using System.Threading;
using System.Security.Principal;
using System.IdentityModel.Policy;

using System.Web.Security;
using NI.Data.Storage.Service;

namespace NI.Data.Storage.Service {
	
	public class BasicAuthorizationManager : ServiceAuthorizationManager, IAuthorizationPolicy {

		static Logger log = new Logger(typeof(BasicAuthorizationManager));

		public BasicAuthorizationManager() {
			
		}

		protected override bool CheckAccessCore(OperationContext operationContext) {
			var authHeader = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
			if (!String.IsNullOrEmpty(authHeader)) {
				var svcCredentials = System.Text.ASCIIEncoding.ASCII
						.GetString(Convert.FromBase64String(authHeader.Substring(6)))
						.Split(':');
				var user = new { Name = svcCredentials[0], Password = svcCredentials[1] };
				if (!String.IsNullOrEmpty(user.Name) && !String.IsNullOrEmpty(user.Password) ) {
					if (Membership.ValidateUser(user.Name, user.Password)) {
						log.Info("Storage API request authenticated for user={0}", user.Name);
						return true;
					}
				}

				throw new ApiException("Invalid username or password", HttpStatusCode.Unauthorized);
			} else {
				WebOperationContext.Current.OutgoingResponse.Headers.Add("WWW-Authenticate: Basic realm=\"StorageService\"");
				throw new ApiException("Please provide a username and password", HttpStatusCode.Unauthorized);
			}
		}


		public bool Evaluate(EvaluationContext evaluationContext, ref object state) {
			var authHeader = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
			if (!String.IsNullOrEmpty(authHeader)) {
				var svcCredentials = System.Text.ASCIIEncoding.ASCII
										.GetString(Convert.FromBase64String(authHeader.Substring(6)))
										.Split(':');
				evaluationContext.Properties["Principal"] = new GenericPrincipal(
						new GenericIdentity(svcCredentials[0]), new string[0]);
				return true;
			}
			return false;
		}

		public System.IdentityModel.Claims.ClaimSet Issuer {
			get { return System.IdentityModel.Claims.ClaimSet.System; }
		}

		public string Id {
			get { return this.GetType().ToString(); }
		}
	}
}
