﻿using Model;
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
            Post post = await DAO.Post.Add(new PostUpload("wassup", user, now, new List<byte[]>()));
            Assert.NotNull(post.Id);
            Assert.AreEqual(now.ToString("yyyy-MM-dd-HH-mm-ss"), post.PostTime.ToString("yyyy-MM-dd-HH-mm-ss"));
            Assert.AreEqual("wassup", post.Contents);
        }
        [Test]
        public async Task GetByUser() {
            User user = new User("Lucca", "123456", "an@email.com");
            await DAO.User.Add(user);
            DateTime now = DateTime.Now;
            Post expected = await DAO.Post.Add(new PostUpload("wassup", user, now, new List<byte[]>()));
            Post fetched = (await DAO.Post.Get(user)).First();
            Assert.AreEqual(expected.Id, fetched.Id);
            Assert.AreEqual(expected.Contents, fetched.Contents);
            Assert.AreEqual(expected.PostTime, fetched.PostTime);
            Assert.AreEqual(expected.Images, fetched.Images);
            Assert.AreEqual(expected.LocationId, fetched.LocationId);
            Assert.AreEqual(expected.User.Id, fetched.User.Id);
        }

        [SetUp]
        public void Setup() {
            ConnectionWrapper.CreateCommand("DELETE FROM users").ExecuteNonQueryAsync().Wait();
            ConnectionWrapper.CreateCommand("DELETE FROM posts").ExecuteNonQueryAsync().Wait();
        }

    }
}