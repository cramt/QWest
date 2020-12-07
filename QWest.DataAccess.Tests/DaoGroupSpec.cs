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
    class DaoGroupSpec : DaoGeographySetupAndTearDown {
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

        [TestFixture]
        public class UpdatesMembers : DaoGroupSpecSetUpAndTearDown
        {
            [Test]
            public async Task AddsMembers()
            {
                User user = new User("Lucca", "123456", "an@email.com");
                await DAO.User.Add(user);
                Group group = await DAO.Group.Create(new Group("lucca's friends", DateTime.Now, "we're lucca's friends", null, new User[] { user }));
                User userToAdd = new User("Alexandra", "12345678", "another@email.com");
                await DAO.User.Add(userToAdd);
                await DAO.Group.UpdateMembers((int)group.Id, new List<int> { (int)userToAdd.Id }, new List<int>());
                Assert.IsTrue(await DAO.Group.IsMember(group, userToAdd));
            }
            [Test]
            public async Task RemovesMembers()
            {
                User user = new User("Lucca", "123456", "an@email.com");
                User userToRemove = new User("Alexandra", "12345678", "another@email.com");
                await DAO.User.Add(user);
                await DAO.User.Add(userToRemove);
                Group group = await DAO.Group.Create(new Group("lucca's friends", DateTime.Now, "we're lucca's friends", null, new User[] { user, userToRemove }));
                await DAO.Group.UpdateMembers((int)group.Id, new List<int>(), new List<int> { (int)userToRemove.Id });
                Assert.IsFalse(await DAO.Group.IsMember(group, (int)userToRemove.Id));
            }
            [Test]
            public async Task AddsAndRemovesMembers()
            {
                User user = new User("Lucca", "123456", "an@email.com");
                User userToRemove = new User("Alexandra", "12345678", "another@email.com");
                User userToAdd = new User("Zoe", "12345678", "athird@email.com");
                await DAO.User.Add(user);
                await DAO.User.Add(userToRemove);
                await DAO.User.Add(userToAdd);
                Group group = await DAO.Group.Create(new Group("lucca's friends", DateTime.Now, "we're lucca's friends", null, new User[] { user, userToRemove }));
                await DAO.Group.UpdateMembers((int)group.Id, new List<int> { (int)userToAdd.Id }, new List<int> { (int)userToRemove.Id });
                Assert.IsTrue(await DAO.Group.IsMember(group, (int)userToAdd.Id));
                Assert.IsFalse(await DAO.Group.IsMember(group, (int)userToRemove.Id));
            }
        }
        

    }

    public class DaoGroupSpecSetUpAndTearDown
    {
        [SetUp]
        [OneTimeTearDown]
        public void Setup()
        {
            ConnectionWrapper.Instance.Use("DELETE FROM users_groups; DELETE FROM users; DELETE FROM groups", stmt => stmt.ExecuteNonQueryAsync()).Wait();
        }
    }
}
