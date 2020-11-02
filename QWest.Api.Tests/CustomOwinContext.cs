using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QWest.Api.Tests {
    public class CustomOwinContext : IOwinContext {
        public IOwinRequest Request => throw new NotImplementedException();

        public IOwinResponse Response => throw new NotImplementedException();

        public IAuthenticationManager Authentication => throw new NotImplementedException();

        public IDictionary<string, object> Environment => throw new NotImplementedException();

        public TextWriter TraceOutput { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();

        public T Get<T>(string key) {
            if (Values.ContainsKey(key)) {
                return (T)Values[key];
            }
            return default;
        }

        public IOwinContext Set<T>(string key, T value) {
            Values[key] = value;
            return this;
        }
    }
}
