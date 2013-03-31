using System;

using NI.Ioc;

using NUnit.Framework;

namespace NI.Tests.Ioc
{
	/// <summary>
	/// Test for MethodInvokingFactory.
	/// </summary>
	[TestFixture]
	[Category("NI.Ioc")]
	public class MethodInvokingFactoryTest
	{
		public MethodInvokingFactoryTest()
		{
		}
		
		[Test]
		public void test_GetObject() {
			MethodInvokingFactory methodInvokingFactory = new MethodInvokingFactory();
			methodInvokingFactory.TargetObject = this;
			methodInvokingFactory.TargetMethod = "TestMethodToInvoke";
			methodInvokingFactory.TargetMethodArgTypes = new Type[] { typeof(string), typeof(int) };
			methodInvokingFactory.TargetMethodArgs = new object[] { "ZZZ", 2 };
			
			Assert.AreEqual( methodInvokingFactory.GetObject(), "ZZZ,2", "GetObject fails");
			Assert.AreEqual( methodInvokingFactory.GetObjectType(), typeof(string), "GetObjectType fails");

			methodInvokingFactory.TargetMethod = "SimpleTestMethodToInvoke";
			methodInvokingFactory.TargetMethodArgTypes = null;
			methodInvokingFactory.TargetMethodArgs = null;

			Assert.AreEqual( methodInvokingFactory.GetObject(), (int)0, "GetObject fails");
		}
		
		public string TestMethodToInvoke(string a, int b) {
			return String.Format("{0},{1}", a,b);
		}
		
		public int SimpleTestMethodToInvoke() {
			return 0;
		}
		
	}
}
