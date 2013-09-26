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

namespace NI.Data
{
	/// <summary>
	/// Query variable
	/// </summary>
	[Serializable]
	public class QVar : QConst, IQueryValue {

		public string Name {
			get; private set;
		}

		public override object Value {
			get {
				if (!_isDefined)
					throw new InvalidOperationException(String.Format("Query variable '{0}' is not defined", Name));
				return _VarValue; 
			}
		}

		private object _VarValue;
		private bool _isDefined = false;

		public QVar(string varName) : base(null) {
			Name = varName;
		}

		public void Set(object varValue) {
			_VarValue = varValue;
			_isDefined = true;
		}

		public void Unset() {
			_isDefined = false;
		}


	}
}
