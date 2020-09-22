using NUnit.Framework;
using System;

namespace Model.Tests {
    [TestFixture]
    public class UserTest {
        [Test]
        public void PasswordVerification() {
            var user = new User("test user", "12345678");
            Assert.True(user.VeryifyPassword("12345678"));
        }
    }
}
