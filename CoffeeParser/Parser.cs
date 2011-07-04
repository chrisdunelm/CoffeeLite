using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeParser {

	using ParseResult = Tuple<int, Token>;

	public static class Parser {

		private static char[] whitespace = { ' ', '\r', '\n', '\t' };

		private static string[] keywords = {
									   "class",
									   "if",
									   "extends",
									   "new",
									   "else",
									   "is",
									   "isnt",
									   "try",
									   "catch",
									   "finally",
									   "for",
									   "in",
									   "of",
									   "while",
									   "until",
									   "do",
									   "then",
									   "switch",
									   "when",
									   "not",
									   "and",
									   "or",
									   "true",
									   "yes",
									   "on",
									   "false",
									   "no",
									   "off",
									   "this",
								   };

		private static Func<string, ParseResult>[] p =
		{
			ParseComment,
			ParseKeyword,
			ParseNumericLiteral,
			ParseIdentifier,
			ParseStringLiteral,
		};

		private static ParseResult ParseComment(string s) {
			if (s[0] == '#') {
				return new ParseResult(s.Length, Token.Comment);
			}
			return null;
		}

		private static ParseResult ParseNumericLiteral(string s) {
			if (!char.IsDigit(s[0])) {
				return null;
			}
			int length = s
				.TakeWhile(c => char.IsNumber(c) || c == '.' || c == 'e' || c == 'E')
				.Count();
			return new ParseResult(length, Token.NumericLiteral);
		}

		private static ParseResult ParseIdentifier(string s) {
			if (!(char.IsLetter(s[0]) || s[0] == '_')) {
				return null;
			}
			int length = s
				.TakeWhile(c => char.IsLetterOrDigit(c) || c == '_')
				.Count();
			return new ParseResult(length, Token.Identifier);
		}

		private static bool PredicateKeyword(string s) {
			return keywords
				.Any(x => s.Substring(0, Math.Min(x.Length,s.Length)) == x && (s.Length <= x.Length || whitespace.Contains(s[x.Length])));
		}

		private static ParseResult ParseKeyword(string s) {
			var keyword = keywords
				.FirstOrDefault(x => s.Substring(0, Math.Min(x.Length, s.Length)) == x && (s.Length <= x.Length || whitespace.Contains(s[x.Length])));
			if (keyword == null) {
				return null;
			}
			return new ParseResult(keyword.Length, Token.Keyword);
		}

		private static ParseResult ParseStringLiteral(string s) {
			if (s[0] == '"' || s[0] == '\'') {
				bool inEscape = false;
				int length = 2 + s.Skip(1).TakeWhile(c => {
					if (c == '\\') {
						inEscape = true;
					} else {
						if (inEscape) {
							inEscape = false;
						} else {
							return c != s[0];
						}
					}
					return true;
				}).Count();
				if (length > s.Length) {
					// Cope with unterminated strings
					length = s.Length;
				}
				return new ParseResult(length, Token.StringLiteral);
			}
			return null;
		}

		public static IEnumerable<TokenInfo> Parse(string coffee) {
			var ret = new List<TokenInfo>();
			int ofs = 0;
			string s = coffee;
			while (s.Length > 0) {
				var r = p.Select(x => x(s)).Where(x => x != null).FirstOrDefault();
				if (r != null) {
					ret.Add(new TokenInfo(ofs, r.Item1, r.Item2, null));
					s = s.Substring(r.Item1);
					ofs += r.Item1;
				} else {
					s = s.Substring(1);
					ofs++;
				}
			}
			return ret;
		}

	}
}
