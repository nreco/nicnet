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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Runtime.Serialization;

namespace NI.Data.Storage.Service {
	
	public class ApiException : ApplicationException {
		
		public HttpStatusCode StatusCode { get; private set; }

		public string ErrorId { get; private set; }

		public ApiException(string msg, HttpStatusCode code) : this( msg, code, null) { }

		public ApiException(string msg, HttpStatusCode code, string errorCode) : this( msg, code, errorCode, null ) {
		}

		public ApiException(string msg, HttpStatusCode code, string errorCode, Exception inner) : base( msg, inner ) {
			StatusCode = code;
			ErrorId = errorCode;
		}
	}

	[DataContract(Name = "error")]
	public class ApiFault {

		[DataMember]
		public string Message { get; set; }

		public ApiFault(string msg) {
			Message = msg;
		}

	}
}
