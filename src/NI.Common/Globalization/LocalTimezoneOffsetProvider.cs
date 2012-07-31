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
using System.Text;
using NI.Common.Providers;

namespace NI.Common.Globalization {
	
	/// <summary>
	/// Local timezone offset (in hours) provider
	/// </summary>
	public class LocalTimezoneOffsetProvider : IObjectProvider, IDecimalProvider {

		public LocalTimezoneOffsetProvider() { }

		public object GetObject(object context) {
			return GetDecimal(context);
		}

		public decimal GetDecimal(object context) {
			return (decimal)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalHours;
		}
	}
	
}
