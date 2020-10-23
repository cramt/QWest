using System.Data.SqlTypes;

namespace QWest.DataAcess {
    public static class Extensions {
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
