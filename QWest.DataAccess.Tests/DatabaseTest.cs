using NUnit.Framework;
using QWest.DataAcess;
using System;

namespace QWest.DataAccess.Tests {
    [TestFixture]
    public class DatabaseTest {
        [Test]
        public void NotNull() {
            Assert.NotNull(ConnectionWrapper.Instance);
        }
    }
}
