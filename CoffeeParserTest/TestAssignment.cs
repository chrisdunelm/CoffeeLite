using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CoffeeParser;

namespace CoffeeParserTest {

	[TestFixture]
	public class TestAssignment {

		[Test]
		public void A1() {
			var t = Parser.Parse("i = 0").ForTest();
			var e = new[] {
				Utils.TI2(0, 1, Token.Identifier),
				Utils.TI2(4, 1, Token.NumericLiteral),
			};
			Assert.That(t, NUnit.Framework. Is.EquivalentTo(e));
		}

	}

}
