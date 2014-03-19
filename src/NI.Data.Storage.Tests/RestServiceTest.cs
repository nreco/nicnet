#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013 Vitalii Fedorchenko
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
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Net;
using System.IO;

using NUnit.Framework;
using NI.Data.Storage.Model;
using NI.Data.Storage.Service;

namespace NI.Data.Storage.Tests {
	
	[TestFixture]
	public class RestStorageServiceTest {

		DataSetStorageContext objPersisterContext;
		WebServiceHost serviceHost;
		ChannelFactory<IStorageService> storageServiceUnderTestProxyFactory;

		[TestFixtureSetUp]
		public void StartStorageService() {
			var objectPersisterTest = new ObjectContainerDalcStorageTest();

			var testSchema = DataSetStorageContext.CreateTestSchema();
			objPersisterContext = new DataSetStorageContext(() => { return testSchema; });

			var storageService = new StorageService(objPersisterContext.ObjectContainerStorage, () => { return testSchema; });
			serviceHost = new WebServiceHost(storageService, new[] { new Uri("http://localhost:8005") });

			serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
			serviceHost.Description.Behaviors.Find<ServiceBehaviorAttribute>().InstanceContextMode = InstanceContextMode.Single;

			serviceHost.Open();
		}

		[TestFixtureTearDown]
		public void StopStorageService() {
			serviceHost.Close();			
		}

		public string GetUrl(string url, string method = "GET", string postData = null) {
			var webReq = WebRequest.Create(url);
			webReq.Method = method;
			webReq.ContentType = "application/xml";
			if (postData!=null) {
				using (var reqStream = webReq.GetRequestStream()) {
					var wr = new StreamWriter(reqStream);
					wr.Write(postData);
					wr.Flush();
				}
			}
			WebResponse webResponse;
			try {
				webResponse = webReq.GetResponse();
			} catch (WebException ex) {
				var stream = ex.Response.GetResponseStream();
				var res = new StreamReader(stream).ReadToEnd();
				Console.WriteLine(res);
				throw ex;
			}
			try {
				var stream = webResponse.GetResponseStream();
				var res = new StreamReader(stream).ReadToEnd();
				return res;
			} finally {
				webResponse.Close();
			}
		}

		[Test]
		public void StorageService_Test() {
			var baseUrl = "http://localhost:8005/";

			var ontologyRes = GetUrl(baseUrl+"dataschema");

			Console.WriteLine(ontologyRes);

		}


	}
}
