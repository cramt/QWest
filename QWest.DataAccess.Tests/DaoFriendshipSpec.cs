using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using NUnit.Framework;
using QWest.DataAcess;

namespace QWest.DataAccess.Tests {
    [TestFixture]
    class DaoFriendshipSpec {
        [Test]
        public async Task AddsFriendRequest() {
            User userTo = new User("luccaflower", "123456", "a@mail.com");
            User userFrom = new User("cramt", "123456", "b@mail.com");
            await DAO.User.Add(userTo);
            await DAO.User.Add(userFrom);

            await DAO.Friendship.AddFriendRequest(userFrom, userTo);

            List<User> friendRequests = await DAO.Friendship.GetFriendRequests(userTo);

            Assert.IsNotEmpty(friendRequests);
        }

        [Test]
        public async Task GetsIncomingFriendRequests() {
            User userTo = new User("luccaflower", "123456", "a@mail.com");
            User userFrom = new User("cramt", "123456", "b@mail.com");
            await DAO.User.Add(userTo);
            await DAO.User.Add(userFrom);

            await DAO.Friendship.AddFriendRequest(userFrom, userTo);
            List<User> friendRequests = await DAO.Friendship.GetFriendRequests(userTo);

            Assert.AreEqual(userFrom.Id, friendRequests[0].Id);
        }

        [Test]
        public async Task AcceptsFriendRequest() {
            User userTo = new User("luccaflower", "123456", "a@mail.com");
            User userFrom = new User("cramt", "123456", "b@mail.com");
            await DAO.User.Add(userTo);
            await DAO.User.Add(userFrom);

            Assert.True(await DAO.Friendship.AddFriendRequest(userFrom, userTo));
            await DAO.Friendship.AcceptFriendRequest(userFrom, userTo);

            List<User> friends = await DAO.Friendship.GetUsersFriends(userTo);
            Assert.IsNotEmpty(friends);
        }

        [Test]
        public async Task GetsUsersFriends() {
            User userTo = new User("luccaflower", "123456", "a@mail.com");
            User userFrom = new User("cramt", "123456", "b@mail.com");
            await DAO.User.Add(userTo);
            await DAO.User.Add(userFrom);

            await DAO.Friendship.AddFriendRequest(userFrom, userTo);
            await DAO.Friendship.AcceptFriendRequest(userFrom, userTo);

            List<User> friends = await DAO.Friendship.GetUsersFriends(userTo);
            Assert.AreEqual(userFrom.Id, friends[0].Id);
        }

        [Test]
        public async Task DeclinesFriendRequest() {
            //TODO: the option to decline friend requests
            //Assert.Fail();
        }

        [SetUp]
        public void Setup() {
            ConnectionWrapper.Instance.Use("DELETE FROM users_friendship_requests", stmt => stmt.ExecuteNonQueryAsync()).Wait();
            ConnectionWrapper.Instance.Use("DELETE FROM users_friendships", stmt => stmt.ExecuteNonQueryAsync()).Wait();
            ConnectionWrapper.Instance.Use("DELETE FROM users", stmt => stmt.ExecuteNonQueryAsync()).Wait();
        }

        [OneTimeTearDown]
        public void TearDownEnd() {
            Setup();
        }
    }
}
