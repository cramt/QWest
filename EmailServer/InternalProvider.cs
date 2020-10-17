using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailServer {
    internal class InternalProvider {
        public static string Username { get; } = Guid.NewGuid().ToString();
        public static string Password { get; } = Guid.NewGuid().ToString();
    }
}
