using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace QWest.DataAcess {
    public static class Extensions {
        public static SqlCommand CreateCommand(this SqlConnection conn, string query) {
            return new SqlCommand(null, conn) {
                CommandText = query
            };
        }
        public static byte[] NullableValue(this SqlBinary bytes) {
            return bytes.IsNull ? null : bytes.Value;
        }
        public static string NullableValue(this SqlString str) {
            return str.IsNull ? null : str.Value;
        }

        public static int? NullableValue(this SqlInt32 i) {
            if (i.IsNull) {
                return null;
            }
            return i.Value;
        }
    }
}
