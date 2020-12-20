using Model;
using NUnit.Framework;
using QWest.DataAccess;
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
    class DaoProgressMapSpec {

        [Test]
        public async Task AddsToDb() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            ProgressMap map = await DAO.ProgressMap.Get(user);
            Assert.NotNull(map.Id);
        }

        [Test]
        public async Task CanMutateLocations() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            ProgressMap map = await DAO.ProgressMap.Get(user);
            int locationId = (int)(await DAO.Geography.GetCountryByAlpha2("DK")).Id;
            map.Locations.Add(locationId);
            await DAO.ProgressMap.Update(map);
            map = await DAO.ProgressMap.Get(map);
            Assert.Contains(locationId, map.Locations.ToList());
            map.Locations.Clear();
            await DAO.ProgressMap.Update(map);
            map = await DAO.ProgressMap.Get(map);
            Assert.IsEmpty(map.Locations);
        }

        [SetUp]
        [OneTimeTearDown]
        public void Setup() {
            Utils.CleanUp();
        }

    }
}
