using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Utilities {
    public static class Extensions {
        public static IEnumerable<TResult> SelectPar<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) {
            return Task.WhenAll(source.Select(x => Task.Factory.StartNew(() => selector(x)))).GetAwaiter().GetResult();
        }
        public static IEnumerable<T> ToIter<T>(this SqlDataReader reader, Func<SqlDataReader, T> transformer) {
            while (reader.Read()) {
                yield return transformer(reader);
            }
        }

        public static IEnumerable<T> ToIterator<T>(this SqlDataReader reader, Func<SqlDataReader, T> transformer) {
            return ToIter(reader, transformer).ToList();
        }

        public static T Unwrap<T>(this T self) where T : class {
            if (self == null) {
                throw new ArgumentException("panic at unwrap");
            }
            return self;
        }

        public static T UnwrapOr<T>(this T self, T or) where T : class {
            if (self == null) {
                return or;
            }
            return self;
        }

        public static U MapValue<T, U>(this T self, Func<T, U> func) where T : class where U : class {
            if (self == null) {
                return null;
            }
            return func(self);
        }

        public static byte[] Base64(this string s) {
            return Convert.FromBase64String(s);
        }

        public static string Base64(this byte[] b) {
            return Convert.ToBase64String(b);
        }
    }
}
