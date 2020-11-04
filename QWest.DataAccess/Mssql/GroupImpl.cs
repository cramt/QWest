using Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using static QWest.DataAcess.Mssql.UserImpl;

namespace QWest.DataAcess.Mssql {
    public class GroupImpl : DAO.IGroup {
        [Serializable]
        internal class GroupDbRep : IDbRep<Group> {
            public int Id { get; }
            public string Name { get; }
            public int CreationTime { get; }
            public string Description { get; }
            public int ProgressMapId { get; }
            public IEnumerable<UserDbRep> Members { get; }
            public IEnumerable<int> Locations { get; }
            public GroupDbRep(SqlDataReader reader) {
                int i = 0;
                Id = reader.GetSqlInt32(i++).Value;
                Name = reader.GetSqlString(i++).Value;
                CreationTime = reader.GetSqlInt32(i++).Value;
                Description = reader.GetSqlString(i++).Value;
                ProgressMapId = reader.GetSqlInt32(i++).Value;
                Members = UserDbRep.FromJson(reader.GetSqlString(i++).Value);
                Locations = reader.GetSqlString(i++).Value.Split(',').Select(int.Parse).ToList();
            }
            public Group ToModel() {
                return new Group {
                    Id = Id,
                    Name = Name,
                    CreationTime = CreationTime.ToUnsigned().ToDate(),
                    Description = Description,
                    ProgressMap = new ProgressMap(Locations.ToList(), ProgressMapId),
                    Members = Members.Select(x => x.ToModel()).ToList()
                };
            }
        }

        private ConnectionWrapper _conn;
        public GroupImpl(ConnectionWrapper conn) {
            _conn = conn;
        }

        public async Task AddMember(Group group, User member) {
            string query = @"
INSERT INTO 
users_groups
(users_id, groups_id)
VALUES
(@user_id, @group_id)
";
            await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@user_id", member.Id);
                stmt.Parameters.AddWithValue("@group_id", group.Id);
                await stmt.ExecuteNonQueryAsync();
                return true;
            });
            if (!group.Members.Contains(member)) {
                group.Members.Add(member);
            }
        }

        public async Task<Group> Create(Group group) {
            if (group.Members.Count == 0) {
                throw new ArgumentException("cant create group without members");
            }
            string query = $@"
DECLARE @progress_map_id INT;
DECLARE @group_id INT;
INSERT INTO progress_maps DEFAULT VALUES;
SET @progress_map_id = SELECT CAST(scope_identity() AS int);
INSERT INTO groups
(name, creation_time, description, progress_maps_id)
VALUES
(@name, @creation_time, @description, @progress_map_id);
SET @group_id = SELECT CAST(scope_identity() AS int);
INSERT INTO users_groups
(users_id, groups_id)
VALUES
{string.Join(",", group.Members.Select((_, i) => $"(@user_id{i}, @group_id)"))};
SELECT @group_id;
";
            int id = await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@name", group.Name);
                stmt.Parameters.AddWithValue("@creation_time", group.CreationTime.ToUint().ToSigned());
                stmt.Parameters.AddWithValue("@description", group.Description);
                int j = 0;
                foreach (User member in group.Members) {
                    stmt.Parameters.AddWithValue("@user_id" + j++, member.Id);
                }
                return (await stmt.ExecuteReaderAsync()).ToIterator(x => x.GetSqlInt32(0).Value).First();
            });
            group.Id = id;
            return group;
        }

        public Task<IEnumerable<Group>> FetchUsers(User user) {
            return FetchUsers((int)user.Id);
        }

        public async Task<IEnumerable<Group>> FetchUsers(int userId) {
            string query = @"
SELECT
id, name, creation_time, description, 
(
	SELECT 
	id, username, password_hash, email, session_cookie, progress_maps_id, description, profile_picture 
	FROM 
	users 
	INNER JOIN 
	users_groups 
	ON 
	users.id = users_groups.users_id 
	WHERE users_groups.groups_id = groups.id FOR JSON PATH
) AS members,
IsNull((
	SELECT 
	STRING_AGG(location, ',') 
	FROM 
	progress_maps 
	inner join 
	progress_maps_locations 
	ON 
	progress_maps.id = progress_maps_locations.progress_maps_id 
	WHERE progress_maps.id = groups.progress_maps_id
), '') AS locations
FROM groups
INNER JOIN
users_groups
ON
groups.id = users_groups.groups_id
WHERE users_groups.users_id = @user_id
";
            return (await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@user_id", userId);
                return (await stmt.ExecuteReaderAsync()).ToIterator(x => new GroupDbRep(x));
            })).Select(x=>x.ToModel()).ToList();
        }

        public async Task RemoveMember(Group group, User member) {
            string query = @"
DELETE FROM 
users_groups
WHERE
users_id = @user_id
AND
groups_id = @group_id
";
            await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@user_id", member.Id);
                stmt.Parameters.AddWithValue("@group_id", group.Id);
                await stmt.ExecuteNonQueryAsync();
                return true;
            });
            if (group.Members.Contains(member)) {
                group.Members.Remove(member);
            }
        }

        public async Task Update(Group group) {
            string query = @"
UPDATE groups
SET
name = @name,
description = @description
WHERE
id = @id
";

            await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@name", group.Name);
                stmt.Parameters.AddWithValue("@description", group.Description);
                stmt.Parameters.AddWithValue("@id", group.Id);
                await stmt.ExecuteNonQueryAsync();
                return true;
            });
        }
    }
}
