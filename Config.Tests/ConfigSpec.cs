using NUnit.Framework;

namespace Config.Test {
    [TestFixture]
    public class ConfigSpec {
        [Test]
        public void NotNull() {
            Assert.NotNull(Config.Instance);
            Assert.NotNull(Config.Instance.ApiPort);
            Assert.NotNull(Config.Instance.DatabaseConnectionString);
            Assert.NotNull(Config.Instance.EmailPort);
            Assert.NotNull(Config.Instance.Ports);
            Assert.NotNull(Config.Instance.ServePort);
            Assert.NotNull(Config.Instance.StartDate);
            Assert.NotNull(Config.Instance.StartYear);
        }
    }
}
