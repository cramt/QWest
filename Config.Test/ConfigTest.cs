using NUnit.Framework;
using System;
using System.IO;

namespace Config.Test {
    [TestFixture]
    public class ConfigTest {
        [Test]
        public void NotNull() {
            Assert.NotNull(Config.Instance);
        }
    }
}
