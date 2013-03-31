using System;

using NI.Ioc;

using NUnit.Framework;

namespace NI.Tests.Ioc
{
	/// <summary>
	/// </summary>
	[TestFixture]
	[Category("NI.Ioc")]
	public class StaticPropertyInvokingFactoryTest
	{
		public StaticPropertyInvokingFactoryTest()
		{

		}
		[Test]
		public void test_GetObject() {
			StaticPropertyInvokingFactory statPropFactory = new StaticPropertyInvokingFactory();
			statPropFactory.TargetType = typeof(customClass);
			statPropFactory.TargetProperty = "staticProperty";
			
			object res = statPropFactory.GetObject();
			Assert.AreEqual( res, "staticvalue", "GetObject fails");
		}
		
	 public class	customClass {
			
			public string objectProperty {
				get {
					return "objectvalue";
				}
				set {
				}
			}

			public static string staticProperty {
				get {
					return "staticvalue";
				}
			}
		}		
		
	}
}
