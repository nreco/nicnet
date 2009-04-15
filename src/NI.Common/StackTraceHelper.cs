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
using System.Diagnostics;
using System.Reflection;


namespace NI.Common
{
	/// <summary>
	/// StackTrace Helper.
	/// </summary>
	public class StackTraceHelper
	{
		/// <summary>
		/// Find method by name in the current call-stack
		/// </summary>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static MethodBase FindMethod(string methodName) {
			StackTrace trace = new StackTrace();
		
			for (int i=0; i<trace.FrameCount; i++) {
				StackFrame frame = trace.GetFrame(i);
				MethodBase method = frame.GetMethod();
				if (method.Name==methodName)
					return method;
			}
			return null;
		
		}
		
		/// <summary>
		/// Find specified method attribute in the call-stack 
		/// </summary>
		/// <param name="attributeType"></param>
		/// <returns></returns>
		public static object FindAttribute(Type attributeType) {
			StackTrace trace = new StackTrace();
		
			for (int i=0; i<trace.FrameCount; i++) {
				StackFrame frame = trace.GetFrame(i);
				MethodBase method = frame.GetMethod();
				object[] customAttributes = method.GetCustomAttributes(attributeType, true);
				if (customAttributes.Length>0) return customAttributes[0];
			}
			return null;
		}
		
		
	}
}
