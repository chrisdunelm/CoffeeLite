using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeParser {
    static class Extensions {

        public static bool In<T>(this T item, params T[] isIn) {
            return isIn.Contains(item);
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> a, T b) {
            return a.Concat(new[] { b });
        }
    
    }
}
