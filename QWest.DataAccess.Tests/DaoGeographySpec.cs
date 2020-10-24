using Model;
using Model.Geographic;
using NUnit.Framework;
using QWest.DataAcess;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QWest.DataAccess.Tests {
    [TestFixture]
    class DaoGeographySpec {
        [Test]
        public async Task FetchesAlbaniaCorrectly() {
            Country albania = await DAO.Geography.GetCountryByAlpha2("AL");
            Assert.AreEqual("Albania", albania.Name);
            Assert.AreEqual("Republic of Albania", albania.OfficialName);
            Assert.AreEqual(12, albania.Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[0].Subdivisions.Count);
            Assert.AreEqual(2, albania.Subdivisions[1].Subdivisions.Count);
            Assert.AreEqual(4, albania.Subdivisions[2].Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[3].Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[4].Subdivisions.Count);
            Assert.AreEqual(4, albania.Subdivisions[5].Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[6].Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[7].Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[8].Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[9].Subdivisions.Count);
            Assert.AreEqual(2, albania.Subdivisions[10].Subdivisions.Count);
            Assert.AreEqual(3, albania.Subdivisions[11].Subdivisions.Count);
        }
    }
}
