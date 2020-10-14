using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Utilities {
    public static class Extensions {
        public static IEnumerable<T> ToIterator<T>(this SqlDataReader reader, Func<SqlDataReader, T> transformer) {
            while (reader.Read()) {
                yield return transformer(reader);
            }
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
    }
}
