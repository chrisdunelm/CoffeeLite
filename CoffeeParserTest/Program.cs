using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoffeeParser;

namespace CoffeeParserTest {
	class Program {
		static void Main(string[] args) {

			var t = Parser.Parse("i = 0").ToArray();

			Console.WriteLine("*** DONE ***");

		}
	}
}
