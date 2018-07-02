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
using System.Diagnostics;

namespace NI.Data
{
	/// <summary>
	/// Represents query variable
	/// </summary>
	/// <remarks>All query variables should be set before processing the query. 
	/// Accessing undefined variable will cause InvalidOperationException.</remarks>
	[DebuggerDisplay("{Value}:({Name})")]
	[Serializable]
	public class QVar : QConst, IQueryValue {

		/// <summary>
		/// Get variable name
		/// </summary>
		public string Name {
			get; private set;
		}

		/// <summary>
		/// Get actual value represented by this variable
		/// </summary>
		public override object Value {
			get {
				if (!_isDefined)
					throw new InvalidOperationException(String.Format("Query variable '{0}' is not defined", Name));
				return _VarValue; 
			}
		}

		/// <summary>
		/// Get format for variable value (null if formatting is not needed)
		/// </summary>
		public string Format { get; private set; }

		private object _VarValue;
		private bool _isDefined = false;

		/// <summary>
		/// Initializes a new instance of the QVar with specified variable name
		/// </summary>
		/// <param name="varName">variable name</param>
		public QVar(string varName) : base(null) {
			var formatIdx = varName.IndexOf(':');
			if (formatIdx >= 0) {
				Name = varName.Substring(0, formatIdx);
				Format = varName.Substring(formatIdx+1);
				if (Format.Length==0)
					throw new ArgumentException("Format cannot be empty");
			} else { 
				Name = varName;
			}
		}

		/// <summary>
		/// Initializes a new instance of the QVar with specified variable name and format
		/// </summary>
		/// <param name="varName">variable name</param>
		/// <param name="format">the string format applied on setting of the variable</param>
		public QVar(string varName, string format) : base(null) {
			Name = varName;
			Format = format;
		}


		/// <summary>
		/// Assigns a value for this variable
		/// </summary>
		/// <remarks>Assigned QVar can be used as QConst</remarks>
		/// <param name="varValue">variable value</param>
		public void Set(object varValue) {
			if (Format != null) {
				_VarValue = String.Format(Format, varValue);
			} else { 
				_VarValue = varValue;
			}
			_isDefined = true;
		}

		/// <summary>
		/// Makes this variable undefined. 
		/// </summary>
		public void Unset() {
			_isDefined = false;
		}


	}
}
