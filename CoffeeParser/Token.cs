using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeParser {
	public enum Token {

        None = 0,

		StringLiteral,
		NumericLiteral,
		Keyword,
		Identifier,
		Comment,
        This,

	}
}
