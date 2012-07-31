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

namespace NI.Common.Expressions
{
	/// <summary>
	/// </summary>
	public class ExpressionDescriptor : IExpressionDescriptor
	{
		string _Marker;
		IExpressionResolver _ExprResolver;
		
		public string Marker {
			get { return _Marker; }
			set { _Marker = value; }
		}
		
		public IExpressionResolver ExprResolver {
			get { return _ExprResolver; }
			set { _ExprResolver = value; }
		}
		
		public ExpressionDescriptor()
		{
		}
		
		public ExpressionDescriptor(string marker, IExpressionResolver exprResolver) {
			Marker = marker;
			ExprResolver = exprResolver;
		}
		
	}
}
