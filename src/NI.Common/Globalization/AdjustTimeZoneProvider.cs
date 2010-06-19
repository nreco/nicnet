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
using System.Globalization;

using NI.Common.Providers;

namespace NI.Common.Globalization {
	
	/// <summary>
	/// Special provider that 'adjusts' datetime with respect to specific timezone
	/// </summary>
	/// <remarks>
	/// This provider useful for following cases:
	/// - when user specifies datetime in timezone that differs from local timezone
	/// - when some date should be treated in different from local timezone
	/// </remarks>
	public class AdjustTimeZoneProvider : IObjectProvider, IDateTimeProvider {
		decimal _TimeZoneOffset = 0;
		IDecimalProvider _TimeZoneOffsetProvider = null;
		IDateTimeProvider _DateTimeProvider = null;
		AdjustDirectionType _AdjustDirection = AdjustDirectionType.From;
		bool _AutoAdjustDaylightSavingDelta = false;

		public enum AdjustDirectionType {
			From = 1, To = 2
		}

		public bool AutoAdjustDaylightSavingDelta {
			get { return _AutoAdjustDaylightSavingDelta; }
			set { _AutoAdjustDaylightSavingDelta = value; }
		}
		
		public AdjustDirectionType AdjustDirection {
			get { return _AdjustDirection; }
			set { _AdjustDirection = value; }
		}
		
		public decimal TimeZoneOffset {
			get { return _TimeZoneOffset; }
			set { _TimeZoneOffset = value; }
		}
		
		public IDecimalProvider TimeZoneOffsetProvider {
			get { return _TimeZoneOffsetProvider; }
			set { _TimeZoneOffsetProvider = value; }
		}
		
		public IDateTimeProvider DateTimeProvider {
			get { return _DateTimeProvider; }
			set { _DateTimeProvider = value; }
		}
		
		public object GetObject(object context) {
			DateTime dt = GetDateTime(context);
			return dt == DateTime.MinValue ? null : (object)dt;
		}

		public DateTime GetDateTime(object context) {
			decimal timeZoneOffset = TimeZoneOffsetProvider!=null ? TimeZoneOffsetProvider.GetDecimal(context) : TimeZoneOffset;
			DateTime dateTime = DateTimeProvider!=null ? 
				DateTimeProvider.GetDateTime(context) : 
				(context is DateTime ? (DateTime)context : DateTime.MinValue);
			if (dateTime==DateTime.MinValue)
				return dateTime; // minvalue used by datetimeprovider for representing 'null' time
			
			decimal localZoneOffset = (decimal)TimeZone.CurrentTimeZone.GetUtcOffset(dateTime).TotalHours;
			decimal timeZoneDiff = timeZoneOffset-localZoneOffset; // calculate difference between desired timezone and local
			if (AutoAdjustDaylightSavingDelta) {
				var daylightChanges = TimeZone.CurrentTimeZone.GetDaylightChanges(dateTime.Year);
				if (TimeZone.CurrentTimeZone.IsDaylightSavingTime(dateTime))
					timeZoneDiff += (decimal)daylightChanges.Delta.TotalHours;
			}
			return dateTime.AddHours( 
				AdjustDirection==AdjustDirectionType.From ? - (double)timeZoneDiff : (double)timeZoneDiff); // apply difference
		}
	}
	
}
