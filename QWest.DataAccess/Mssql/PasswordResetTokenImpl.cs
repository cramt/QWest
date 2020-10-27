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
    class PasswordResetTokenImpl : IPasswordResetToken {
        private SqlConnection _conn;
        public PasswordResetTokenImpl(SqlConnection conn) {
            _conn = conn;
        }
        public async Task<string> NewToken(RUser user) {
            if (user.Id == null) {
                throw new ArgumentException("tried to create a password reset token for user " + user.Username + ", but they dont have an id");
            }
            SqlCommand stmt = _conn.CreateCommand("" +
                "DECLARE @value BINARY(50); " +
                "DECLARE @temp_val BINARY(50); " +
                "" +
                "SET @value = NULL; " +
                "SET @temp_val = NULL; " +
                "" +
                "WHILE @value IS NULL " +
                "BEGIN " +
                "SET @temp_val = convert(binary(8), RAND()) + convert(binary(8), RAND()) + convert(binary(8), RAND()) + convert(binary(8), RAND()) + convert(binary(8), RAND()) +convert(binary(8), RAND()) + convert(binary(2), RAND()); " +
                "IF (SELECT COUNT(*) FROM password_reset_tokens where token = @temp_val) < 1" +
                "BEGIN " +
                "SET @value = @temp_val; " +
                "END " +
                "END " +
                "INSERT INTO password_reset_tokens (users_id, token) VALUES (@user_id, @value); " +
                "SELECT @value");
            stmt.Parameters.AddWithValue("@user_id", user.Id);
            byte[] token = (await stmt.ExecuteReaderAsync())
                .ToIterator(x => x.GetSqlBinary(0).Value)
                .FirstOrDefault();
            return Convert.ToBase64String(token);
        }

        public  async Task<RUser> GetUser(string stringToken) {
            byte[] token = Convert.FromBase64String(stringToken);
            SqlCommand stmt = _conn.CreateCommand("SELECT id, username, password_hash, email, session_cookie, description FROM users INNER JOIN password_reset_tokens ON users_id = id WHERE token = @token");
            stmt.Parameters.AddWithValue("@token", token);
            return (await stmt.ExecuteReaderAsync())
                .ToIterator(reader => new RUser(reader.GetSqlString(1).Value, reader.GetSqlBinary(2).Value, reader.GetSqlString(3).Value, reader.GetSqlString(5).Value, reader.GetSqlBinary(4).NullableValue(), reader.GetSqlInt32(0).Value))
                .FirstOrDefault();
        }

        public  async Task DeleteToken(string stringToken) {
            byte[] token = Convert.FromBase64String(stringToken);
            SqlCommand stmt = _conn.CreateCommand("DELETE FROM password_reset_tokens WHERE token = @token");
            stmt.Parameters.AddWithValue("@token", token);
            await stmt.ExecuteNonQueryAsync();
        }
    }
}
