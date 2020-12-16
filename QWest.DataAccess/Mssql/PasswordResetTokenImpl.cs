using Model;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using static QWest.DataAccess.DAO;

namespace QWest.DataAccess.Mssql {
    internal class PasswordResetTokenImpl : IPasswordResetToken {
        private ConnectionWrapper _conn;
        public PasswordResetTokenImpl(ConnectionWrapper conn) {
            _conn = conn;
        }
        public Task<string> NewToken(User user) {
            if (user.Id == null) {
                throw new ArgumentException("tried to create a password reset token for user " + user.Username + ", but they dont have an id");
            }
            string query = @"
DECLARE @value BINARY(50);
DECLARE @temp_val BINARY(50);
SET @value = NULL;
SET @temp_val = NULL;
WHILE @value IS NULL
BEGIN
SET @temp_val = convert(binary(8), RAND()) + convert(binary(8), RAND()) + convert(binary(8), RAND()) + convert(binary(8), RAND()) + convert(binary(8), RAND()) +convert(binary(8), RAND()) + convert(binary(2), RAND());
IF (SELECT COUNT(*) FROM password_reset_tokens where token = @temp_val) < 1
BEGIN
SET @value = @temp_val;
END
END
INSERT INTO password_reset_tokens (users_id, token) VALUES (@user_id, @value);
SELECT @value
";
            return _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@user_id", user.Id);
                byte[] token = (await stmt.ExecuteReaderAsync())
                    .ToIterator(x => x.GetSqlBinary(0).Value)
                    .FirstOrDefault();
                return Convert.ToBase64String(token);
            });
        }

        public Task<User> GetUser(string stringToken) {
            byte[] token = Convert.FromBase64String(stringToken);
            string query = "SELECT id, username, password_hash, email, session_cookie, description FROM users INNER JOIN password_reset_tokens ON users_id = id WHERE token = @token";
            return _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@token", token);
                return (await stmt.ExecuteReaderAsync())
                    .ToIterator(reader => new User(reader.GetSqlString(1).Value, reader.GetSqlBinary(2).Value, reader.GetSqlString(3).Value, reader.GetSqlString(5).Value, reader.GetSqlBinary(4).NullableValue(), reader.GetSqlInt32(0).Value))
                    .FirstOrDefault();
            });
        }

        public async Task DeleteToken(string stringToken) {
            byte[] token = Convert.FromBase64String(stringToken);
            string query = "DELETE FROM password_reset_tokens WHERE token = @token";
            await _conn.Use(query, stmt => {
                stmt.Parameters.AddWithValue("@token", token);
                return stmt.ExecuteNonQueryAsync();
            });
        }
    }
}
