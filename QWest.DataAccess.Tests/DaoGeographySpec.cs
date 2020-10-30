using Model.Geographic;
using NUnit.Framework;
using QWest.DataAcess;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QWest.DataAccess.Tests {
    [TestFixture]
    class DaoGeographySpec {
        [Test]
        public async Task BackUpsCorrectly() {
            await ConnectionWrapper.Instance.Use("DELETE FROM geopolitical_location", stmt => stmt.ExecuteNonQueryAsync());
            List<Country> countries = GeopoliticalLocation.Parse(File.ReadAllText(Utilities.Utilities.SolutionLocation + "\\QWest.DataAccess\\res\\geopolitical_location_backup.json"));
            await DAO.Geography.InsertBackup(countries);
            int amount = (await DAO.Geography.CreateBackup()).Count();
            Assert.AreEqual(countries.Count, amount);
        }
        [Test]
        public async Task FetchesAlbaniaCorrectly() {
            Country albania = await DAO.Geography.GetCountryByAlpha2("AL");
            Assert.AreEqual("Albania", albania.Name);
            Assert.AreEqual(12, albania.Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[0].Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[1].Subdivisions.Count);
            Assert.AreEqual(2, albania.Subdivisions[2].Subdivisions.Count);
            Assert.AreEqual(4, albania.Subdivisions[3].Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[4].Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[5].Subdivisions.Count);
            Assert.AreEqual(4, albania.Subdivisions[6].Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[7].Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[8].Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[9].Subdivisions.Count);
            Assert.AreEqual(2, albania.Subdivisions[10].Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[11].Subdivisions.Count);
        }
        [Test]
        public async Task FetchesNordjyllandFromAlpha2s() {
            GeopoliticalLocation nordjylland = await DAO.Geography.GetAnyByAlpha2s("DK-81");
            Assert.AreEqual("Nordjylland", nordjylland.Name);
        }
        [Test]
        public async Task FetchesBeratFromAlpha2s() {
            GeopoliticalLocation berat = await DAO.Geography.GetAnyByAlpha2s("AL-01");
            Assert.AreEqual("Berat", berat.Name);
            Assert.AreEqual(3, berat.Subdivisions.Count);
        }

    }
}
