using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QWest.DataAcess {
    public static class Extensions {
        public static byte[] NullableValue(this SqlBinary bytes) {
            return bytes.IsNull ? null : bytes.Value;
        }
    }
}
