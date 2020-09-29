using NUnit.Framework;
using QWest.DataAcess;
using System;
using System.Linq;
using System.Net;

namespace QWest.DataAccess.Tests {
    [TestFixture]
    public class DatabaseTest {
        [Test]
        public void NotNull() {
            Assert.NotNull(ConnectionWrapper.Instance);
        }
        [Test]
        public void CorrectOrder() {
            var scripts = new ConnectionWrapper.ScriptProvider().GetScripts(null).ToList();
            Assert.True(scripts[0].Name.EndsWith("1.sql"));
        }
    }
}
