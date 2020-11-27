using Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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
                try {
                    Locations = reader.GetSqlString(i++).NullableValue().UnwrapOr("").Split(',').Select(int.Parse).ToList();
                }
                catch (FormatException) {
                    Locations = new List<int>();
                }
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

        public async Task<int> Create(string name, string description, List<int> members, DateTime? _creation = null) {
            DateTime creation;
            if (_creation == null) {
                creation = DateTime.Now;
            }
            else {
                creation = (DateTime)_creation;
            }
            string query = $@"
DECLARE @progress_map_id INT;
DECLARE @group_id INT;
INSERT INTO progress_maps DEFAULT VALUES;
SET @progress_map_id = CAST(scope_identity() AS int);
INSERT INTO groups
(name, creation_time, description, progress_maps_id)
VALUES
(@name, @creation_time, @description, @progress_map_id);
SET @group_id = CAST(scope_identity() AS int);
INSERT INTO users_groups
(users_id, groups_id)
VALUES
{string.Join(",", members.Select((_, i) => $"(@user_id{i}, @group_id)"))};
SELECT @group_id;
";
            return await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@name", name);
                stmt.Parameters.AddWithValue("@creation_time", creation.ToUint().ToSigned());
                stmt.Parameters.AddWithValue("@description", description);
                int j = 0;
                foreach (int member in members) {
                    stmt.Parameters.AddWithValue("@user_id" + j++, member);
                }
                return (await stmt.ExecuteReaderAsync()).ToIterator(x => x.GetSqlInt32(0).Value).First();
            });
        }

        public async Task<Group> Create(Group group) {
            if (group.Members.Count == 0) {
                throw new ArgumentException("cant create group without members");
            }
            group.Id = await Create(group.Name, group.Description, group.Members.Select(x => (int)x.Id).ToList(), group.CreationTime);
            return group;
        }

        public Task<IEnumerable<Group>> FetchUsersGroups(User user) {
            return FetchUsersGroups((int)user.Id);
        }

        public async Task<IEnumerable<Group>> FetchUsersGroups(int userId) {
            string query = @"
SELECT
id, name, creation_time, description, progress_maps_id, 
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
(
	SELECT 
	STRING_AGG(location, ',') 
	FROM 
	progress_maps 
	inner join 
	progress_maps_locations 
	ON 
	progress_maps.id = progress_maps_locations.progress_maps_id 
	WHERE progress_maps.id = groups.progress_maps_id
) AS locations
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
            })).Select(x => x.ToModel()).ToList();
        }

        public async Task UpdateMembers(int groupId, List<int> additions, List<int> subtractions) {
            string deleteQuery = "";
            if (subtractions.Count != 0) {
                deleteQuery = $@"
DELETE FROM 
users_groups
WHERE
{string.Join(" OR ", subtractions.Select((_, i) => $@"
users_id = @sub_user_id{i}
AND
groups_id = @group_id
"))}
";
            }
            string addQuery = "";
            if (additions.Count != 0) {
                addQuery = $@"
INSERT INTO 
users_groups
(users_id, groups_id)
VALUES
{string.Join(",", subtractions.Select((_, i) => $@"
(@add_user_id{i}, @group_id)
"))}
";
                string seperator = "";
                if (addQuery != "" && deleteQuery != "") {
                    seperator = ";";
                }
                string query = addQuery + seperator + deleteQuery;
                await _conn.Use(query, async stmt => {
                    int i = 0;
                    foreach (int add in additions) {
                        stmt.Parameters.AddWithValue("@add_user_id" + i++, add);
                    }
                    i = 0;
                    foreach (int sub in subtractions) {
                        stmt.Parameters.AddWithValue("@sub_user_id" + i++, sub);
                    }
                    stmt.Parameters.AddWithValue("@group_id", groupId);
                    return await stmt.ExecuteNonQueryAsync();
                });
            }
        }

        public async Task Update(int id, string name, string description) {
            string query = @"
UPDATE groups
SET
name = @name,
description = @description
WHERE
id = @id
";

            await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@name", name);
                stmt.Parameters.AddWithValue("@description", description);
                stmt.Parameters.AddWithValue("@id", id);
                await stmt.ExecuteNonQueryAsync();
                return true;
            });
        }

        public Task Update(Group group) {
            return Update((int)group.Id, group.Name, group.Description);
        }

        public async Task<bool> IsMember(int groupId, int userId) {
            string query = @"
SELECT * FROM users_groups
WHERE
users_id = @user_id
AND
groups_id = @group_id
";
            return (await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@user_id", userId);
                stmt.Parameters.AddWithValue("@group_id", groupId);
                return (await stmt.ExecuteReaderAsync()).ToIterator(_ => true);
            })).FirstOrDefault();
        }

        public Task<bool> IsMember(int groupId, User user) {
            return IsMember(groupId, (int)user.Id);
        }

        public Task<bool> IsMember(Group group, int userId) {
            return IsMember((int)group.Id, userId);
        }

        public Task<bool> IsMember(Group group, User user) {
            return IsMember((int)group.Id, user);
        }

        public async Task<Group> Get(int id) {
            string query = @"
SELECT
id, name, creation_time, description, progress_maps_id, 
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
(
	SELECT 
	STRING_AGG(location, ',') 
	FROM 
	progress_maps 
	inner join 
	progress_maps_locations 
	ON 
	progress_maps.id = progress_maps_locations.progress_maps_id 
	WHERE progress_maps.id = groups.progress_maps_id
)
FROM
groups
INNER JOIN
users_groups
ON
groups.id = users_groups.groups_id
WHERE groups.id = @id
";
            return (await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@id", id);
                return (await stmt.ExecuteReaderAsync()).ToIterator(x => new GroupDbRep(x)).FirstOrDefault();
            })).MapValue(x => x.ToModel());
        }
    }
}
