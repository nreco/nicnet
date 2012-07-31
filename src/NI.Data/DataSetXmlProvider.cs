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

using System.Data;

using NI.Common;
using NI.Common.Providers;



namespace NI.Data
{
	/// <summary>
	/// DataSetXmlProvider provides XML representation of the data stored in the System.Data.DataSet
	/// provided by underlying NI.Data.IDataSetProvider
	/// </summary>
	public class DataSetXmlProvider : IObjectProvider, IStringProvider
	{
		#region Dependencies

		protected IDataSetProvider _UnderlyingDataSetProvider;
		public IDataSetProvider UnderlyingDataSetProvider
		{
			get { return _UnderlyingDataSetProvider; }
			set { _UnderlyingDataSetProvider = value; }
		}



		protected string _DataSetName;
		/// <summary>
		/// If DataSetName is set, it overrides the name of the dataset provided by UnderlyingDataSetProvider
		/// </summary>
		public string DataSetName
		{
			get { return _DataSetName; }
			set { _DataSetName = value; }
		}

		#endregion



		public string GetString(object context)
		{
			DataSet data = UnderlyingDataSetProvider.GetDataSet(context);

			if (DataSetName != null)
				data.DataSetName = DataSetName;

			return data.GetXml();
		}



		public object GetObject(object context)
		{
			return GetString(context);
		}
	}
}
