using System;

using NI.Ioc;

using NUnit.Framework;

namespace NI.Tests.Ioc
{
	/// <summary>
	/// ArrayFactoryTest.
	/// </summary>
	[TestFixture]
	[Category("NI.Ioc")]
	public class ArrayFactoryTest
	{
		public ArrayFactoryTest()
		{
		}
		
		[Test]
		public void test_GetObject() {
			ArrayFactory arrayFactory = new ArrayFactory();
			arrayFactory.ElementType = typeof(string);
			arrayFactory.Elements = new string[] {"a", "b"};
			
			Assert.AreEqual( arrayFactory.GetObjectType(), typeof(string[]), "GetObjectType failed");
			string[] obj = arrayFactory.GetObject() as string[];
			Assert.AreEqual( obj.Length, 2, "Invalid result object");
			Assert.AreEqual( obj[0], "a", "Invalid first element of result object");
			Assert.AreEqual( obj[1], "b", "Invalid second element of result object");
		}
		
	}
}
