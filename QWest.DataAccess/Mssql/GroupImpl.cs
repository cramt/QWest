using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace QWest.DataAcess.Mssql {
    public class GroupImpl : DAO.IGroup {
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
SET @progress_map_id = SELECT CAST(scope_identity() AS int);
INSERT INTO progress_maps DEFAULT VALUES;
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
