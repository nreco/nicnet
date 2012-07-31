using System;

using NI.Ioc;

using NUnit.Framework;

namespace NI.Tests.Winter
{
	/// <summary>
	/// Test for StaticMethodInvokingFactoryTest.
	/// </summary>
	[TestFixture]
	[Category("NI.Ioc")]
	public class StaticMethodInvokingFactoryTest
	{
		public StaticMethodInvokingFactoryTest()
		{
		}
		
		[Test]
		public void test_GetObject() {
			StaticMethodInvokingFactory staticMethodInvokingFactory = new StaticMethodInvokingFactory();
			staticMethodInvokingFactory.TargetType = typeof(System.Text.Encoding);
			staticMethodInvokingFactory.TargetMethod = "GetEncoding";
			staticMethodInvokingFactory.TargetMethodArgTypes = new Type[] { typeof(int) };
			staticMethodInvokingFactory.TargetMethodArgs = new object[] { (int)1252 };
			
			
			Assert.AreEqual( staticMethodInvokingFactory.GetObjectType(), typeof(System.Text.Encoding), "GetObjectType fails");
			System.Text.Encoding enc = (System.Text.Encoding)staticMethodInvokingFactory.GetObject();
			Assert.AreEqual( enc.CodePage, (int)1252, "GetObject fails");
			
		}
		
	}
}
