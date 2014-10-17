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
using System.Xml;
using System.Xml.XPath;
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
		StorageDalc storageDalc;
		string baseUrl = "http://localhost:8005/";

		[TestFixtureSetUp]
		public void StartStorageService() {
			var testSchema = DataSetStorageContext.CreateTestSchema();
			Func<DataSchema> getTestSchema = () => { return testSchema; };
			objPersisterContext = new DataSetStorageContext(getTestSchema);
			storageDalc = new StorageDalc(
				objPersisterContext.StorageDbMgr.Dalc, 
				objPersisterContext.ObjectContainerStorage, 
				getTestSchema);

			DataSetStorageContext.AddTestData(testSchema, objPersisterContext.ObjectContainerStorage);

			var storageService = new StorageService(
					objPersisterContext.ObjectContainerStorage, 
					storageDalc,
					getTestSchema);
			serviceHost = new WebServiceHost(storageService, new[] { new Uri(baseUrl) });
			
			serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
			serviceHost.Description.Behaviors.Find<ServiceBehaviorAttribute>().InstanceContextMode = InstanceContextMode.Single;

			serviceHost.Open();

			var endPoint = (WebHttpEndpoint)serviceHost.Description.Endpoints.First();
			endPoint.AutomaticFormatSelectionEnabled = true;
			((WebHttpBehavior)endPoint.EndpointBehaviors.First()).AutomaticFormatSelectionEnabled = true;
			((WebHttpBehavior)endPoint.EndpointBehaviors.First()).DefaultOutgoingResponseFormat = WebMessageFormat.Json;


		}

		[TestFixtureTearDown]
		public void StopStorageService() {
			serviceHost.Close();			
		}

		public string GetUrl(string url, string method = "GET", string postData = null) {
			var webReq = WebRequest.Create(url) as HttpWebRequest;
			webReq.Method = method;
			webReq.ContentType = "text/json";// "application/xml";
			webReq.Accept = "application/xml"; //"application/json";
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
				Console.WriteLine("ERROR: {0}", ex);
				if (ex.Response!=null) {
					var stream = ex.Response.GetResponseStream();
					if (stream!=null) {
						var res = new StreamReader(stream).ReadToEnd();
						Console.WriteLine("RESPONSE OUTPUT:\n", res);
					}
				}
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

		protected XPathDocument LoadXPathDoc(string res) {
			var doc = new XPathDocument( new StringReader(res) );
			return doc;
		}

		protected XmlNamespaceManager GetNsManager(XPathNavigator nav) {
			var xmlNsMgr = new XmlNamespaceManager(nav.NameTable);
			xmlNsMgr.AddNamespace("s", "http://schemas.datacontract.org/2004/07/NI.Data.Storage.Service.Schema");
			xmlNsMgr.AddNamespace("a", "http://schemas.microsoft.com/2003/10/Serialization/Arrays");
			return xmlNsMgr;
		}

		[Test]
		public void DataSchema() {
			var ontologyRes = GetUrl(baseUrl+"dataschema");
			Console.WriteLine(ontologyRes);

		}

		[Test]
		public void LoadRows() {

			var contactsRelexRes = GetUrl(baseUrl + "load/rows?q=contacts[*;id]");

			Console.WriteLine(contactsRelexRes);

			var contactsResXmlDoc = LoadXPathDoc(contactsRelexRes);
			var contactsResNav = contactsResXmlDoc.CreateNavigator();
			var contactsResNsMgr = GetNsManager(contactsResNav);

			var contactsResRows = contactsResNav.Select("/s:rowsResult/s:data/s:row", contactsResNsMgr);
			Assert.AreEqual(3, contactsResRows.Count);
			var contactNames = new[] {"John","Mary","Bob"};
			var contactIdx = 0;
			foreach (XPathNavigator contact in contactsResRows) {
				Assert.AreEqual( contactNames[contactIdx++], contact.SelectSingleNode("name", contactsResNsMgr).Value );
			}

		}

		[Test]
		public void LoadValues() {
			var contactsRelexRes = GetUrl(baseUrl + "load/values?q=contacts[*;id]");

			Console.WriteLine(contactsRelexRes);

			var contactsResXmlDoc = LoadXPathDoc(contactsRelexRes);
			var contactsResNav = contactsResXmlDoc.CreateNavigator();
			var contactsResNsMgr = GetNsManager(contactsResNav);

			var contactsResValueArrays = contactsResNav.Select("/s:valuesResult/s:data/a:ArrayOfanyType/a:anyType", contactsResNsMgr);
			Assert.AreEqual(3, contactsResValueArrays.Count);
		}


	}
}
