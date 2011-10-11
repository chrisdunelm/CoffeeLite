using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeParser {
	public class TokenInfo {

		internal TokenInfo(int start, int length, Token token, string value, IEnumerable<TokenInfo> innerTokens) {
			this.Start = start;
			this.Length = length;
			this.Token = token;
            this.Value = value;
			this.InnerTokens = innerTokens ?? Enumerable.Empty<TokenInfo>();
		}

		public int Start { get; private set; }
		public int Length { get; private set; }
		public Token Token { get; private set; }
        public string Value { get; private set; }
		public IEnumerable<TokenInfo> InnerTokens { get; private set; }

		public override string ToString() {
			return string.Format("{{ {0}: {1}[{2}] '{3}' }}", this.Token, this.Start, this.Length, this.Value);
		}

	}
}
