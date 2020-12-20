using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using NUnit.Framework;
using QWest.Apis;
using QWest.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QWest.Api.Tests {
    [TestFixture]
    public class UserControllerSpec {
        public class UserRepoMock : DAO.IUser {
            private Random _random = new Random();
            public List<User> Users { get; set; } = new List<User>();

            public async Task Add(User user) {
                Users.Add(user);
            }

            public async Task<User> Get(int id) {
                return Users.Find(x => x.Id == id);
            }

            public async Task<User> GetByEmail(string email) {
                return Users.Find(x => x.Email == email);
            }

            public Task<User> GetBySessionCookie(byte[] sessionCookie) {
                return GetBySessionCookie(Convert.ToBase64String(sessionCookie));
            }

            public async Task<User> GetBySessionCookie(string sessionCookie) {
                return Users.Find(x => x.SessionCookie == sessionCookie);
            }

            public async Task<List<User>> GetByUsername(string username) {
                return Users.Where(x => x.Username == username).ToList();
            }

            public Task<IEnumerable<User>> Search(string searchTerm)
            {
                throw new NotImplementedException();
            }

            public async Task<User> SetNewSessionCookie(User user) {
                string sessionCookie;
                do {
                    byte[] bytes = new byte[2];
                    _random.NextBytes(bytes);
                    sessionCookie = Convert.ToBase64String(bytes);
                }
                while (GetBySessionCookie(sessionCookie) == null);
                user.SessionCookie = sessionCookie;
                return user;
            }

            public async Task Update(User user) {

            }

            public async Task UpdateProfilePicture(byte[] profilePicture, User user) {

            }

            public async Task<int> UpdateProfilePicture(byte[] profilePicture, int userId) {
                return 0;
            }
        }
        [Test]
        public async Task ReturnsAUser() {
            UserRepoMock repo = new UserRepoMock();
            UserController controller = new UserController {
                UserRepo = repo
            };
            User lucca = new User("lucca", "lucca_is_lucca", "lucca@gmail.com");
            int id = 3;
            lucca.Id = id;
            repo.Users.Add(lucca);
            User expected = lucca;
            User actual = (await controller.Get(id)).Value;
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Username, actual.Username);
            Assert.AreEqual(expected.Email, actual.Email);
        }
        [Test]
        public async Task Returns404OnNoUser() {
            UserRepoMock repo = new UserRepoMock();
            UserController controller = new UserController {
                UserRepo = repo
            };
            Assert.AreEqual(404, ((await controller.Get(4)).Result as StatusCodeResult).StatusCode);
        }
        [Test]
        public async Task CanUpdateUser() {
            UserRepoMock repo = new UserRepoMock();
            UserController controller = new UserController {
                UserRepo = repo,
            };
            
            User lucca = new User("lucca", "lucca_is_lucca", "lucca@gmail.com");
            controller.Request.HttpContext.Items.Add("user", lucca);
            await controller.Update(new UserController.NewUser() {
                Description = "i am lucca"
            });
            Assert.AreEqual("i am lucca", lucca.Description);
        }

        [Test]
        public async Task Update500sOnUnauthorized() {
            UserRepoMock repo = new UserRepoMock();
            UserController controller = new UserController {
                UserRepo = repo
            };
            ActionResult result = await controller.Update(new UserController.NewUser());
            Assert.AreEqual(401, (result as StatusCodeResult).StatusCode);
        }
    }
}
