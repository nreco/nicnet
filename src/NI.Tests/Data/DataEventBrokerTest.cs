using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using NI.Data;

namespace NI.Tests.Data {
	
	public class DataEventBrokerTest {
		
		[Test]
		public void PublishAndSubscribe() {
			var broker = new DataEventBroker();

			int publishingCallCount = 0;
			int publishedCallCount = 0;
			int allEventsSubscriberCallCount = 0;
			int customEventsSubscriberCallCount = 0;
			EventHandler<EventArgs> publishingHandler = (sender, args) => {
				publishingCallCount++;
			};
			EventHandler<EventArgs> publishedHandler = (sender, args) => {
				publishedCallCount++;
				Assert.AreEqual( publishingCallCount, publishedCallCount );
			};
			EventHandler<EventArgs> allEventsSubscribedHandler = (sender, args) => {
				allEventsSubscriberCallCount++;
				Assert.AreEqual(allEventsSubscriberCallCount, publishingCallCount );
				Assert.AreEqual(allEventsSubscriberCallCount-1, publishedCallCount);
			};
			EventHandler<CustomEventArgs> customEventsSubscribedHandler = (sender, args) => {
				customEventsSubscriberCallCount++;
			};

			broker.Publishing += publishingHandler;
			broker.Published += publishedHandler;

			broker.Subscribe(allEventsSubscribedHandler);
			broker.Subscribe(customEventsSubscribedHandler);

			broker.Publish( this, EventArgs.Empty );
			Assert.AreEqual( 1, allEventsSubscriberCallCount );
			Assert.AreEqual(0, customEventsSubscriberCallCount);

			broker.Publish(this, new CustomEventArgs() );
			Assert.AreEqual(2, allEventsSubscriberCallCount);
			Assert.AreEqual(1, customEventsSubscriberCallCount);
		}

		public class CustomEventArgs : EventArgs {
		}

		[Test]
		public void Unsubscribe() {
			var broker = new DataEventBroker();
			
			var counterHandler = new CounterHanlder();
			broker.Subscribe( new EventHandler<EventArgs>(counterHandler.Handler) );
			broker.Subscribe(new EventHandler<CustomEventArgs>(counterHandler.Handler));
			
			var anotherHandlerCounter = 0;
			broker.Subscribe<EventArgs>( (sender,args) => {
				anotherHandlerCounter++;
			});

			Assert.True( broker.Unsubscribe( new EventHandler<EventArgs>(counterHandler.Handler) ) );
			Assert.False(broker.Unsubscribe(new EventHandler<CustomEventArgs>(counterHandler.Handler)));

			broker.Publish(this, new CustomEventArgs() );
			Assert.AreEqual(0, counterHandler.Counter);
			Assert.AreEqual(1, anotherHandlerCounter);
		}

		public class CounterHanlder {
			public int Counter = 0;
			public void Handler(object sender, EventArgs e) {
				Counter++;
			}
		}

	}
}
