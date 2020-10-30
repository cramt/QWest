using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using static QWest.DataAcess.DAO;
using Model;

namespace QWest.DataAcess.Mssql {
    class ProgressMapImpl : IProgressMap {
        private ConnectionWrapper _conn;
        public ProgressMapImpl(ConnectionWrapper conn) {
            _conn = conn;
        }
        public async Task<ProgressMap> Get(User user) {
            if (user.Id == null) {
                throw new ArgumentException("tried to fetch progress map of user " + user.Username + ", but they dont have an id");
            }
            ProgressMap progessMap = await GetByUserId(user.Id ?? 0);
            user.ProgressMap = progessMap;
            return progessMap;
        }
        public async Task<ProgressMap> Get(ProgressMap map) {
            if (map.Id == null) {
                throw new ArgumentException("tried to fetch progress map but wasnt given an id");
            }
            return await Get(map.Id ?? 0);
        }
        public Task<ProgressMap> Get(int id) {
            return _conn.Use("select location from progress_maps inner join progress_maps_locations on progress_maps.id = progress_maps_locations.progress_maps_id where progress_maps.id = @id", async stmt => {
                stmt.Parameters.AddWithValue("@id", id);
                return new ProgressMap((await stmt.ExecuteReaderAsync()).ToIterator(reader => reader.GetSqlInt32(0).Value).ToList(), id);
            });
        }
        public Task<ProgressMap> GetByUserId(int userId) {
            return _conn.Use("select progress_maps.id, location from users inner join progress_maps on users.progress_maps_id = progress_maps.id inner join progress_maps_locations on progress_maps.id = progress_maps_locations.progress_maps_id where users.id = @id", async stmt => {
                stmt.Parameters.AddWithValue("@id", userId);
                List<(int, int)> result = (await stmt.ExecuteReaderAsync()).ToIterator(reader => (reader.GetSqlInt32(0).Value, reader.GetSqlInt32(1).Value)).ToList();
                if (result.Count == 0) {
                    stmt = stmt.Connection.CreateCommand("select progress_maps.id from users inner join progress_maps on users.progress_maps_id = progress_maps.id where users.id = @id");
                    stmt.Parameters.AddWithValue("@id", userId);
                    return new ProgressMap(new List<int>(), (await stmt.ExecuteReaderAsync()).ToIterator(reader => reader.GetSqlInt32(0).Value).First());
                }
                else {
                    return new ProgressMap(result.Select(x => x.Item2).ToList(), result[0].Item1);
                }
            });
        }
        public async Task Update(int id, List<int> additions, List<int> subtractions) {
            if ((additions.Count + subtractions.Count) == 0) {
                return;
            }
            string insertSql;
            string deleteSql;
            if (additions.Count == 0) {
                insertSql = "";
            }
            else {
                insertSql = "INSERT INTO progress_maps_locations (progress_maps_id, location) VALUES "
                   + string.Join(",", additions.Select((_, i) => "(@id, @add_location" + i + ")").ToArray());
            }
            if (subtractions.Count == 0) {
                deleteSql = "";
            }
            else {
                deleteSql = "DELETE FROM progress_maps_locations WHERE progress_maps_id = @id AND ("
                + string.Join(" OR ", subtractions.Select((_, i) => "location = @sub_location" + i).ToArray()) + ")";
            }
            string seperator = (insertSql.Length != 0 && deleteSql.Length != 0) ? ";" : "";
            string query = insertSql + seperator + deleteSql;
            await _conn.Use(query, stmt => {
                stmt.Parameters.AddWithValue("@id", id);
                for (int i = 0; i < additions.Count; i++) {
                    stmt.Parameters.AddWithValue("@add_location" + i, additions[i]);
                }
                for (int i = 0; i < subtractions.Count; i++) {
                    stmt.Parameters.AddWithValue("@sub_location" + i, subtractions[i]);
                }
                return stmt.ExecuteNonQueryAsync();
            });
        }
        public async Task Update(ProgressMap map) {
            if (map.Id == null) {
                throw new ArgumentException("tried to update progress map but wasnt given an id");
            }
            await Update((int)map.Id, map.Additions, map.Subtractions);
            map.Additions.Clear();
            map.Subtractions.Clear();
        }
    }
}
