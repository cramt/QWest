using NUnit.Framework;

namespace Config.Test {
    [TestFixture]
    public class ConfigTest {
        [Test]
        public void NotNull() {
            Assert.NotNull(Config.Instance);
        }
    }
}
