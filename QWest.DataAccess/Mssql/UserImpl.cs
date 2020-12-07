using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using static QWest.DataAcess.DAO;

namespace QWest.DataAcess.Mssql {
    class UserImpl : IUser {
        [Serializable]
        internal class UserDbRep : IDbRep<User> {
            public const string SELECT_ORDER = @"
id, 
username,
password_hash, 
email, 
session_cookie, 
description, 
profile_picture
";
            [JsonProperty("id")]
            public int Id { get; set; }
            [JsonProperty("username")]
            public string Username { get; set; }
            [JsonProperty("password_hash")]
            public string PasswordHashBase64String { get; set; }
            [JsonIgnore]
            public byte[] PasswordHash { get; private set; }
            [JsonProperty("email")]
            public string Email { get; set; }
            [JsonProperty("session_cookie")]
            public string SessionCookie { get; set; }
            [JsonProperty("progress_maps_id")]
            public int? ProgressMapId { get; set; }
            [JsonProperty("description")]
            public string Description { get; set; }
            [JsonProperty("profile_picture")]
            public int? ProfilePicture { get; set; }

            public UserDbRep(SqlDataReader reader) {
                int i = 0;
                Id = reader.GetSqlInt32(i++).Value;
                Username = reader.GetSqlString(i++).Value;
                PasswordHash = reader.GetSqlBinary(i++).Value;
                Email = reader.GetSqlString(i++).Value;
                SessionCookie = reader.GetSqlBinary(i++).NullableValue().MapValue(Convert.ToBase64String);
                Description = reader.GetSqlString(i++).Value;
                ProfilePicture = reader.GetSqlInt32(i++).NullableValue();
            }
            public User ToModel() {
                return new User {
                    Id = Id,
                    Username = Username,
                    PasswordHash = PasswordHash,
                    Email = Email,
                    SessionCookie = SessionCookie,
                    Description = Description,
                    ProfilePicture = ProfilePicture,
                };
            }

            public static IEnumerable<UserDbRep> FromJson(string json) {
                return JsonConvert.DeserializeObject<List<UserDbRep>>(json).Select(x => {
                    x.PasswordHash = x.PasswordHashBase64String.Base64();
                    return x;
                }).ToList();
            }

            [JsonConstructor]
            public UserDbRep() {

            }
        }
        private ConnectionWrapper _conn;
        public UserImpl(ConnectionWrapper conn) {
            _conn = conn;
        }
        public async Task Add(User user) {
            if (user.Id != null) {
                throw new ArgumentException("tried to add user " + user.Username + ", but they already have an id");
            }
            string query = @"
INSERT INTO progress_maps DEFAULT VALUES;
INSERT INTO users (username, password_hash, email, session_cookie, progress_maps_id) VALUES 
(@username, @password_hash, @email, NULL, (SELECT CAST(scope_identity() AS int))); SELECT CAST(scope_identity() AS int)
";
            user.Id = await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@username", user.Username);
                stmt.Parameters.AddWithValue("@password_hash", user.PasswordHash);
                stmt.Parameters.AddWithValue("@email", user.Email);
                return (int)await stmt.ExecuteScalarAsync();
            });
        }
        public async Task Update(User user) {
            if (user.Id == null) {
                throw new ArgumentException("tried to update user " + user.Username + ", but they dont have an id");
            }
            string query = @"
UPDATE users 
SET username = @username, 
password_hash = @password_hash, 
email = @email, 
session_cookie = @session_cookie, 
description = @description 
WHERE id = @id";
            await _conn.Use(query, stmt => {
                stmt.Parameters.AddWithValue("@username", user.Username);
                stmt.Parameters.AddWithValue("@password_hash", user.PasswordHash);
                stmt.Parameters.AddWithValue("@id", user.Id);
                stmt.Parameters.AddWithValue("@session_cookie", (user.SessionCookie.MapValue(Convert.FromBase64String) as object).UnwrapOr(SqlBinary.Null));
                stmt.Parameters.AddWithValue("@email", user.Email);
                stmt.Parameters.AddWithValue("@description", user.Description);
                return stmt.ExecuteNonQueryAsync();
            });
        }
        public Task<List<User>> GetByUsername(string username) {
            string query = "SELECT id, password_hash, email, session_cookie, description, profile_picture FROM users WHERE username = @username";
            return _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@username", username);
                return (await stmt.ExecuteReaderAsync())
                .ToIterator(reader => new User(username, reader.GetSqlBinary(1).Value, reader.GetSqlString(2).Value, reader.GetSqlString(4).Value, reader.GetSqlBinary(3).NullableValue(), reader.GetSqlInt32(0).Value) {
                    ProfilePicture = reader.GetSqlInt32(5).NullableValue()
                })
                .ToList();
            });
        }
        public Task<User> Get(int id) {
            string query = "SELECT username, password_hash, email, session_cookie, description, profile_picture FROM users WHERE id = @id";
            return _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@id", id);
                return (await stmt.ExecuteReaderAsync())
                    .ToIterator(reader => new User(reader.GetSqlString(0).Value, reader.GetSqlBinary(1).Value, reader.GetSqlString(2).Value, reader.GetSqlString(4).Value, reader.GetSqlBinary(3).NullableValue(), id) {
                        ProfilePicture = reader.GetSqlInt32(5).NullableValue()
                    })
                    .FirstOrDefault();
            });
        }

        public Task<User> GetBySessionCookie(byte[] sessionCookie) {
            string query = "SELECT username, password_hash, email, id, description, profile_picture FROM users WHERE session_cookie = @session_cookie";
            return _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@session_cookie", sessionCookie);
                return (await stmt.ExecuteReaderAsync())
                     .ToIterator(reader => new User(reader.GetSqlString(0).Value, reader.GetSqlBinary(1).Value, reader.GetSqlString(2).Value, reader.GetSqlString(4).Value, sessionCookie, reader.GetSqlInt32(3).Value) {
                         ProfilePicture = reader.GetSqlInt32(5).NullableValue()
                     })
                     .FirstOrDefault();
            });
        }

        public async Task<User> GetBySessionCookie(string sessionCookie) {
            return await GetBySessionCookie(Convert.FromBase64String(sessionCookie));
        }

        public Task<User> GetByEmail(string email) {
            string query = "SELECT username, password_hash, id, session_cookie, description, profile_picture FROM users WHERE email = @email";
            return _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@email", email);
                return (await stmt.ExecuteReaderAsync())
                     .ToIterator(reader => new User(reader.GetSqlString(0).Value, reader.GetSqlBinary(1).Value, email, reader.GetSqlString(4).Value, reader.GetSqlBinary(3).NullableValue(), reader.GetSqlInt32(2).Value) {
                         ProfilePicture = reader.GetSqlInt32(5).NullableValue()
                     })
                     .FirstOrDefault();
            });
        }

        public async Task<User> SetNewSessionCookie(User user) {
            string query = @"
DECLARE @value BINARY(20); 
DECLARE @temp_val BINARY(20); 

SET @value = NULL; 
SET @temp_val = NULL; 
WHILE @value IS NULL 
BEGIN 
SET @temp_val = convert(binary(8), RAND()) + convert(binary(8), RAND()) + convert(binary(4), RAND());
IF (SELECT COUNT(*) FROM users where session_cookie = @temp_val) < 1
BEGIN
SET @value = @temp_val; 
END
END
UPDATE users SET session_cookie = @value WHERE id = @user_id;
SELECT @value
";
            user.SessionCookie = Convert.ToBase64String(await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@user_id", user.Id);
                return (await stmt.ExecuteReaderAsync()).ToIterator(x => x.GetSqlBinary(0).Value).FirstOrDefault();
            }));
            return user;
        }

        public async Task UpdateProfilePicture(byte[] profilePicture, User user) {
            if (user.Id == null) {
                throw new ArgumentException("tried to set profile pic of user " + user.Username + " but that user doesnt have an id");
            }
            user.ProfilePicture = await UpdateProfilePicture(profilePicture, (int)user.Id);
        }

        public Task<int> UpdateProfilePicture(byte[] profilePicture, int userId) {
            string query = @"
DECLARE @image_id INT;
INSERT INTO images (image_blob) VALUES (@image_blob);
SET @image_id = CAST(scope_identity() as int);
UPDATE users SET profile_picture = @image_id WHERE id = @id;
SELECT @image_id
";
            return _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@image_blob", profilePicture);
                stmt.Parameters.AddWithValue("@id", userId);
                return (await stmt.ExecuteReaderAsync()).ToIterator(x => x.GetSqlInt32(0).Value).First();
            });
        }
        public async Task<IEnumerable<User>> Search(string search) {
            string query = $@"
SELECT
{UserDbRep.SELECT_ORDER}
FROM
users
WHERE
username LIKE @search1
OR
username LIKE @search2
OR
username  LIKE @search3
OR
email LIKE @search1
OR
email LIKE @search2
OR
email LIKE @search3
";
            return (await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@search1", "%" + search + "%");
                stmt.Parameters.AddWithValue("@search2", search + "%");
                stmt.Parameters.AddWithValue("@search3", "%" + search);
                return (await stmt.ExecuteReaderAsync()).ToIterator(x => new UserDbRep(x));
            })).Select(x => x.ToModel());
        }
    }
}
