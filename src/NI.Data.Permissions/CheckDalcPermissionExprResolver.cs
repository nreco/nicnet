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

using NI.Security.Permissions;

namespace NI.Data.Permissions {
	
	public class CheckDalcPermissionExprResolver : CheckPermissionExprResolver
	{
		string[] _UidFieldNames = new string[] {"id"};
		char _RecordInfoSeparator = '#';
		
		public char RecordInfoSeparator {
			get { return _RecordInfoSeparator; }
			set { _RecordInfoSeparator = value; }
		}
		
		public string[] UidFieldNames {
			get { return _UidFieldNames; }
			set { _UidFieldNames = value; }
		}
		
		protected override Permission ComposePermission(string input) {
			Permission permission = base.ComposePermission(input);
			DalcOperation dalcOp = (DalcOperation)Enum.Parse(typeof(DalcOperation), Convert.ToString(permission.Operation),true);
			
			string[] recordInfoParts = Convert.ToString(permission.Object).Split(RecordInfoSeparator);
			// record info should be sourcename + uidfields values
			if (recordInfoParts.Length!=(UidFieldNames.Length+1))
				throw new ArgumentException(
					String.Format("invalid object definition: should contain {0} parts separated by '{1}'", UidFieldNames.Length+1, RecordInfoSeparator ) );
			Hashtable uidInfo = new Hashtable();
			for (int i=0; i<UidFieldNames.Length; i++)
				uidInfo[UidFieldNames[i]] = recordInfoParts[i+1];
			
			DalcRecordInfo dalcRecordInfo = new DalcRecordInfo(recordInfoParts[0], uidInfo);
			return new DalcPermission(permission.Subject,dalcOp,dalcRecordInfo);
		}
	
	
	}
}
