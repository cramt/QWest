using Model.Geographic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QWest.DataAcess {
    public static partial class DAO {
        public interface IGeography {
            Task InsertBackup(IEnumerable<Country> countries);
            Task<IEnumerable<Country>> CreateBackup();
            Task<Country> GetCountryByAlpha2(string alpha2);
            Task<GeopoliticalLocation> GetAnyByAlpha2s(string alphas2);
            Task<GeopoliticalLocation> GetAnyByAlpha2s(IEnumerable<string> alpha2s);
            Task Update(GeopoliticalLocation location);
            Task Delete(GeopoliticalLocation location);
            Task Delete(int id);
            Task<int> Add(GeopoliticalLocation location);
            Task<IEnumerable<Country>> GetCountries();
            Task<IEnumerable<Subdivision>> GetSubdivisions(int superId);
            Task<IEnumerable<Subdivision>> GetSubdivisions(GeopoliticalLocation location);
            Task<IEnumerable<GeopoliticalLocation>> NameSearch(string search);
        }

        public static IGeography Geography { get; set; } = new Mssql.GeographyImpl(ConnectionWrapper.Instance);
    }
}
