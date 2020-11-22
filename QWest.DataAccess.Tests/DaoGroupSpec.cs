using Model;
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
using Utilities;

namespace QWest.DataAccess.Tests {
    [TestFixture]
    class DaoGroupSpec {
        [Test]
        public async Task AddsToDb() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            Group group = await DAO.Group.Create(new Group("lucca's friends", DateTime.Now, "we're lucca's friends", null, new User[] { user }));
            Assert.NotNull(group.Id);
        }

        [Test]
        public async Task FetchFromDb() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            Group group = await DAO.Group.Create(new Group("lucca's friends", DateTime.Now, "we're lucca's friends", null, new User[] { user }));
            IEnumerable<Group> groups = await DAO.Group.FetchUsersGroups(user);
            Assert.AreEqual(group.Id, groups.ElementAt(0).Id);
        }
        [SetUp]
        [OneTimeTearDown]
        public void Setup() {
            ConnectionWrapper.Instance.Use("DELETE FROM users_groups; DELETE FROM users; DELETE FROM groups", stmt => stmt.ExecuteNonQueryAsync()).Wait();
        }

    }
}
