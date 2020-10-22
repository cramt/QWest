using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities {
    public class ReferenceWrapper<T> {
        public T Value { get; set; }
        public ReferenceWrapper(T t) {
            Value = t;
        }
        public ReferenceWrapper() {
            Value = default;
        }
    }
}
