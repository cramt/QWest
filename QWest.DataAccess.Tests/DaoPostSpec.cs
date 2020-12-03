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
    class DaoPostSpec : DaoPostSpecSetupAndTearDown {
        [Test]
        public async Task AddsToDb() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            DateTime now = DateTime.Now;
            Post post = await DAO.Post.Add("wassup", user, new List<byte[]>(), null);
            Assert.NotNull(post.Id);
            Assert.AreEqual(now.ToString("yyyy-MM-dd-HH-mm"), post.PostTime.ToString("yyyy-MM-dd-HH-mm"));
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
            Assert.AreEqual(now.ToString("yyyy-MM-dd-HH-mm"), post.PostTime.ToString("yyyy-MM-dd-HH-mm"));
            Assert.AreEqual("wassup", post.Contents);
        }
        [Test]
        public async Task AddsToDbWithLocation() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            DateTime now = DateTime.Now;
            Post post = await DAO.Post.Add("wassup", user, new List<byte[]> { }, (await DAO.Geography.GetCountryByAlpha2("DK")).Id);
            Assert.NotNull(post.Id);
            Assert.AreEqual(now.ToString("yyyy-MM-dd-HH-mm"), post.PostTime.ToString("yyyy-MM-dd-HH-mm"));
            Assert.AreEqual("wassup", post.Contents);
        }

        [Test]
        public async Task AddsToDbWithGroupAuthor()
        {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            Group group = await DAO.Group.Create(new Group("lucca's friends", DateTime.Now, "we're lucca's friends", null, new User[] { user }));
            Post post = await DAO.Post.Add("wassup", group, new List<byte[]>(), null);
            Assert.NotNull(post.Id);
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
            Assert.AreEqual(expected.UserAuthor.Id, fetched.UserAuthor.Id);
        }

        [TestFixture]
        public class GetsUserFeed : DaoPostSpecSetupAndTearDown {
            [Test]
            public async Task WhenAmountFetchedIsLessThanUserPostAmount() {
                User user = new User("Lucca", "123456", "an@email.com");
                await DAO.User.Add(user);
                
                for (int i = 0; i < 5; i++) {
                    await DAO.Post.Add("wassup" + i, user, new List<byte[]>(), null);
                }
                List<Post> posts = (await DAO.Post.GetFeed(user, 4, 0)).ToList();
                Assert.AreEqual(4, posts.Count);
            }
            [Test]
            public async Task WhenAmountFetchedExceedsUserPostAmount() {
                User user = new User("Lucca", "123456", "an@email.com");
                await DAO.User.Add(user);

                for (int i = 0; i < 5; i++) {
                    await DAO.Post.Add("wassup" + i, user, new List<byte[]>(), null);
                }
                List<Post> postsWithNoOffset = (await DAO.Post.GetFeed(user, 7, 0)).ToList();
                List<Post> postsWithOffset = (await DAO.Post.GetFeed(user, 7, 3)).ToList();

                Assert.AreEqual(5, postsWithNoOffset.Count);
                Assert.AreEqual(2, postsWithOffset.Count);
            }
        }
        [Test]
        public async Task GetsGroupFeed()
        {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            Group group = await DAO.Group.Create(new Group("lucca's friends", DateTime.Now, "we're lucca's friends", null, new User[] { user }));
            for (int i = 0; i < 5; i++)
            {
                await DAO.Post.Add("wassup" + i, group, new List<byte[]>(), null);
            }
            List<Post> posts = (await DAO.Post.GetGroupFeed(group)).ToList();
            Assert.AreEqual(5, posts.Count);
        }
    }
    public class DaoPostSpecSetupAndTearDown {
        [SetUp]
        [OneTimeTearDown]
        public void Setup() {
            ConnectionWrapper.Instance.Use("DELETE FROM images; DELETE FROM users_friendships; DELETE FROM users_friendship_requests; DELETE FROM users_groups; DELETE FROM posts; DELETE FROM groups; DELETE FROM users", stmt => stmt.ExecuteNonQueryAsync()).Wait();
        }
    }
}
