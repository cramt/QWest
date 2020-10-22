using NUnit.Framework;
using QWest.DataAcess;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Utilities;

namespace QWest.DataAccess.Tests {
    [TestFixture]
    public class ConnectionWrapperSpec {
        [Test]
        public void NotNull() {
            Assert.NotNull(ConnectionWrapper.Instance);
        }
        [Test]
        public void CorrectOrder() {
            var scripts = ConnectionWrapper.GetScripts().ToList();
            Assert.AreEqual(scripts[0].numeric, 1);
        }
        [Test]
        public void MigrationsSucceed() {
            ConnectionWrapper.Migrate = false;
            List<string> names = ConnectionWrapper.CreateCommand("SELECT name FROM sys.tables").ExecuteReader()
                .ToIterator(reader => reader.GetSqlString(0).Value).ToList();

            while (names.Count != 0) {
                List<string> newNames = new List<string>();
                foreach(string name in names) {
                    try {
                        ConnectionWrapper.CreateCommand("DROP TABLE " + name).ExecuteNonQuery();
                    }
                    catch (SqlException) {
                        newNames.Add(name);
                    }
                }
                names = newNames;
            }

            names = ConnectionWrapper.CreateCommand("SELECT name FROM sys.tables").ExecuteReader()
                .ToIterator(reader => reader.GetSqlString(0).Value).ToList();

            Assert.AreEqual(0, names.Count);

            ConnectionWrapper.Migrate = true;
            ConnectionWrapper.ResetInstance();
            Assert.NotNull(ConnectionWrapper.Instance);
        }
    }
}
