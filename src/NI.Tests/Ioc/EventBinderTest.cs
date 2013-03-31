using System;

using NI.Ioc;

using NUnit.Framework;

namespace NI.Tests.Ioc
{
	/// <summary>
	/// Test for EventBinder.
	/// </summary>
	[TestFixture]
	[Category("NI.Ioc")]
	public class EventBinderTest
	{
		public EventBinderTest() {
		}
		
		//event TestEvent;
		
		[Test]
		public void test_Init() {
			EventBinder EventBinder = new EventBinder();
			EventBinder.SenderObject = this;
			EventBinder.ReceiverObject = this;			
			EventBinder.SenderEvent = "TestEventToBind";
			EventBinder.ReceiverMethod = "TestMethodToBind";

            EventBinder.Init();
			TestEventArgs arg1 = new TestEventArgs();
            this.TestEventToBind(this,arg1);
			Assert.AreEqual( true, arg1.Result );


			EventBinder.SenderObject = this;
			EventBinder.ReceiverObject = this;
			EventBinder.SenderEvent = "TestEventToBind";
			EventBinder.ReceiverMethod = "SimpleTestMethodToBind";

            EventBinder.Init();
			TestEventArgs arg2 = new TestEventArgs();
            this.TestEventToBind.Invoke(this, arg2);
			Assert.AreEqual( true, arg2.Result );
		}
		
		public class TestEventArgs : EventArgs {
			public bool Result = false;

			public TestEventArgs() { }
		}

		public void TestMethodToBind(object sender, EventArgs e) {
			((TestEventArgs)e).Result = true;
		}
		
		public void SimpleTestMethodToBind(object sender, EventArgs e) {
			((TestEventArgs)e).Result = true;
		}
				
		public event EventHandler TestEventToBind;
	}
}
