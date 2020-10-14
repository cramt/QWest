using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using static Utilities.Utilities;
using RUser = Model.User;

namespace QWest.DataAcess {
    public static partial class DAO {
        public static class PasswordResetToken {
            public static async Task<string> NewToken(RUser user) {
                if (user.Id == null) {
                    throw new ArgumentException("tried to create a password reset token for user " + user.Username + ", but they dont have an id");
                }
                Random rng = new Random();
                byte[] token = new byte[50];
                rng.NextBytes(token);
                SqlCommand stmt = ConnectionWrapper.CreateCommand("INSERT INTO password_reset_tokens (users_id, token) VALUES (@user_id, @token)");
                stmt.Parameters.AddWithValue("@user_id", user.Id);
                stmt.Parameters.AddWithValue("@token", token);
                await stmt.ExecuteNonQueryAsync();
                return Convert.ToBase64String(token);
            }

            public static async Task<RUser> GetUser(string stringToken) {
                byte[] token = Convert.FromBase64String(stringToken);
                SqlCommand stmt = ConnectionWrapper.CreateCommand("SELECT id, username, password_hash, email, session_cookie FROM users INNER JOIN password_reset_tokens ON users_id = id WHERE token = @token");
                stmt.Parameters.AddWithValue("@token", token);
                return (await stmt.ExecuteReaderAsync())
                    .ToIterator(reader => new RUser(reader.GetSqlString(0).Value, reader.GetSqlBinary(1).Value, reader.GetSqlString(2).Value, reader.GetSqlBinary(3).NullableValue(), reader.GetSqlInt32(4).Value))
                    .FirstOrDefault();
            }

            public static async Task DeleteToken(string stringToken) {
                byte[] token = Convert.FromBase64String(stringToken);
                SqlCommand stmt = ConnectionWrapper.CreateCommand("DELETE FROM password_reset_tokens WHERE token = @token");
                stmt.Parameters.AddWithValue("@token", token);
                await stmt.ExecuteNonQueryAsync();
            }
        }
    }
}
