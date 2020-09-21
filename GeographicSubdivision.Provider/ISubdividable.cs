using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeographicSubdivision.Provider {
    public interface ISubdividable {
        List<Subdivision> Subdivisions { get; }
        string Name { get; }
    }
}
