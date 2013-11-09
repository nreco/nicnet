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
	public class FuncInterfaceTest
	{
		public FuncInterfaceTest()
		{
		}
		
		[Test]
		public void test_GetObject() {
			Func<int, string> f = (i) => { return i.ToString(); };




		}

		public interface ITest {
			string ToStr(int i);
		}

	}
}
