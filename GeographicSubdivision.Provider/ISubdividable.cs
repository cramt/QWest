using System.Collections.Generic;

namespace GeographicSubdivision.Provider {
    public interface ISubdividable {
        List<Subdivision> Subdivisions { get; }
        string Name { get; }
        List<string> Names { get; }
        string Type { get; }
        string GetFullId();
    }
}
