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

using NI.Data;
using NI.Security.Permissions;

namespace NI.Data.Permissions
{
	/// <summary>
	/// DALC permission.
	/// </summary>
	public class DalcPermission : Permission
	{
		public new DalcRecordInfo Object {
			get { return base.Object as DalcRecordInfo; }
		}
		
		public new DalcOperation Operation {
			get { return (DalcOperation)Enum.Parse( typeof(DalcOperation), base.Operation.ToString(), true); }
		}
		
		public DalcPermission(object subject, DalcOperation operation, DalcRecordInfo recordId) :
			base(subject, operation, recordId) {
		}
		
	}
}
