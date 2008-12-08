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

using NI.Common;

namespace NI.Data.Dalc
{
	/// <summary>
	/// Chain-based IQueryFieldValueFormatter implementation.
	/// </summary>
	public class ChainQueryFieldValueFormatter : IQueryFieldValueFormatter
	{
		IQueryFieldValueFormatter[] _Formatters;
		
		[Dependency]
		public IQueryFieldValueFormatter[] Formatters {
			get { return _Formatters; }
			set { _Formatters = value; }
		}

		public ChainQueryFieldValueFormatter() { }
	
		public ChainQueryFieldValueFormatter(params IQueryFieldValueFormatter[] formatters) {
			Formatters = formatters;
		}

		public string Format(IQueryFieldValue fieldValue) {
			IQueryFieldValue currentValue = fieldValue;
			for (int i=0; i<Formatters.Length; i++)
				currentValue = new QField(Formatters[i].Format(currentValue));
			return currentValue.Name;
		}

	}
}