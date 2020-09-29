using NUnit.Framework;
using QWest.DataAcess;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

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
        [Test]
        public void Migration() {
            List<string> names = new List<string>();
            using (SqlDataReader reader = ConnectionWrapper.CreateCommand("SELECT name FROM sys.tables").ExecuteReader()) {
                while (reader.Read()) {
                    names.Append(reader.GetSqlString(0).Value);
                }
            }
            if (names.Count != 0) {
                ConnectionWrapper.CreateCommand("DROP TABLE " + string.Join(", ", names.ToArray())).ExecuteNonQuery();
                ConnectionWrapper.ResetInstance();
            }
            Assert.NotNull(ConnectionWrapper.Instance);
        }
    }
}
