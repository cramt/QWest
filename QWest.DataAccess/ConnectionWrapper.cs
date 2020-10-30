using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using Utilities;
using Model.Geographic;

namespace QWest.DataAcess {
    public class ConnectionWrapper {
        private static ConnectionWrapper _instance;
        public SqlConnection Connection { get { return new SqlConnection(Config.Config.Instance.DatabaseConnectionString); } }
        private ConnectionWrapper() {
            var conn = Connection;
            conn.Open();
            if (Migrate) {
                ApplyMigration(conn).Wait();
            }
            conn.Close();
        }
        public static void ResetInstance() {
            _instance = null;
        }
        public static SqlCommand CreateCommand(string command) {
            return Instance.Connection.CreateCommand(command);
        }
        public static ConnectionWrapper Instance {
            get {
                if (_instance == null) {
                    _instance = new ConnectionWrapper();
                }
                return _instance;
            }
        }

        public static bool Migrate { get; set; } = true;

        public static IEnumerable<(int numeric, string text)> GetScripts() {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(name => name.EndsWith(".sql"))
                .Select(name => {
                    Stack<string> stack = new Stack<string>(name.Split('.'));
                    stack.Pop();
                    int? numeric = null;
                    if (int.TryParse(stack.Pop(), out int i)) {
                        numeric = i;
                    }
                    return (name, numeric);
                })
                .Where(x => x.numeric != null)
                .Select(x => (x.name, numeric: (int)x.numeric))
                .OrderBy(x => x.numeric)
                .Select(x => {
                    string text = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(x.name)).ReadToEnd();
                    return (x.numeric, text);
                });
        }

        private async Task ApplyMigration(SqlConnection conn) {
            SqlCommand stmt = conn.CreateCommand();
            stmt.CommandText = "select schema_version from info";
            int schemaVersion = 0;
            try {
                schemaVersion = (await stmt.ExecuteReaderAsync()).ToIterator(x => x.GetSqlInt32(0).Value).FirstOrDefault();
            }
            catch (Exception) {
                stmt = conn.CreateCommand();
                stmt.CommandText = "CREATE TABLE info (schema_version INT NOT NULL)";
                await stmt.ExecuteNonQueryAsync();
            }
            foreach ((int numeric, string text) in GetScripts().Where(x => x.numeric > schemaVersion)) {
                stmt = conn.CreateCommand();
                stmt.CommandText = text;
                Console.WriteLine("migrating: " + numeric);
                await stmt.ExecuteNonQueryAsync();
                schemaVersion = numeric;
            }
            stmt = conn.CreateCommand();
            stmt.CommandText = "delete from info; insert into info (schema_version) values (@schema_version)";
            stmt.Parameters.AddWithValue("@schema_version", schemaVersion);
            await stmt.ExecuteNonQueryAsync();

            stmt = conn.CreateCommand();
            stmt.CommandText = "SELECT COUNT(*) FROM geopolitical_location";
            bool applyGeopoliticalLocationBackup = false;
            try {
                applyGeopoliticalLocationBackup = (await stmt.ExecuteReaderAsync()).ToIterator(reader => reader.GetSqlInt32(0).Value == 0).First();
            }
            catch (Exception) { }
            if (applyGeopoliticalLocationBackup) {
                List<Country> countries = GeopoliticalLocation.Parse(File.ReadAllText(Utilities.Utilities.SolutionLocation + "\\QWest.DataAccess\\res\\geopolitical_location_backup.json"));
                await new Mssql.GeographyImpl(null).InsertBackup(countries, conn);
            }
        }

        public async Task<T> Use<T>(string query, Func<SqlCommand, Task<T>> func) {
            var conn = Connection;
            conn.Open();
            T result = await func(conn.CreateCommand(query));
            conn.Close();
            return result;
        }

        public async Task<T> Use<T>(Func<SqlCommand, Task<T>> func) {
            var conn = Connection;
            conn.Open();
            T result = await func(conn.CreateCommand());
            conn.Close();
            return result;
        }

        public async Task Use<T>(string query, Func<SqlCommand, Task> func) {
            var conn = Connection;
            conn.Open();
            await func(conn.CreateCommand(query));
            conn.Close();
        }

        public async Task Use<T>(Func<SqlCommand, Task> func) {
            var conn = Connection;
            conn.Open();
            await func(conn.CreateCommand());
            conn.Close();
        }
    }
}
