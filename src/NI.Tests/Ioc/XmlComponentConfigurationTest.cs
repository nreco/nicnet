using System;

using NI.Ioc;
using NI.Ioc.Exceptions;
using NUnit.Framework;

namespace NI.Tests.Ioc
{
	/// <summary>
	/// ArrayFactoryTest.
	/// </summary>
	[TestFixture]
	[Category("NI.Ioc")]
	public class XmlComponentConfigurationTest
	{
		public XmlComponentConfigurationTest()
		{
		}
		
		[Test]
		public void FromXmlString() {
			var xmlConfig = new XmlComponentConfiguration(@"
				<components xmlns='urn:schemas-nicnet:ioc:v1'>
					<description>test</description>
					<component name='s1' type='System.String'/>
					<component name='sb1' type='System.Text.StringBuilder'>
						<constructor-arg><ref name='s1'/></constructor-arg>
					</component>
				</components>");
			Assert.AreEqual("test", xmlConfig.Description);
			Assert.AreEqual(2, xmlConfig.Count);
		}

		[Test]
		[ExpectedException(typeof(XmlConfigurationException))]
		public void WrongNamespace() {
			var xmlConfig = new XmlComponentConfiguration(@"
				<components xmlns='urn:strangenamespace'>
					<description>test</description>
				</components>");
		}

		[Test]
		[ExpectedException(typeof(XmlConfigurationException))]
		public void WrongConfigSchema() {
			var xmlConfig = new XmlComponentConfiguration(@"
				<components xmlns='urn:schemas-nicnet:ioc:v1'>
					<component/>
					<component1/>
				</components>");
		}

		[Test]
		[ExpectedException(typeof(XmlConfigurationException))]
		public void WrongRef() {
			var xmlConfig = new XmlComponentConfiguration(@"
				<components xmlns='urn:schemas-nicnet:ioc:v1'>
					<component name='sb1' type='System.Text.StringBuilder'>
						<constructor-arg><ref name='s2'/></constructor-arg>
					</component>
				</components>");
		}

		[Test]
		[ExpectedException(typeof(XmlConfigurationException))]
		public void WrongType() {
			var xmlConfig = new XmlComponentConfiguration(@"
				<components xmlns='urn:schemas-nicnet:ioc:v1'>
					<component name='sb1' type='System.Bla'>
					</component>
				</components>");
		}

		
	}
}
