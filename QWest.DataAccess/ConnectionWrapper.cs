using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;
using Newtonsoft.Json;
using Model.Geographic;

namespace QWest.DataAcess {
    public class ConnectionWrapper {
        private static SqlConnection _instance;
        public static void ResetInstance() {
            _instance = null;
        }
        public static SqlCommand CreateCommand(string command) {
            return new SqlCommand(null, Instance) {
                CommandText = command
            };
        }
        public static SqlConnection Instance {
            get {
                if (_instance == null) {
                    _instance = CreateConnection();
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

        private static async Task ApplyMigration(SqlConnection conn) {
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
                List<Country> countries = JsonConvert.DeserializeObject<List<Country>>(File.ReadAllText(Utilities.Utilities.SolutionLocation + "\\QWest.DataAccess\\res\\geopolitical_location_backup.json"));
                await DAO.Geography.InsertBackup(countries, conn);
            }
        }

        private static SqlConnection CreateConnection() {
            string connString = Config.Config.Instance.DatabaseConnectionString;
            var conn = new SqlConnection(connString);
            conn.Open();
            if (Migrate) {
                ApplyMigration(conn).Wait();
            }
            return conn;
        }
    }
}
