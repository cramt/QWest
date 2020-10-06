using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using static Utilities.Utilities;
using RProgressMap = Model.ProgressMap;
using RUser = Model.User;

namespace QWest.DataAcess {
    public static partial class DAO {
        public static class ProgressMap {
            public static async Task<RProgressMap> Get(RUser user) {
                if (user.Id == null) {
                    throw new ArgumentException("tried to fetch progress map of user " + user.Username + ", but they dont have an id");
                }
                RProgressMap progessMap = await GetByUserId(user.Id ?? 0);
                user.ProgressMap = progessMap;
                return progessMap;
            }
            public static async Task<RProgressMap> Get(RProgressMap map) {
                if (map.Id == null) {
                    throw new ArgumentException("tried to fetch progress map but wasnt given an id");
                }
                return await Get(map.Id ?? 0);
            }
            public static async Task<RProgressMap> Get(int id) {
                SqlCommand stmt = ConnectionWrapper.CreateCommand("select location from progress_maps inner join progress_maps_locations on progress_maps.id = progress_maps_locations.progress_maps_id where progress_maps.id = @id");
                stmt.Parameters.AddWithValue("@id", id);
                return new RProgressMap((await stmt.ExecuteReaderAsync()).ToIterator(reader => reader.GetSqlString(0).Value).ToList(), id);
            }
            public static async Task<RProgressMap> GetByUserId(int userId) {
                SqlCommand stmt = ConnectionWrapper.CreateCommand("select progress_maps.id, location from users inner join progress_maps on users.progress_maps_id = progress_maps.id inner join progress_maps_locations on progress_maps.id = progress_maps_locations.progress_maps_id where users.id = @id");
                stmt.Parameters.AddWithValue("@id", userId);
                List<(int, string)> result = (await stmt.ExecuteReaderAsync()).ToIterator(reader => (reader.GetSqlInt32(0).Value, reader.GetSqlString(1).Value)).ToList();
                if (result.Count == 0) {
                    stmt = ConnectionWrapper.CreateCommand("select progress_maps.id from users inner join progress_maps on users.progress_maps_id = progress_maps.id where users.id = @id");
                    stmt.Parameters.AddWithValue("@id", userId);
                    return new RProgressMap(new List<string>(), (await stmt.ExecuteReaderAsync()).ToIterator(reader => reader.GetSqlInt32(0).Value).First());
                }
                else {
                    return new RProgressMap(result.Select(x => x.Item2).ToList(), result[0].Item1);
                }
            }
            public static async Task Update(RProgressMap map) {
                if ((map.Additions.Count + map.Subtractions.Count) == 0) {
                    return;
                }
                string insertSql;
                string deleteSql;
                if (map.Additions.Count == 0) {
                    insertSql = "";
                }
                else {
                    insertSql = "INSERT INTO progress_maps_locations (progress_maps_id, location) VALUES "
                       + string.Join(",", map.Additions.Select((_, i) => "(@id, @add_location" + i + ")").ToArray()) + ";";
                }
                if (map.Subtractions.Count == 0) {
                    deleteSql = "";
                }
                else {
                    deleteSql = "DELETE FROM progress_maps_locations WHERE progress_maps_id = @id AND ("
                    + string.Join(" OR ", map.Subtractions.Select((_, i) => "location = @sub_location" + i).ToArray()) + ")";
                }
                SqlCommand stmt = ConnectionWrapper.CreateCommand(insertSql + deleteSql);
                stmt.Parameters.AddWithValue("@id", map.Id);
                map.Additions.Select((x, i) => stmt.Parameters.AddWithValue("@add_location" + i, x));
                map.Subtractions.Select((x, i) => stmt.Parameters.AddWithValue("@sub_location" + i, x));
                await stmt.ExecuteNonQueryAsync();
                map.Additions.Clear();
                map.Subtractions.Clear();
            }
        }
    }
}
