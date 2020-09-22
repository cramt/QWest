﻿using Microsoft.Win32;
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
            public static async void Add(RUser user) {
                if (user.Id != null) {
                    throw new ArgumentException("tried to add user " + user.Username + ", but they already have an id");
                }
                var conn = ConnectionWrapper.Instance;
                conn.Open();
                SqlCommand stmt = new SqlCommand(null, conn) {
                    CommandText = "INSERT INTO users (username, password_hash) VALUES (@username, @password_hash)"
                };
                stmt.Parameters.AddWithValue("@username", user.Username);
                stmt.Parameters.AddWithValue("@password_hash", user.PasswordHash);
                stmt.Prepare();
                await stmt.ExecuteNonQueryAsync();
            }
            public static async void Update(RUser user) {
                if (user.Id == null) {
                    throw new ArgumentException("tried to update user " + user.Username + ", but they dont have an id");
                }
                var conn = ConnectionWrapper.Instance;
                conn.Open();
                SqlCommand stmt = new SqlCommand(null, conn) {
                    CommandText = "UPDATE users SET username = @username, password_hash = @password_hash WHERE id = @id"
                };
                stmt.Parameters.AddWithValue("@username", user.Username);
                stmt.Parameters.AddWithValue("@password_hash", user.PasswordHash);
                stmt.Parameters.AddWithValue("@id", user.Id);
                stmt.Prepare();
                await stmt.ExecuteNonQueryAsync();
            }
            public static async Task<IEnumerable<RUser>> GetByUsername(string username) {
                var conn = ConnectionWrapper.Instance;
                conn.Open();
                SqlCommand stmt = new SqlCommand(null, conn) {
                    CommandText = "SELECT id, password_hash FROM users WHERE username = @username",
                };
                stmt.Parameters.AddWithValue("@username", username);
                stmt.Prepare();
                List<RUser> users = new List<RUser>();
                using (SqlDataReader reader = await stmt.ExecuteReaderAsync()) {
                    while(reader.Read()) {
                        users.Add(new RUser(username, reader.GetSqlBinary(1).Value, reader.GetSqlInt32(0).Value));
                    }
                }
                return users;
            }
        }
    }
}