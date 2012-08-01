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

namespace NI.Data.Permissions
{
	/// <summary>
	/// Holds DALC record identification
	/// </summary>
	public class DalcRecordInfo
	{
		string _SourceName; 
		IDictionary _UidFields;
		IDictionary _Fields = null;
		
		/// <summary>
		/// Record source name 
		/// </summary>
		public string SourceName { 
			get { return _SourceName; }
		}
		
		/// <summary>
		/// Fieldname-to-value map that forms record UID
		/// </summary>
		public IDictionary UidFields {
			get { return _UidFields; }
		}

		/// <summary>
		/// Get or set record fieldname-to-value map (optional, can be null)
		/// </summary>
		public IDictionary Fields {
			get { return _Fields; }
			set { _Fields = value; }
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="DalcRecordInfo"/> class
		/// with basic record identification info.
		/// </summary>
		/// <param name="sourceName">record source name</param>
		/// <param name="uidFields">fieldname-to-value map that forms record UID</param>
		public DalcRecordInfo(string sourceName, IDictionary uidFields) :
			this(sourceName, uidFields, null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="DalcRecordInfo"/> class
		/// with full record identification info.
		/// </summary>
		/// <param name="sourceName">record source name</param>
		/// <param name="uidFields">fieldname-to-value map that forms record UID</param>
		/// <param name="fields">record fieldname-to-value map</param>
		public DalcRecordInfo(string sourceName, IDictionary uidFields, IDictionary fields)
		{
			_SourceName = sourceName;
			_UidFields = uidFields;
			_Fields = fields;
		}
	}
}
