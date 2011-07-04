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

		public static bool In<T>(this T item, params T[] isIn) {
			return isIn.Contains(item);
		}

		public static IEnumerable<T> Distinct<T, TCompare>(this IEnumerable<T> en, Func<T, TCompare> selector, Func<T, T, T> result) {
			var seen = new Dictionary<TCompare, T>();
			foreach (T item in en) {
				var compare = selector(item);
				T already;
				if (seen.TryGetValue(compare, out already)) {
					var r = result(already, item);
					seen[compare] = r;
				} else {
					seen.Add(compare, item);
				}
			}
			return seen.Values;
		}

	}
}
