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
    class DaoPostSpec {
        [Test]
        public async Task AddsToDb() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            DateTime now = DateTime.Now;
            Post post = await DAO.Post.Add("wassup", user, new List<byte[]>(), null);
            Assert.NotNull(post.Id);
            Assert.AreEqual(now.ToString("yyyy-MM-dd-HH-mm-ss"), post.PostTime.ToString("yyyy-MM-dd-HH-mm-ss"));
            Assert.AreEqual("wassup", post.Contents);
        }
        [Test]
        public async Task AddsToDbWithImage() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            DateTime now = DateTime.Now;
            Image image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("QWest.DataAccess.Tests.res.profile-picture.png"));
            MemoryStream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Jpeg);
            byte[] imageData = stream.ToArray();
            Post post = await DAO.Post.Add("wassup", user, new List<byte[]> { imageData }, null);
            Assert.NotNull(post.Id);
            Assert.AreEqual(now.ToString("yyyy-MM-dd-HH-mm-ss"), post.PostTime.ToString("yyyy-MM-dd-HH-mm-ss"));
            Assert.AreEqual("wassup", post.Contents);
        }
        [Test]
        public async Task AddsToDbWithLocation() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            DateTime now = DateTime.Now;
            Post post = await DAO.Post.Add("wassup", user, new List<byte[]> { }, (await DAO.Geography.GetCountryByAlpha2("DK")).Id);
            Assert.NotNull(post.Id);
            Assert.AreEqual(now.ToString("yyyy-MM-dd-HH-mm-ss"), post.PostTime.ToString("yyyy-MM-dd-HH-mm-ss"));
            Assert.AreEqual("wassup", post.Contents);
        }
        [Test]
        public async Task GetByUser() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            Post expected = await DAO.Post.Add("wassup", user, new List<byte[]>(), null);
            Post fetched = (await DAO.Post.Get(user)).First();
            Assert.AreEqual(expected.Id, fetched.Id);
            Assert.AreEqual(expected.Contents, fetched.Contents);
            Assert.AreEqual(expected.PostTime, fetched.PostTime);
            Assert.AreEqual(expected.Images, fetched.Images);
            Assert.AreEqual(expected.Location, fetched.Location);
            Assert.AreEqual(expected.User.Id, fetched.User.Id);
        }

        [SetUp]
        public void Setup() {
            ConnectionWrapper.Instance.Use("DELETE FROM images; DELETE FROM users_friendships; DELETE FROM users_friendship_requests; DELETE FROM users; DELETE FROM posts", stmt => stmt.ExecuteNonQueryAsync()).Wait();
        }
    }
}
