using NUnit.Framework;
using QWest.DataAccess;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
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

            var conn = ConnectionWrapper.Instance.Connection;
            conn.Open();
            Task.WaitAll(new Task[] { DeleteAllFunctions(conn), DeleteAllTables(conn) });
            conn.Close();

            ConnectionWrapper.Migrate = true;
            ConnectionWrapper.ResetInstance();
            Assert.NotNull(ConnectionWrapper.Instance);
        }

        private async Task DeleteAllTables(SqlConnection conn) {
            const string fetchSql = "SELECT name FROM sys.tables";
            List<string> names = conn.CreateCommand(fetchSql).ExecuteReader()
                .ToIterator(reader => reader.GetSqlString(0).Value).ToList();

            while (names.Count != 0) {
                List<string> newNames = new List<string>();
                foreach (string name in names) {
                    try {
                        await conn.CreateCommand("DROP TABLE " + name).ExecuteNonQueryAsync();
                    }
                    catch (SqlException) {
                        newNames.Add(name);
                    }
                }
                names = newNames;
            }

            names = (await conn.CreateCommand(fetchSql).ExecuteReaderAsync())
                .ToIterator(reader => reader.GetSqlString(0).Value).ToList();

            Assert.AreEqual(0, names.Count);
        }

        private async Task DeleteAllFunctions(SqlConnection conn) {
            const string fetchSql = "SELECT name FROM sys.sql_modules m INNER JOIN sys.objects o ON m.object_id=o.object_id WHERE type_desc like '%function%'";
            List<string> names = conn.CreateCommand(fetchSql).ExecuteReader()
                .ToIterator(reader => reader.GetSqlString(0).Value).ToList();

            while (names.Count != 0) {
                List<string> newNames = new List<string>();
                foreach (string name in names) {
                    try {
                        await conn.CreateCommand("DROP FUNCTION " + name).ExecuteNonQueryAsync();
                    }
                    catch (SqlException) {
                        newNames.Add(name);
                    }
                }
                names = newNames;
            }

            names = (await conn.CreateCommand(fetchSql).ExecuteReaderAsync())
                .ToIterator(reader => reader.GetSqlString(0).Value).ToList();

            Assert.AreEqual(0, names.Count);
        }
    }
}
