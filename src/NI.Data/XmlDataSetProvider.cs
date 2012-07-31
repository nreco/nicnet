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
using System.Data;
using System.IO;

using NI.Common.Providers;

namespace NI.Data
{
	/// <summary>
	/// XmlDataSetProvider provides DataSet object from xml representation
	/// provided by underlying NI.Common.Providers.IStringProvider
	/// </summary>
	public class XmlDataSetProvider : IDataSetProvider
	{
		protected IStringProvider _XmlStringProvider;

		public IStringProvider XmlStringProvider
		{
			get { return _XmlStringProvider; }
			set { _XmlStringProvider = value; }
		}
		
		public DataSet GetDataSet(object context)
		{
			DataSet ds = new DataSet();
			string xmlString = XmlStringProvider.GetString(context);
			ds.ReadXml(new StringReader(xmlString));
			return ds;
		}
	}
}
