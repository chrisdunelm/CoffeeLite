using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeSyntax {
	static class Extensions {

		public static bool IsAt(this string s, int ofs, string isAt) {
			return s.Skip(ofs).Take(isAt.Length).SequenceEqual(isAt);
		}

		public static TResult NullThru<T, TResult>(this T o, Func<T, TResult> fn, TResult @default = default(TResult)) {
			return o == null ? @default : fn(o);
		}

		public static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, TValue @default = default(TValue)) {
			TValue ret;
			if (d.TryGetValue(key, out ret)) {
				return ret;
			}
			return @default;
		}

		//public static IEnumerable<T> Distinct<T, TDistinct>(this IEnumerable<T> en, Func<T, TDistinct> selector) {

		//}

	}
}
