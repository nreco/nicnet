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

namespace NI.Common.Providers
{
	/// <summary>
	/// Constant object provider.
	/// </summary>
	public class ConstObjectProvider : IObjectProvider, IStringProvider, IBooleanProvider, IDateTimeProvider
	{
		object _Constant = null;
        private Type _ConstantType = null;
		
		public object Constant {
			get { return _Constant; }
			set { _Constant = value; }
		}

        public Type ConstantType
        {
            get { return _ConstantType; }
            set { _ConstantType = value; }
        }
	
		public ConstObjectProvider() {
		}

		public ConstObjectProvider(object constant) {
			Constant = constant;
		}


		public object GetObject(object context) 
        {
            if (ConstantType == null)
            {
                return Constant;
            }
            else
            {
                if (ConstantType.IsEnum)
                {
                    return Enum.Parse(ConstantType, Convert.ToString(Constant));
                }
                else
                {
                    return Convert.ChangeType(Constant, ConstantType);
                }
            }
		}

		public string GetString(object context) {
			return Convert.ToString( GetObject(context) );
		}


		public bool GetBoolean(object context) {
			return Convert.ToBoolean( GetObject(context) );
		}


		public DateTime GetDateTime(object context) {
			return Convert.ToDateTime( GetObject(context) );
		}

	}
}
