using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NI.Data.Storage.Model;

namespace NI.Data.Storage.Tests.Model {

	[TestFixture]
	public class RelationshipTests {

		[Test]
		public void EqualsAndHashCode() {
			var class1 = new Class("class1");
			var class2 = new Class("class2");
			var pred1 = new Class("pred1") { IsPredicate = true };
			var pred2 = new Class("pred2") { IsPredicate = true };

			var rel1 = new Relationship(class1, pred1, class2, false,false, null);
			var rel1same = new Relationship(class1, pred1, class2, false,false, null);
			Assert.AreEqual(
				rel1same, rel1);
			Assert.AreEqual(
				rel1same.GetHashCode(), rel1.GetHashCode() );
			Assert.AreNotEqual(
				new Relationship(class2, pred1, class1, false, false, null), rel1);
			Assert.AreNotEqual(
				new Relationship(class1, pred2, class2, false, false, null), rel1);

			var rel2 = new Relationship(class2, pred2, class1, false, false, null);
			var infRel1 = new Relationship(class1, new[] { rel1, rel2 }, class1);
			var infRel1same = new Relationship(class1, new[] { rel1, rel2 }, class1);
			Assert.True(infRel1.Inferred);
			Assert.AreEqual( infRel1same, infRel1 );
			Assert.AreEqual(infRel1same.GetHashCode(), infRel1.GetHashCode() );

		}

	}
}
