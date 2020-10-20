using System.Data.SqlTypes;

namespace QWest.DataAcess {
    public static class Extensions {
        public static byte[] NullableValue(this SqlBinary bytes) {
            return bytes.IsNull ? null : bytes.Value;
        }
        public static string NullableValue(this SqlString bytes) {
            return bytes.IsNull ? null : bytes.Value;
        }
    }
}
