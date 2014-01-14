#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas,  Vitalii Fedorchenko (v.2 changes)
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
using System.Reflection;



namespace NI.Ioc {

	/// <summary>
	/// EventBinder is a helper component that gives ability to bind events and events handlers inside IoC-configuration.
	/// </summary>
	public class EventBinder {
		object _SenderObject;
		object _ReceiverObject;
		string _SenderEvent;
		string _ReceiverMethod;
		
		/// <summary>
		/// Get or set event sender object
		/// </summary>
		public object SenderObject {
			get { return _SenderObject; }
			set { _SenderObject = value; }
		}
		
		/// <summary>
		/// Get or set receiver method object
		/// </summary>
		public object ReceiverObject {
			get { return _ReceiverObject; }
			set { _ReceiverObject = value; }
		}
		
		/// <summary>
		/// Get or set event name
		/// </summary>
		public string SenderEvent {
			get { return _SenderEvent; }
			set { _SenderEvent = value; }
		}
		
		/// <summary>
		/// Get or set event name
		/// </summary>
		public string ReceiverMethod {
			get { return _ReceiverMethod; }
			set { _ReceiverMethod = value; }
		}
		
		
		public EventBinder() {
		}
		
		
		/// <summary>
		/// Perform all necessary calls
		/// </summary>
		public virtual void Init() {
			EventInfo myEventBinding = SenderObject.GetType().GetEvent(SenderEvent);
            if (myEventBinding == null) throw new NullReferenceException(SenderEvent);
				
			MethodInfo mInfo = ReceiverObject.GetType().GetMethod(ReceiverMethod);
            if (mInfo == null) throw new MissingMethodException(ReceiverObject.GetType().ToString(), ReceiverMethod);

            try {
                System.Delegate del = System.Delegate.CreateDelegate(myEventBinding.EventHandlerType, ReceiverObject, mInfo);
                myEventBinding.AddEventHandler(SenderObject, del);
            } catch (Exception e) { 
                throw new Exception(
						String.Format("Cannot subscribe '{0}' method on '{1}' event: {2}",
						ReceiverMethod, SenderEvent, e.Message), e );
            }

		}
		
	}
}
