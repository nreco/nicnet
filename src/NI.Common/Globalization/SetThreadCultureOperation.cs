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
using System.Diagnostics;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Web;

using NI.Common.Operations;

namespace NI.Common.Globalization {
	
	/// <summary>
	/// Base class for setting thread culture operation.
	/// </summary>
	/// <remarks>Implements setting logic, including .NET 2.0 ru-RU locale bugfix.</remarks>
	public abstract class SetThreadCultureOperation : IOperation {
		bool _OnlyUICulture = false;
		
		public bool OnlyUICulture {
			get { return _OnlyUICulture; }
			set { _OnlyUICulture = value; }
		}
		
		public abstract void Execute(IDictionary context);
		
		protected void SetCulture(string cultureName) {
			try {
				CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);
				// apply net20 bugfix related to MonthDayPattern for ru-RU culture
				if (culture.TwoLetterISOLanguageName == "ru" &&
					culture.DateTimeFormat.MonthDayPattern == "MMMM dd") {
						culture = (CultureInfo)culture.Clone(); // obtain writable copy
						//DateTimeFormatInfo dtFormatCopy = (DateTimeFormatInfo)culture.DateTimeFormat.Clone();
						//dtFormatCopy.MonthDayPattern =
						culture.DateTimeFormat.MonthDayPattern = "dd MMMM";
					}

				Thread.CurrentThread.CurrentUICulture = culture;
				if (!OnlyUICulture)
					Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
			} catch (Exception ex) {
				Trace.WriteLine("Applying account-specific language failed: " + ex.Message);
			}
		}
		
	}
	
}
