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
using System.Data;
using System.Data.Common;

namespace NI.Data.Dalc
{
	/// <summary>
	/// DbType enumeration member resolver.
	/// </summary>
	public class DbTypeResolver
	{
		bool _UseAnsiString = false;
		
		public bool UseAnsiString {
			get { return _UseAnsiString; }
			set { _UseAnsiString = value; }
		}
		
		public DbTypeResolver()
		{
		}
		
		/// <summary>
		/// Resolve DbType by System.Type
		/// </summary>
		/// <param name="type">.net type</param>
		/// <returns>DB type</returns>
		public virtual DbType Resolve(Type type) {
			if (type==typeof(byte) ) return DbType.Byte;
			if (type==typeof(bool) ) return DbType.Boolean;
			if (type==typeof(long) ) return DbType.Int64;
			if (type==typeof(int) ) return DbType.Int32;
			if (type==typeof(double) ) return DbType.Double;
			if (type==typeof(float) ) return DbType.Single;
			if (type==typeof(string) ) return UseAnsiString ? DbType.AnsiString : DbType.String;
			if (type==typeof(byte[]) ) return DbType.Binary;
			if (type==typeof(DateTime) ) return DbType.DateTime;
			if (type==typeof(Guid) ) return DbType.Guid;
			if (type==typeof(Decimal) ) return DbType.Decimal;
			return DbType.Object;
		}
		
		public virtual DbType Resolve(object value) {
			if (value!=null)
				return Resolve(value.GetType());
			return DbType.Object;
		}
		
	}
}
