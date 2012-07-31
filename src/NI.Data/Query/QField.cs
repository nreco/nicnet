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

namespace NI.Data
{
	/// <summary>
	/// IQueryFieldValue implementation
	/// </summary>
	[Serializable]
	public struct QField : IQueryFieldValue
	{
		string _Name;
		
		public string Name {
			get { return _Name; } 
		}
	
		public QField(string field_name) {
			_Name = field_name;
		}
		
		public override bool Equals(object obj) {
			return base.Equals (obj);
		}
		
		public override int GetHashCode() {
			return base.GetHashCode ();
		}

		
		
		public static explicit operator QField(string fld) {
			return new QField(fld);
		}
		
		public static QueryConditionNode operator ==(QField lvalue, IQueryValue rvalue) {
			return new QueryConditionNode( lvalue, Conditions.Equal, rvalue );
		}		

		public static QueryConditionNode operator !=(QField lvalue, IQueryValue rvalue) {
			return new QueryConditionNode( lvalue, Conditions.Equal|Conditions.Not, rvalue );
		}		

		public static QueryConditionNode operator <(QField lvalue, IQueryValue rvalue) {
			return new QueryConditionNode( lvalue, Conditions.LessThan, rvalue );
		}		

		public static QueryConditionNode operator >(QField lvalue, IQueryValue rvalue) {
			return new QueryConditionNode( lvalue, Conditions.GreaterThan, rvalue );
		}		

		public static QueryConditionNode operator >=(QField lvalue, IQueryValue rvalue) {
			return new QueryConditionNode( lvalue, Conditions.GreaterThan|Conditions.Equal, rvalue );
		}		

		public static QueryConditionNode operator <=(QField lvalue, IQueryValue rvalue) {
			return new QueryConditionNode( lvalue, Conditions.LessThan|Conditions.Equal, rvalue );
		}		

		public static QueryConditionNode operator %(QField lvalue, IQueryValue rvalue) {
			return new QueryConditionNode( lvalue, Conditions.Like, rvalue );
		}		
		
		
	}
}
