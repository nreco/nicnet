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

using NI.Common.Providers;

namespace NI.Data
{
	/// <summary>
	/// DataSetProvider provides get DataSet provided by underlying object provider
	/// </summary>
	public class DataSetProvider : IDataSetProvider, IObjectProvider
	{
		protected IObjectProvider _UnderlyingObjectProvider;

		public IObjectProvider UnderlyingObjectProvider
		{
			get { return _UnderlyingObjectProvider; }
			set { _UnderlyingObjectProvider = value; }
		}

		public DataSet GetDataSet(object context)
		{
			return (DataSet)UnderlyingObjectProvider.GetObject(context);
		}

		public object GetObject(object context) {
			return GetDataSet(context);
		}
	}
}
