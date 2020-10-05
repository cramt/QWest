using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
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
                return new RProgressMap {
                    Locations = (await stmt.ExecuteReaderAsync()).ToIterator(reader => reader.GetSqlString(0).Value).ToList(),
                    Id = id
                };
            }
            public static async Task<RProgressMap> GetByUserId(int userId) {
                SqlCommand stmt = ConnectionWrapper.CreateCommand("select progress_maps.id, location from users inner join progress_maps on users.progress_maps_id = progress_maps.id inner join progress_maps_locations on progress_maps.id = progress_maps_locations.progress_maps_id where users.id = @id");
                stmt.Parameters.AddWithValue("@id", userId);
                RProgressMap map = new RProgressMap();
                List<(int, string)> result = (await stmt.ExecuteReaderAsync()).ToIterator(reader => (reader.GetSqlInt32(0).Value, reader.GetSqlString(1).Value)).ToList();
                if (result.Count == 0) {
                    stmt = ConnectionWrapper.CreateCommand("select progress_maps.id from users inner join progress_maps on users.progress_maps_id = progress_maps.id where users.id = @id");
                    stmt.Parameters.AddWithValue("@id", userId);
                    map.Id = (await stmt.ExecuteReaderAsync()).ToIterator(reader => reader.GetSqlInt32(0).Value).First();
                    return map;
                }
                else {
                    map.Id = result[0].Item1;
                    map.Locations = result.Select(x => x.Item2).ToList();
                    return map;
                }
            }
            public static async Task Update(RProgressMap newMap) {
                RProgressMap existingMap = await Get(newMap);
                ListDifferenceResult<string> diffs = ListDifference(existingMap.Locations, newMap.Locations);
                if (diffs.Count == 0) {
                    return;
                }
                string insertSql;
                string deleteSql;
                if (diffs.Additions.Count == 0) {
                    insertSql = "";
                }
                else {
                    insertSql = "INSERT INTO progress_maps_locations (progress_maps_id, location) VALUES "
                       + string.Join(",", diffs.Additions.Select((_, i) => "(@id, @add_location" + i + ")").ToArray()) + ";";
                }
                if (diffs.Subtractions.Count == 0) {
                    deleteSql = "";
                }
                else {
                    deleteSql = "DELETE FROM progress_maps_locations WHERE progress_maps_id = @id AND ("
                    + string.Join(" OR ", diffs.Subtractions.Select((_, i) => "location = @sub_location" + i).ToArray()) + ")";
                }
                SqlCommand stmt = ConnectionWrapper.CreateCommand(insertSql + deleteSql);
                stmt.Parameters.AddWithValue("@id", existingMap.Id);
                diffs.Additions.Select((x, i) => stmt.Parameters.AddWithValue("@add_location" + i, x));
                diffs.Subtractions.Select((x, i) => stmt.Parameters.AddWithValue("@sub_location" + i, x));
                await stmt.ExecuteNonQueryAsync();
            }
        }
    }
}
