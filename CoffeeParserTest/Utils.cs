using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoffeeParser;

namespace CoffeeParserTest {

	struct TokenInfo2 {
		public int Start, Length;
		public Token Token;
		public override string ToString() {
			return string.Format("{0}:{1}[{2}]", this.Token, this.Start, this.Length);
		}
	}

	static class Utils {

		public static TokenInfo2 TI2(int start, int length, Token token) {
			return new TokenInfo2 {
				Start = start,
				Length = length,
				Token = token,
			};
		}

		public static IEnumerable<TokenInfo2> ForTest(this IEnumerable<TokenInfo> tis) {
			return tis.Select(x => new TokenInfo2 {
				Start = x.Start,
				Length = x.Length,
				Token = x.Token,
			}).ToArray();
		}

	}
}
