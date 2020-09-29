using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RUser = Model.User;

namespace QWest.DataAcess {
    public static partial class DAO {
        public static class User {
            public static async Task Add(RUser user) {
                if (user.Id != null) {
                    throw new ArgumentException("tried to add user " + user.Username + ", but they already have an id");
                }
                var conn = ConnectionWrapper.Instance;
                SqlCommand stmt = new SqlCommand(null, conn) {
                    CommandText = "INSERT INTO users (username, password_hash, email, session_cookie) VALUES (@username, @password_hash, @email, @session_cookie); SELECT CAST(scope_identity() AS int)"
                };
                stmt.Parameters.AddWithValue("@username", user.Username);
                stmt.Parameters.AddWithValue("@password_hash", user.PasswordHash);
                stmt.Parameters.AddWithValue("@email", user.Email);
                stmt.Parameters.AddWithValue("@session_cookie", Convert.FromBase64String(user.SessionCookie));

                user.Id = (int)await stmt.ExecuteScalarAsync();
            }
            public static async Task Update(RUser user) {
                if (user.Id == null) {
                    throw new ArgumentException("tried to update user " + user.Username + ", but they dont have an id");
                }
                var conn = ConnectionWrapper.Instance;
                SqlCommand stmt = new SqlCommand(null, conn) {
                    CommandText = "UPDATE users SET username = @username, password_hash = @password_hash, email = @email WHERE id = @id"
                };
                stmt.Parameters.AddWithValue("@username", user.Username);
                stmt.Parameters.AddWithValue("@password_hash", user.PasswordHash);
                stmt.Parameters.AddWithValue("@id", user.Id);
                stmt.Parameters.AddWithValue("@email", user.Email);
                await stmt.ExecuteNonQueryAsync();
            }
            public static async Task<IEnumerable<RUser>> GetByUsername(string username) {
                var conn = ConnectionWrapper.Instance;
                SqlCommand stmt = new SqlCommand(null, conn) {
                    CommandText = "SELECT id, password_hash, email, session_cookie FROM users WHERE username = @username",
                };
                stmt.Parameters.AddWithValue("@username", username);
                List<RUser> users = new List<RUser>();
                using (SqlDataReader reader = await stmt.ExecuteReaderAsync()) {
                    while(reader.Read()) {
                        users.Add(new RUser(username, reader.GetSqlBinary(1).Value, reader.GetSqlString(2).Value, reader.GetSqlBinary(3).NullableValue(), reader.GetSqlInt32(0).Value));
                    }
                }
                return users;
            }
            public static async Task<RUser> Get(int id) {
                var conn = ConnectionWrapper.Instance;
                SqlCommand stmt = new SqlCommand(null, conn) {
                    CommandText = "SELECT username, password_hash, email, session_cookie FROM users WHERE id = @id",
                };
                stmt.Parameters.AddWithValue("@id", id);
                RUser user = null;
                using (SqlDataReader reader = await stmt.ExecuteReaderAsync()) {
                    if(reader.Read()) {
                        user = new RUser(reader.GetSqlString(0).Value, reader.GetSqlBinary(1).Value, reader.GetSqlString(2).Value, reader.GetSqlBinary(3).NullableValue(), id);
                    }
                }
                return user;
            }

            public static async Task<RUser> GetBySessionCookie(byte[] sessionCookie) {
                var conn = ConnectionWrapper.Instance;
                SqlCommand stmt = new SqlCommand(null, conn) {
                    CommandText = "SELECT username, password_hash, email, id FROM users WHERE session_cookie = @session_cookie",
                };
                stmt.Parameters.AddWithValue("@session_cookie", sessionCookie);
                RUser user = null;
                using (SqlDataReader reader = await stmt.ExecuteReaderAsync()) {
                    if (reader.Read()) {
                        user = new RUser(reader.GetSqlString(0).Value, reader.GetSqlBinary(1).Value, reader.GetSqlString(2).Value, sessionCookie, reader.GetSqlInt32(3).Value);
                    }
                }
                return user;
            }

            public static async Task<RUser> GetBySessionCookie(string sessionCookie) {
                return await GetBySessionCookie(Convert.FromBase64String(sessionCookie));
            }

            public static async Task<RUser> GetByEmail(string email) {
                var conn = ConnectionWrapper.Instance;
                SqlCommand stmt = new SqlCommand(null, conn) {
                    CommandText = "SELECT username, password_hash, id, session_cookie FROM users WHERE email = @email",
                };
                stmt.Parameters.AddWithValue("@email", email);
                RUser user = null;
                using (SqlDataReader reader = await stmt.ExecuteReaderAsync()) {
                    if (reader.Read()) {
                        user = new RUser(reader.GetSqlString(0).Value, reader.GetSqlBinary(1).Value, email, reader.GetSqlBinary(3).NullableValue(), reader.GetSqlInt32(2).Value);
                    }
                }
                return user;
            }
        }
    }
}
