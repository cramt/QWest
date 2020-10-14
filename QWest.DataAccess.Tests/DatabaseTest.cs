using NUnit.Framework;
using QWest.DataAcess;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Utilities;

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
        [Test]
        public void Test() {
            byte[] value = ConnectionWrapper.CreateCommand("SELECT *  FROM (VALUES ( convert(binary(8), RAND()) + convert(binary(8), RAND()) )) t1 (col1) WHERE 1 = 1")
                .ExecuteReader()
                .ToIterator(x => x.GetSqlBinary(0).Value).FirstOrDefault();
            Console.WriteLine(Convert.ToBase64String(value));
        }

        [Test]
        public void Test2() {
            byte[] value = ConnectionWrapper.CreateCommand("DECLARE @value BINARY(50); DECLARE @temp_val BINARY(50); SET @value = NULL; SET @temp_val = NULL; WHILE @value IS NULL BEGIN SET @temp_val = convert(binary(8), RAND()) + convert(binary(8), RAND()) + convert(binary(8), RAND()) + convert(binary(8), RAND()) +convert(binary(8), RAND()) +convert(binary(8), RAND()) + convert(binary(2), RAND()); IF (SELECT COUNT(*) FROM password_reset_tokens where token = @temp_val) < 1 BEGIN SET @value = @temp_val; END END SELECT @value")
                .ExecuteReader()
                .ToIterator(x => x.GetSqlBinary(0).Value).FirstOrDefault();
            Console.WriteLine(Convert.ToBase64String(value));
        }
    }
}
