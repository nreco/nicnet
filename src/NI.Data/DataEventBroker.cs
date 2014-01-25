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
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace NI.Data
{
	/// <summary>
	/// Generic implementation of data event broker used for data triggers.
	/// </summary>
	public class DataEventBroker
	{

		/// <summary>
		/// Occurs each time when event is published, but before executing subscribed handlers
		/// </summary>
		public event EventHandler<EventArgs> Publishing;

		/// <summary>
		/// Occurs each time when event is published, but after executing subscribed handlers
		/// </summary>
		public event EventHandler<EventArgs> Published;

		private IDictionary<Type, IList<HandlerWrapper>> eventHandlers = new Dictionary<Type, IList<HandlerWrapper>>();

		/// <summary>
		/// Initializes a new instance of DataEventBroker
		/// </summary>
		public DataEventBroker() {
		}

		/// <summary>
		/// Initializes a new instance of DataEventBroker and binds broker Publish method to all events
		/// of specified <see cref="NI.Data.DbDalc"/>.
		/// </summary>
		/// <param name="dalc">DbDalc instance</param>
		public DataEventBroker(DbDalc dalc) {
			dalc.RowUpdating += new EventHandler<RowUpdatingEventArgs>(Publish);
			dalc.RowUpdated += new EventHandler<RowUpdatedEventArgs>(Publish);
			dalc.DbCommandExecuting += new EventHandler<DbCommandExecutingEventArgs>(Publish);
			dalc.DbCommandExecuted += new EventHandler<DbCommandExecutedEventArgs>(Publish);
		}

		/// <summary>
		/// Publish specified event
		/// </summary>
		/// <param name="sender">event source</param>
		/// <param name="eventArgs">event arguments</param>
		public virtual void Publish(object sender, EventArgs eventArgs) {
			if (eventArgs==null)
				throw new ArgumentNullException("eventData");
			if (Publishing != null)
				Publishing(sender, eventArgs);

			var eventDataType = eventArgs.GetType();
			while (eventDataType != null) {
				if (eventHandlers.ContainsKey(eventDataType))
					foreach (var h in eventHandlers[eventDataType])
						h.Handler.DynamicInvoke(sender, eventArgs);
				eventDataType = eventDataType.BaseType;
			}

			if (Published != null)
				Published(sender, eventArgs);
		}

		/// <summary>
		/// Subscribe a handler for specified event type
		/// </summary>
		/// <typeparam name="T">event type to match</typeparam>
		/// <param name="handler">event handler delegate</param>
		public void Subscribe<T>(EventHandler<T> handler) where T : EventArgs {
			var eventType = typeof(T);
			SubscribeInternal(eventType, null, handler);
		}

		/// <summary>
		/// Subscribe a handler for specified event type
		/// </summary>
		/// <typeparam name="T">event type to match</typeparam>
		/// <param name="match">match condition delegate</param>
		/// <param name="handler">event handler delegate</param>
		public void Subscribe<T>(Func<EventArgs, bool> match, EventHandler<T> handler) where T : EventArgs {
			var eventType = typeof(T);
			SubscribeInternal(eventType, match, handler);
		}


		/// <summary>
		/// Subscribe a handler for specified event type
		/// </summary>
		/// <param name="eventType">event type to match</param>
		/// <param name="handler">event handler delegate</param>
		public void Subscribe(Type eventType, EventHandler<EventArgs> handler) {
			SubscribeInternal(eventType, null, handler);
		}

		/// <summary>
		/// Subscribe a handler for specified event type and match condition
		/// </summary>
		/// <param name="eventType">event type to match</param>
		/// <param name="match">match condition delegate</param>
		/// <param name="handler">event handler delegate</param>
		public void Subscribe(Type eventType, Func<EventArgs, bool> match, EventHandler<EventArgs> handler) {
			SubscribeInternal(eventType, match, handler);
		}

		protected virtual void SubscribeInternal(Type eventType, Func<EventArgs, bool> match, Delegate handler) {
			if (!eventHandlers.ContainsKey(eventType))
				eventHandlers[eventType] = new List<HandlerWrapper>();
			eventHandlers[eventType].Add(new HandlerWrapper(handler, match));
		}

		/// <summary>
		/// Unsubscribes specified delegate from all events
		/// </summary>
		/// <remarks>This method unsubscribes specified delegate from ALL event types</remarks>
		/// <param name="handler">delegate to unsubscribe</param>
		/// <returns>true if item is successfully removed; otherwise, false.</returns>
		public virtual bool Unsubscribe(Delegate handler) {
			var removed = false;
			var h = new HandlerWrapper(handler, null);
			foreach (var entry in eventHandlers) {
				if (entry.Value.Remove(h)) {
					removed = true;
				}
			}
			return removed;
		}

		internal class HandlerWrapper {
			internal Delegate Handler;
			internal Func<EventArgs,bool> Match;

			public HandlerWrapper(Delegate d, Func<EventArgs,bool> match) {
				Handler = d;
				Match = match;
			}
		
			public bool IsMatch(EventArgs e) {
				if (Match!=null)
					return Match(e);
				return true;
			}

			public override bool Equals(object o)
			{
 				if (!(o is HandlerWrapper)) return false;
				var other = (HandlerWrapper)o;

				if (Handler.Target == other.Handler.Target && Handler.Method==other.Handler.Method) {
					return true;
				}
				return Handler==((HandlerWrapper)o).Handler;
			}

			public override int GetHashCode() {
				return Handler.GetHashCode();
			}

		}


	}
}
