using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeParser {

	using ParseResult = Tuple<int, Token, string>;

	public static class Parser {

		private static char[] whitespace = { ' ', '\r', '\n', '\t' };

		private static string[] keywords = {
									   "and",
									   "break",
									   "by",
									   "catch",
									   "class",
									   "continue",
                                       "debugger",
                                       "delete",
									   "do",
									   "else",
									   "extends",
									   "false",
									   "finally",
									   "for",
									   "if",
									   "in",
                                       "instanceof",
									   "is",
									   "isnt",
                                       "loop",
									   "new",
									   "no",
									   "not",
									   "null",
									   "of",
									   "off",
									   "on",
									   "or",
									   "return",
									   "super",
									   "switch",
									   "then",
									   "this",
									   "throw",
									   "true",
									   "try",
                                       "typeof",
									   "undefined",
                                       "unless",
									   "until",
									   "when",
									   "while",
									   "yes",
								   };

		private static Func<string, ParseResult>[] p =
		{
			ParseComment,
            ParseAt,
			ParseKeyword,
			ParseNumericLiteral,
			ParseIdentifier,
			ParseStringLiteral,
		};

		private static ParseResult ParseComment(string s) {
			if (s[0] == '#') {
				return new ParseResult(s.Length, Token.Comment, s);
			}
			return null;
		}

        private static ParseResult ParseAt(string s) {
            if (s[0] == '@') {
                return new ParseResult(1, Token.This, "@");
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
			return new ParseResult(length, Token.NumericLiteral, s.Substring(0, length));
		}

		private static ParseResult ParseIdentifier(string s) {
			if (!(char.IsLetter(s[0]) || s[0] == '_')) {
				return null;
			}
			int length = s
				.TakeWhile(c => char.IsLetterOrDigit(c) || c == '_')
				.Count();
			return new ParseResult(length, Token.Identifier, s.Substring(0, length));
		}

		//private static bool PredicateKeyword(string s) {
		//    return keywords
		//        .Any(x => s.Substring(0, Math.Min(x.Length,s.Length)) == x && (s.Length <= x.Length || whitespace.Contains(s[x.Length])));
		//}

		private static ParseResult ParseKeyword(string s) {
            var keyword = keywords
                .FirstOrDefault(x => s.Substring(0, Math.Min(x.Length, s.Length)) == x &&
                    (s.Length <= x.Length || whitespace.Contains(s[x.Length]) || s[x.Length].In('.', ',' , '[', '(', ';', ')', ']')));
			if (keyword == null) {
				return null;
			}
			return new ParseResult(keyword.Length, Token.Keyword, keyword);
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
				return new ParseResult(length, Token.StringLiteral, s.Substring(length));
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
					ret.Add(new TokenInfo(ofs, r.Item1, r.Item2, r.Item3, null));
					s = s.Substring(r.Item1);
					ofs += r.Item1;
				} else {
					s = s.Substring(1);
					ofs++;
				}
			}
            return PostProcess(ret);
		}

        private static IEnumerable<TokenInfo> PostProcess(IEnumerable<TokenInfo> tokens) {
            TokenInfo prevToken = null;
            foreach (var token in tokens.Concat(new TokenInfo(-1, -1, Token.None, null, null))) {
                if (prevToken != null) {
                    if (prevToken.Token == Token.This && token.Token == Token.Identifier) {
                        yield return new TokenInfo(prevToken.Start, prevToken.Length + token.Length, Token.This, prevToken.Value+token.Value, null);
                    } else if (prevToken.Token == Token.Keyword && prevToken.Value == "this") {
                        yield return new TokenInfo(prevToken.Start, prevToken.Length, Token.This, prevToken.Value, null);
                    } else {
                        if (prevToken.Token != Token.None) {
                            yield return prevToken;
                        }
                    }
                }
                prevToken = token;
            }
        }

	}
}
