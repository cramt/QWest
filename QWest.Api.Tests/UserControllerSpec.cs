using Model;
using Newtonsoft.Json;
using NUnit.Framework;
using QWest.Apis;
using QWest.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http;

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
            controller.Request = new HttpRequestMessage();
            controller.Request.SetConfiguration(Startup.GlobalConfig);
            User lucca = new User("lucca", "lucca_is_lucca", "lucca@gmail.com");
            int id = 3;
            lucca.Id = id;
            repo.Users.Add(lucca);
            User expected = lucca;
            string json = await (await controller.Get(id)).Content.ReadAsStringAsync();
            User actual = JsonConvert.DeserializeObject<User>(json);
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
            Assert.AreEqual(HttpStatusCode.NotFound, (await controller.Get(4)).StatusCode);
        }
        [Test]
        public async Task CanUpdateUser() {
            UserRepoMock repo = new UserRepoMock();
            UserController controller = new UserController {
                UserRepo = repo
            };
            controller.Request = new HttpRequestMessage();
            controller.Request.SetConfiguration(Startup.GlobalConfig);
            User lucca = new User("lucca", "lucca_is_lucca", "lucca@gmail.com");
            controller.Request.SetOwinContext(new CustomOwinContext());
            controller.Request.GetOwinContext().Set("user", lucca);
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
            controller.Request = new HttpRequestMessage();
            controller.Request.SetConfiguration(Startup.GlobalConfig);
            controller.Request.SetOwinContext(new CustomOwinContext());
            HttpResponseMessage message = await controller.Update(new UserController.NewUser());
            Assert.AreEqual(HttpStatusCode.Unauthorized, message.StatusCode);
        }
    }
}
