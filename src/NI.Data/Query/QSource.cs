#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
 * Copyright 2008-2013 Vitalii Fedorchenko (changes and v.2)
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
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace NI.Data
{
	/// <summary>
	/// Query source name structure
	/// </summary>
	[Serializable]
	public class QSource
	{

		public string Name { get; private set; }

		public string Alias { get; private set; }

		public QSource(string sourceName) {
			int dotIdx = sourceName.LastIndexOf('.'); // allow dot in table name (alias for this case is required), like dbo.users.u
			if (dotIdx >= 0) {
				Name = sourceName.Substring(0, dotIdx);
				Alias = sourceName.Substring(dotIdx+1);
			}
			else {
				Name = sourceName;
				Alias = null;
			}

		}

		public QSource(string sourceName, string alias) {
			Name = sourceName;
			Alias = alias;
		}
		
		public override string ToString() {
			return String.IsNullOrEmpty(Alias) ? Name : Name+"."+Alias;
		}

		public static implicit operator QSource(string value) {
			return new QSource(value);
		}
		public static implicit operator string(QSource value) {
			return value.ToString();
		}
		
	}
}
