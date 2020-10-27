using Model.Geographic;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using static QWest.DataAcess.DAO;
using RUser = Model.User;

namespace QWest.DataAcess.Mssql {
    class UserImpl : IUser {
        private SqlConnection _conn;
        public UserImpl(SqlConnection conn) {
            _conn = conn;
        }
        //TODO: add admin property
        public async Task Add(RUser user) {
            if (user.Id != null) {
                throw new ArgumentException("tried to add user " + user.Username + ", but they already have an id");
            }
            SqlCommand stmt = _conn.CreateCommand("INSERT INTO progress_maps DEFAULT VALUES;" +
                "INSERT INTO users (username, password_hash, email, session_cookie, progress_maps_id) VALUES " +
                "(@username, @password_hash, @email, NULL, (SELECT CAST(scope_identity() AS int))); SELECT CAST(scope_identity() AS int)");
            stmt.Parameters.AddWithValue("@username", user.Username);
            stmt.Parameters.AddWithValue("@password_hash", user.PasswordHash);
            stmt.Parameters.AddWithValue("@email", user.Email);
            user.Id = (int)await stmt.ExecuteScalarAsync();
        }
        public async Task Update(RUser user) {
            if (user.Id == null) {
                throw new ArgumentException("tried to update user " + user.Username + ", but they dont have an id");
            }
            SqlCommand stmt = _conn.CreateCommand("UPDATE users SET username = @username, password_hash = @password_hash, email = @email, session_cookie = @session_cookie, description = @description WHERE id = @id");
            stmt.Parameters.AddWithValue("@username", user.Username);
            stmt.Parameters.AddWithValue("@password_hash", user.PasswordHash);
            stmt.Parameters.AddWithValue("@id", user.Id);
            stmt.Parameters.AddWithValue("@session_cookie", (user.SessionCookie.MapValue(Convert.FromBase64String) as object).UnwrapOr(SqlBinary.Null));
            stmt.Parameters.AddWithValue("@email", user.Email);
            stmt.Parameters.AddWithValue("@description", user.Description);
            await stmt.ExecuteNonQueryAsync();
        }
        public async Task<IEnumerable<RUser>> GetByUsername(string username) {
            SqlCommand stmt = _conn.CreateCommand("SELECT id, password_hash, email, session_cookie, description, profile_picture FROM users WHERE username = @username");
            stmt.Parameters.AddWithValue("@username", username);
            List<RUser> users = (await stmt.ExecuteReaderAsync())
                .ToIterator(reader => new RUser(username, reader.GetSqlBinary(1).Value, reader.GetSqlString(2).Value, reader.GetSqlString(4).Value, reader.GetSqlBinary(3).NullableValue(), reader.GetSqlInt32(0).Value) {
                    ProfilePicture = reader.GetSqlInt32(5).NullableValue()
                })
                .ToList();
            return users;
        }
        public async Task<RUser> Get(int id) {
            SqlCommand stmt = _conn.CreateCommand("SELECT username, password_hash, email, session_cookie, description, profile_picture FROM users WHERE id = @id");
            stmt.Parameters.AddWithValue("@id", id);
            RUser user = (await stmt.ExecuteReaderAsync())
                .ToIterator(reader => new RUser(reader.GetSqlString(0).Value, reader.GetSqlBinary(1).Value, reader.GetSqlString(2).Value, reader.GetSqlString(4).Value, reader.GetSqlBinary(3).NullableValue(), id) {
                    ProfilePicture = reader.GetSqlInt32(5).NullableValue()
                })
                .FirstOrDefault();
            return user;
        }

        public async Task<RUser> GetBySessionCookie(byte[] sessionCookie) {
            SqlCommand stmt = _conn.CreateCommand("SELECT username, password_hash, email, id, description, profile_picture FROM users WHERE session_cookie = @session_cookie");
            stmt.Parameters.AddWithValue("@session_cookie", sessionCookie);
            RUser user = (await stmt.ExecuteReaderAsync())
                .ToIterator(reader => new RUser(reader.GetSqlString(0).Value, reader.GetSqlBinary(1).Value, reader.GetSqlString(2).Value, reader.GetSqlString(4).Value, sessionCookie, reader.GetSqlInt32(3).Value) {
                    ProfilePicture = reader.GetSqlInt32(5).NullableValue()
                })
                .FirstOrDefault();
            return user;
        }

        public async Task<RUser> GetBySessionCookie(string sessionCookie) {
            return await GetBySessionCookie(Convert.FromBase64String(sessionCookie));
        }

        public async Task<RUser> GetByEmail(string email) {
            SqlCommand stmt = _conn.CreateCommand("SELECT username, password_hash, id, session_cookie, description, profile_picture FROM users WHERE email = @email");
            stmt.Parameters.AddWithValue("@email", email);
            RUser user = (await stmt.ExecuteReaderAsync())
                .ToIterator(reader => new RUser(reader.GetSqlString(0).Value, reader.GetSqlBinary(1).Value, email, reader.GetSqlString(4).Value, reader.GetSqlBinary(3).NullableValue(), reader.GetSqlInt32(2).Value) {
                    ProfilePicture = reader.GetSqlInt32(5).NullableValue()
                })
                .FirstOrDefault();
            return user;
        }

        public async Task<RUser> SetNewSessionCookie(RUser user) {
            SqlCommand stmt = _conn.CreateCommand("" +
                "DECLARE @value BINARY(20); " +
                "DECLARE @temp_val BINARY(20); " +
                "" +
                "SET @value = NULL; " +
                "SET @temp_val = NULL; " +
                "" +
                "WHILE @value IS NULL " +
                "BEGIN " +
                "SET @temp_val = convert(binary(8), RAND()) + convert(binary(8), RAND()) + convert(binary(4), RAND());" +
                "IF (SELECT COUNT(*) FROM users where session_cookie = @temp_val) < 1" +
                "BEGIN " +
                "SET @value = @temp_val; " +
                "END " +
                "END " +
                "UPDATE users SET session_cookie = @value WHERE id = @user_id;" +
                "SELECT @value");
            stmt.Parameters.AddWithValue("@user_id", user.Id);
            var v = (await stmt.ExecuteReaderAsync()).ToIterator(x => x.GetSqlBinary(0).Value).FirstOrDefault();

            user.SessionCookie = Convert.ToBase64String(v);
            return user;
        }

        public async Task UpdateProfilePicture(byte[] profilePicture, RUser user) {
            if (user.Id == null) {
                throw new ArgumentException("tried to set profile pic of user " + user.Username + " but that user doesnt have an id");
            }
            user.ProfilePicture = await UpdateProfilePicture(profilePicture, (int)user.Id);
        }

        public async Task<int> UpdateProfilePicture(byte[] profilePicture, int userId) {
            SqlCommand stmt = _conn.CreateCommand("" +
                "DECLARE @image_id INT;" +
                "INSERT INTO images (image_blob) VALUES (@image_blob);" +
                "SET @image_id = CAST(scope_identity() as int);" +
                "UPDATE users SET profile_picture = @image_id WHERE id = @id;" +
                "SELECT @image_id");
            stmt.Parameters.AddWithValue("@image_blob", profilePicture);
            stmt.Parameters.AddWithValue("@id", userId);
            return (await stmt.ExecuteReaderAsync()).ToIterator(x => x.GetSqlInt32(0).Value).First();
        }
    }
}
