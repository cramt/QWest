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
            var scripts = new ConnectionWrapper.ScriptProvider().GetScripts(null).ToList();
            Assert.True(scripts[0].Name.EndsWith("1.sql"));
        }
        [Test]
        public void MigrationsSucceed() {
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
            ConnectionWrapper.ResetInstance();
            Assert.NotNull(ConnectionWrapper.Instance);
        }
    }
}
