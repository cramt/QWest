using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;
using RPost = Model.Post;
using RUser = Model.User;

namespace QWest.DataAcess {
    public static partial class DAO {
        public static class Post {
            public static async Task<List<RPost>> Get(RUser user) {
                if (user.Id == null) {
                    throw new ArgumentException("tried to fetch posts of user " + user.Username + " but this user does not have an id");
                }
                return await GetByUserId(user.Id ?? 0);
            }
            public static async Task<List<RPost>> GetByUserId(int userId) {
                SqlCommand stmt = ConnectionWrapper.CreateCommand("select" +
"posts.id, content, users_id, post_time, location_id," +
"username, password_hash, email, description, session_cookie," +
"(select STRING_AGG(images_id, ',') as images from posts_images where posts_images.posts_id = posts.id) as images" +
"from" +
"users inner join posts on users.id = posts.users_id" +
"where users.id = @id");
                stmt.Parameters.AddWithValue("@id", userId);
                RUser user = null;
                return (await stmt.ExecuteReaderAsync()).ToIterator(reader => {
                    if (user == null) {
                        user = new RUser(reader.GetSqlString(5).Value, reader.GetSqlBinary(6).Value, reader.GetSqlString(7).Value, reader.GetSqlString(8).Value, reader.GetSqlBinary(9).Value, reader.GetSqlInt32(2).Value);
                    }
                    return new RPost(reader.GetSqlString(1).Value, user, reader.GetSqlInt32(3).Value, reader.GetSqlString(10).Value.Split(',').Select(x => int.Parse(x)).ToList(), reader.GetSqlInt32(0).Value);
                }).ToList();
            }
        }
    }
}
