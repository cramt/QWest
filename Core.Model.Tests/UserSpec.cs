using NUnit.Framework;

namespace Model.Tests {
    [TestFixture]
    public class UserTest {
        [Test]
        public void PasswordVerification() {
            var user = new User("test user", "12345678", "email");
            Assert.True(user.VerifyPassword("12345678"));
        }
    }
}
