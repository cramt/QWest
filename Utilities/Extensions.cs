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
    }
}
