using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeParser {
	public static class Parser {

		private static char[] whitespace = { ' ', '\r', '\n', '\t' };

		static string[] keywords = {
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

		static Func<string, Tuple<int,Token>>[] p = {
					ParseKeyword,
					ParseNumericLiteral,
					ParseIdentifier,
					ParseStringLiteral,
				};

		static Tuple<int, Token> ParseNumericLiteral(string s) {
			if (!char.IsDigit(s[0])) {
				return null;
			}
			int length = s
				.TakeWhile(c => char.IsNumber(c) || c == '.' || c == 'e' || c == 'E')
				.Count();
			return Tuple.Create(length, Token.NumericLiteral);
		}

		static Tuple<int, Token> ParseIdentifier(string s) {
			if (!(char.IsLetter(s[0]) || s[0] == '_')) {
				return null;
			}
			int length = s
				.TakeWhile(c => char.IsLetterOrDigit(c) || c == '_')
				.Count();
			return Tuple.Create(length, Token.Identifier);
		}

		static bool PredicateKeyword(string s) {
			return keywords
				.Any(x => s.Substring(0, Math.Min(x.Length,s.Length)) == x && (s.Length <= x.Length || whitespace.Contains(s[x.Length])));
		}
		static Tuple<int, Token> ParseKeyword(string s) {
			var keyword = keywords
				.FirstOrDefault(x => s.Substring(0, Math.Min(x.Length, s.Length)) == x && (s.Length <= x.Length || whitespace.Contains(s[x.Length])));
			if (keyword == null) {
				return null;
			}
			return Tuple.Create(keyword.Length, Token.Keyword);
		}

		static Tuple<int, Token> ParseStringLiteral(string s) {
			if (s[0] == '"' || s[0] == '\'') {
				int length = 2 + s.Skip(1).TakeWhile(c => c != s[0]).Count();
				if (length > s.Length) {
					// Cope with unterminated strings
					length = s.Length;
				}
				return Tuple.Create(length, Token.StringLiteral);
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
