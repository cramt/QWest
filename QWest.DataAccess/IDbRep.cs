using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QWest.DataAcess {
    interface IDbRep<T> {
        T ToModel();
    }
}
