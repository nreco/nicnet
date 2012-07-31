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
using System.Collections;
using NI.Common.Providers;
using NI.Common.Collections;

namespace NI.Data
{
	/// <summary>
	/// DalcDictionaryListProvider.
	/// </summary>
	public class DalcDictionaryListProvider:DalcObjectListProvider,IDictionaryListProvider
	{
		public DalcDictionaryListProvider() {
			// this class supports that optimization
			UseDataReader = true;
		}

		public IDictionary[] GetDictionaryList(object context) {
			IList objects = GetObjectList(context);
			IDictionary[] result = new IDictionary[objects.Count];
			for (int i=0;i <result.Length; i++)
				result[i] = objects[i] as IDictionary;
			return result;
		}

		protected override object PrepareObject(System.Data.DataRow row, string[] fields) {
			return new DataRowDictionary(row);
		}
		
		protected override object PrepareObject(IDictionary row, string[] fields) {
			if (row.Contains(0))
				row.Remove(0); // remove 'virtual' first column value
			return row;
		}

	}
}
