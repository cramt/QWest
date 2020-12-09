using Model.Geographic;
using NUnit.Framework;
using QWest.DataAccess;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QWest.DataAccess.Tests {
    [TestFixture]
    class DaoGeographySpec : DaoGeographySetupAndTearDown {
        [Test]
        public async Task BackUpsCorrectly() {
            await ConnectionWrapper.Instance.Use("DELETE FROM geopolitical_location", stmt => stmt.ExecuteNonQueryAsync());
            List<Country> countries = GeopoliticalLocation.Parse(File.ReadAllText(Utilities.Utilities.SolutionLocation + "\\QWest.DataAccess\\res\\geopolitical_location_backup.json"));
            await DAO.Geography.InsertBackup(countries);
            int amount = (await DAO.Geography.FetchEverythingParsed()).Count();
            Assert.AreEqual(countries.Count, amount);
        }
        
        [TestFixture]
        public class AddsToTheDatabase : DaoGeographySetupAndTearDown {
            [Test]
            public async Task CountryWithNoSubdivisions() {
                Country country = new Country {
                    Alpha2 = "NO",
                    Name = "Norway",
                    Alpha3 = "NOR"
                };
                await DAO.Geography.AddCountry(country);
                Assert.IsNotNull(country.Id);
            }


            [Test]
            public async Task SubdivisionToExistingCountry() {
                Country country = new Country {
                    Alpha2 = "NO",
                    Name = "Norway",
                    Alpha3 = "NOR"
                };
                await DAO.Geography.AddCountry(country);

                Subdivision subdivision = new Subdivision {
                    SuperId = (int)country.Id,
                    Alpha2 = "01",
                    Name = "Vestfold",
                };
                await DAO.Geography.AddSubdivision(subdivision);
                Assert.IsNotNull(subdivision.Id);                
            }
        }

        [TestFixture]
        public class FetchesFromTheDatabase : DaoGeographySetupAndTearDown {
            [Test]
            public async Task AllCountries() {
                Country country1 = new Country {
                    Alpha2 = "NO",
                    Name = "Norway",
                    Alpha3 = "NOR"
                };
                Country country2 = new Country {
                    Alpha2 = "DK",
                    Name = "Denmark",
                    Alpha3 = "DEN"
                };

                await DAO.Geography.AddCountry(country1);
                await DAO.Geography.AddCountry(country2);
                List<Country> countries = (List<Country>)await DAO.Geography.GetCountries();
                Assert.AreEqual(2, countries.Count);
            }
            
            [Test]
            public async Task CountryByAlpha2() {
                Country country = new Country {
                    Alpha2 = "NO",
                    Name = "Norway",
                    Alpha3 = "NOR"
                };
                await DAO.Geography.AddCountry(country);
                Country fetchedCountry = await DAO.Geography.GetCountryByAlpha2(country.Alpha2);
                Assert.AreEqual(country.Id, fetchedCountry.Id);
            }

            public async Task SubdivisionByParentLocation() {
                Country country = new Country {
                    Alpha2 = "NO",
                    Name = "Norway",
                    Alpha3 = "NOR"
                };
                await DAO.Geography.AddCountry(country);

                Subdivision subdivision = new Subdivision {
                    SuperId = (int)country.Id,
                    Alpha2 = "01",
                    Name = "Vestfold",
                };
                await DAO.Geography.AddSubdivision(subdivision);

                Subdivision fetchedSubdivision = (await DAO.Geography.GetSubdivisions((int)country.Id) as List<Subdivision>)[0];
                Assert.AreEqual(subdivision.Id, fetchedSubdivision.Id);
            }
        }

        [Test]
        public async Task UpdatesLocationInDatabase() {
            Country country = new Country {
                Alpha2 = "NO",
                Name = "Norway",
                Alpha3 = "NOR"
            };
            await DAO.Geography.AddCountry(country);

            country.Name = "Norway Denmark";
            await DAO.Geography.Update(country);

            Country fetchedCountry = await DAO.Geography.GetCountryByAlpha2("NO");
            Assert.AreEqual(country.Name, fetchedCountry.Name);
        }

        [TestFixture]
        public class DeletesFromTheDatabase : DaoGeographySetupAndTearDown {
            [Test]
            public async Task CountryWithNoSubdivisions() {
                Assert.Ignore();
            }
            
            [Test]
            public async Task SubdivisionWithNoSubdivision() {
                Assert.Ignore();
            }

            [Test]
            public async Task CountryWithSubdivisions() {
                Assert.Ignore();
            }

            [Test]
            public async Task SubdivisionWithSubdivisions() {
                Assert.Ignore();
            }
        }
    }

    class DaoGeographySetupAndTearDown {
        [SetUp]
        public void SetUp() {
            Utils.CleanUp(true);
        }

        [OneTimeTearDown]
        public void TearDown() {
            Utils.CleanUp(true);
            List<Country> countries = GeopoliticalLocation.Parse(File.ReadAllText(Utilities.Utilities.SolutionLocation + "\\QWest.DataAccess\\res\\geopolitical_location_backup.json"));
            DAO.Geography.InsertBackup(countries).Wait();
        }
    }
}
