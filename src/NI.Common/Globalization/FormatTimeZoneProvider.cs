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

using NI.Common.Providers;

namespace NI.Common.Globalization {
	
	/// <summary>
	/// Special date time formatter that can format specified timezone friendly name (EST,PST,etc)
	/// </summary>
	public class FormatTimeZoneProvider : IStringProvider, IObjectProvider {
		string _FormatString = null;
		IDateTimeProvider _DateTimeProvider = null;
		IStringProvider _TimeZoneNameProvider;
		
		public IStringProvider TimeZoneNameProvider {
			get { return _TimeZoneNameProvider; }
			set { _TimeZoneNameProvider = value; }
		}
		
		public string FormatString {
			get { return _FormatString; }
			set { _FormatString = value; }
		}

		public IDateTimeProvider DateTimeProvider {
			get { return _DateTimeProvider; }
			set { _DateTimeProvider = value; }
		}

		public string GetString(object context) {
			DateTime dateTime = DateTimeProvider.GetDateTime(context);
			string timeZoneName = TimeZoneNameProvider.GetString(context);
			string fmt = FormatString.Replace("zzz", "@@@@@@");
			string formattedStr = dateTime.ToString(fmt);
			if (timeZoneName!=null)
				formattedStr = formattedStr.Replace("@@@@@@", timeZoneName);
			else {
				formattedStr = formattedStr.Replace("@@@@@@", "GMT"+dateTime.ToString("zzz") );
			}
			return formattedStr;
		}

		public object GetObject(object context) {
			return GetString(context);
		}
	}
}
