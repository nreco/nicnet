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
using System.Collections.Specialized;

namespace NI.Data.Dalc.Permissions
{
	/// <summary>
	/// Holds DALC record field identification.
	/// </summary>
	public class DalcRecordFieldInfo : DalcRecordInfo
	{
		string _FieldName;

		/// <summary>
		/// Field name
		/// </summary>
		public string FieldName {
			get { return _FieldName; }
		}

		public DalcRecordFieldInfo(string sourceName, string fieldName, IDictionary uidFields) :
			this(sourceName, fieldName, uidFields, null ) { }

		public DalcRecordFieldInfo(string sourceName, string fieldName, IDictionary uidFields, IDictionary fields) :
			base(sourceName, uidFields, fields) {
			_FieldName = fieldName;
		}
	}
}
