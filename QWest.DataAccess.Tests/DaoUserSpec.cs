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

namespace QWest.DataAccess.Tests {
    [TestFixture]
    class DaoUserSpec {

        [Test]
        public async Task AddsToDb() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            Assert.NotNull(user.Id);
        }

        [Test]
        public async Task GetsById() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            User fetched = await DAO.User.Get((int)user.Id);
            Assert.AreEqual(user.Email, fetched.Email);
        }

        [Test]
        public async Task GetsBySessionCookie() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            await DAO.User.SetNewSessionCookie(user);
            User fetched = await DAO.User.GetBySessionCookie(user.SessionCookie);
            Assert.AreEqual(user.Id, fetched.Id);
        }

        [Test]
        public async Task SetsNewSessionCookie() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            await DAO.User.SetNewSessionCookie(user);
            Assert.IsNotNull(user.SessionCookie);
        }

        [TestCase("an@email.com")]
        [TestCase("example@example.com")]
        [TestCase("alexandra_is@cute.com")]
        public async Task GetsFromDbByEmail(string email) {
            User user = new User("Lucca", "123456", email);
            await DAO.User.Add(user);
            User fetched = await DAO.User.GetByEmail(email);
            Assert.AreEqual(user.Id, fetched.Id);
        }

        [TestCase("Allan")]
        [TestCase("Ruth")]
        [TestCase("Alexandra")]
        public async Task GetsFromDbByUsername(string name) {
            User user = new User(name, "123456", "e@mail.com");
            await DAO.User.Add(user);
            List<User> fetched = (await DAO.User.GetByUsername(name)).ToList();
            Assert.AreEqual(1, fetched.Count);
            Assert.AreEqual(name, fetched[0].Username);
            Assert.AreEqual(user.Id, fetched[0].Id);
        }

        [Test]
        public async Task UpdatesExistingInDb() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            string newName = "LuccaHibiscus";
            user.Username = newName;
            await DAO.User.Update(user);
            user = await DAO.User.Get((int)user.Id);
            Assert.AreEqual(newName, user.Username);
        }

        [Test]
        public async Task AddProfilePictureInDb() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            Image image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("QWest.DataAccess.Tests.res.profile-picture.png"));
            MemoryStream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Jpeg);
            byte[] imageData = stream.ToArray();
            await DAO.User.UpdateProfilePicture(imageData, user);
            Assert.NotNull(user.ProfilePicture);
            Assert.AreEqual(imageData, await DAO.Image.Get((int)user.ProfilePicture));
        }

        [SetUp]
        public void Setup() {
            ConnectionWrapper.CreateCommand("DELETE FROM users").ExecuteNonQueryAsync().Wait();
        }

    }
}
