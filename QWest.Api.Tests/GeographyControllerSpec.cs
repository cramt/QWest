using Model.Geographic;
using NUnit.Framework;
using QWest.Apis;
using QWest.DataAcess;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QWest.Api.Tests {
    [TestFixture]
    public class GeographyControllerSpec {
        public class GeographyRepoMock : DAO.IGeography {
            public Task<int> Add(GeopoliticalLocation location) {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<Country>> CreateBackup() {
                throw new NotImplementedException();
            }

            public Task Delete(GeopoliticalLocation location) {
                throw new NotImplementedException();
            }

            public Task Delete(int id) {
                throw new NotImplementedException();
            }

            public Task<GeopoliticalLocation> GetAnyByAlpha2s(string alphas2) {
                throw new NotImplementedException();
            }

            public Task<GeopoliticalLocation> GetAnyByAlpha2s(IEnumerable<string> alpha2s) {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<Country>> GetCountries() {
                throw new NotImplementedException();
            }

            public Task<Country> GetCountryByAlpha2(string alpha2) {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<Subdivision>> GetSubdivisions(int superId) {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<Subdivision>> GetSubdivisions(GeopoliticalLocation location) {
                throw new NotImplementedException();
            }

            public Task InsertBackup(IEnumerable<Country> countries) {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<GeopoliticalLocation>> NameSearch(string search) {
                throw new NotImplementedException();
            }

            public Task Update(GeopoliticalLocation location) {
                throw new NotImplementedException();
            }
        }
        [Test]
        public async Task NullReturnsValidImage() {
            
        }
    }
}
