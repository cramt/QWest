using Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace QWest.DataAcess.Mssql {
    public class FriendshipImpl : DAO.IFriendship {
        private ConnectionWrapper _conn;
        public FriendshipImpl(ConnectionWrapper conn) {
            _conn = conn;
        }

        public async Task<List<User>> GetUsersFriends(User user) {
            List<User> result = await GetUsersFriends((int)user.Id);
            user.Friends = result;
            return result;
        }

        public Task<List<User>> GetUsersFriends(int userId) {
            string query = @"
SELECT
id, username, password_hash, email, session_cookie, description, profile_picture 
FROM users 
INNER JOIN users_friendships 
ON left_user_id = users.id 
WHERE right_user_id = @user_id
";
            return _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@user_id", userId);
                return (await stmt.ExecuteReaderAsync())
                    .ToIterator(reader =>
                    new User(reader.GetSqlString(1).Value, reader.GetSqlBinary(2).Value, reader.GetSqlString(3).Value, reader.GetSqlString(5).Value, reader.GetSqlBinary(4).NullableValue(), reader.GetSqlInt32(0).Value)).ToList();
            });
        }
        public Task<bool> AddFriendRequest(User from, User to) {
            return AddFriendRequest((int)from.Id, (int)to.Id);
        }

        public Task<bool> AddFriendRequest(User from, int to) {
            return AddFriendRequest((int)from.Id, to);
        }

        public Task<bool> AddFriendRequest(int from, User to) {
            return AddFriendRequest(from, (int)to.Id);
        }

        public async Task<bool> AddFriendRequest(int from, int to) {
            string query = @"
DECLARE @valid BIT;
SET @valid = 0;
IF (
    SELECT 
    COUNT(*) 
    FROM 
    users_friendship_requests 
    WHERE 
    from_user_id = @from_user_id 
    AND
    to_user_id = @to_user_id
) = 0
BEGIN
    INSERT INTO users_friendship_requests
    (from_user_id, to_user_id)
    VALUES
    (@from_user_id, @to_user_id);
    SET @valid = 1;
END
SELECT @valid;
";
            return await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@to_user_id", to);
                stmt.Parameters.AddWithValue("@from_user_id", from);
                return (await stmt.ExecuteReaderAsync()).ToIterator(x => x.GetSqlBoolean(0).Value).First();
            });
        }
        public Task<List<User>> GetFriendRequests(User user) {
            return GetFriendRequests((int)user.Id);
        }
        public Task<List<User>> GetFriendRequests(int userId) {
            string query = @"
SELECT 
id, username, password_hash, email, session_cookie, description, profile_picture 
FROM users 
INNER JOIN users_friendship_requests 
ON from_user_id = users.id 
WHERE to_user_id = @user_id
";
            return _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@user_id", userId);
                return (await stmt.ExecuteReaderAsync())
                    .ToIterator(reader =>
                    new User(reader.GetSqlString(1).Value, reader.GetSqlBinary(2).Value, reader.GetSqlString(3).Value, reader.GetSqlString(5).Value, reader.GetSqlBinary(4).NullableValue(), reader.GetSqlInt32(0).Value)).ToList();
            });
        }
        public Task<bool> AcceptFriendRequest(User from, User to) {
            return AcceptFriendRequest((int)from.Id, (int)to.Id);
        }

        public Task<bool> AcceptFriendRequest(User from, int to) {
            return AcceptFriendRequest((int)from.Id, to);
        }

        public Task<bool> AcceptFriendRequest(int from, User to) {
            return AcceptFriendRequest(from, (int)to.Id);
        }

        public async Task<bool> AcceptFriendRequest(int from, int to) {
            string query = @"
DECLARE @valid BIT;
SET @valid = 0;
DECLARE @t TABLE(id INT);

DELETE FROM users_friendship_requests 
OUTPUT Deleted.from_user_id INTO @t
WHERE 
from_user_id = @from_user_id
AND
to_user_id = @to_user_id;

IF (SELECT COUNT(*) FROM @t) != 0
BEGIN
    INSERT INTO users_friendships
    (left_user_id, right_user_id)
    VALUES
    (@to_user_id, @from_user_id),
    (@from_user_id, @to_user_id);
    SET @valid = 1;
END
SELECT @valid;
";
            return await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@to_user_id", to);
                stmt.Parameters.AddWithValue("@from_user_id", from);
                return (await stmt.ExecuteReaderAsync()).ToIterator(x => x.GetSqlBoolean(0).Value).First();
            });
        }
    }
}
