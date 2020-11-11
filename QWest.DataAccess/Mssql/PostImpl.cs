using Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using static QWest.DataAcess.DAO;
using static QWest.DataAcess.Mssql.GeographyImpl;

namespace QWest.DataAcess.Mssql {
    class PostImpl : IPost {
        private ConnectionWrapper _conn;
        public PostImpl(ConnectionWrapper conn) {
            _conn = conn;
        }
        public async Task<List<Post>> Get(User user) {
            if (user.Id == null) {
                throw new ArgumentException("tried to fetch posts of user " + user.Username + " but this user does not have an id");
            }
            return await GetByUserId((int)user.Id);
        }
        public Task<List<Post>> GetByUserId(int userId) {
            string query = @"
SELECT
posts.id, content, users_id, post_time, (SELECT * FROM dbo.FetchGeopoliticalLocation(location) FOR JSON PATH) AS location,
username, password_hash, email, description, session_cookie, 
(SELECT STRING_AGG(images_id, ',') AS images FROM posts_images WHERE posts_images.posts_id = posts.id) AS images
FROM
users INNER JOIN posts ON users.id = posts.users_id
WHERE users.id = @id";
            return _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@id", userId);
                User user = null;
                return (await stmt.ExecuteReaderAsync()).ToIterator(reader => {
                    if (user == null) {
                        user = new User(reader.GetSqlString(5).Value, reader.GetSqlBinary(6).Value, reader.GetSqlString(7).Value, reader.GetSqlString(8).NullableValue(), reader.GetSqlBinary(9).NullableValue(), reader.GetSqlInt32(2).Value);
                    }
                    return new Post(reader.GetSqlString(1).Value, user, reader.GetSqlInt32(3).Value, reader.GetSqlString(10).NullableValue().MapValue(y => y.Split(',').Select(x => int.Parse(x)).ToList()).UnwrapOr(new List<int>()), reader.GetSqlString(4).NullableValue().MapValue(x => GeopoliticalLocationDbRep.ToTreeStructure(GeopoliticalLocationDbRep.FromJson(x)).First()), reader.GetSqlInt32(0).Value);
                }).ToList();
            });
        }
        public Task<Post> Add(string contents, User user, List<byte[]> images, int? locationId) {
            string query = $@"
DECLARE @post_id INT;
INSERT INTO posts
(content, users_id, post_time, location)
VALUES
(@content, @user_id, @post_time, @location);
SET @post_id = CAST(scope_identity() AS INT);
" +
            string.Join("", images.Select((_, i) => $@"
INSERT INTO images
(image_blob)
VALUES
(@image_blob{i});

INSERT INTO posts_images
(posts_id, images_id)
VALUES
(@post_id, (SELECT CAST(scope_identity() as int)));
")) +
            $@"
SELECT
(SELECT STRING_AGG(images_id, ',') AS images FROM posts_images WHERE posts_images.posts_id = @post_id) AS IMAGES, 
(select * from dbo.FetchGeopoliticalLocation(location) for json path) as location, 
id
FROM posts
WHERE posts.id = @post_id
"; ;
            return _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@content", contents);
                stmt.Parameters.AddWithValue("@user_id", user.Id);
                uint upostTime = DateTime.Now.ToUint();
                int postTime = upostTime.ToSigned();
                stmt.Parameters.AddWithValue("@post_time", postTime);
                stmt.Parameters.AddWithValue("@location", locationId ?? SqlInt32.Null);
                for (int i = 0; i < images.Count; i++) {
                    stmt.Parameters.AddWithValue("@image_blob" + i, images[i]);
                }

                return (await stmt.ExecuteReaderAsync())
                    .ToIterator(reader => new Post(contents, user, upostTime, reader.GetSqlString(0).NullableValue().MapValue(y => y.Split(',').Select(x => int.Parse(x)).ToList()).UnwrapOr(new List<int>()), reader.GetSqlString(1).NullableValue().MapValue(x => GeopoliticalLocationDbRep.ToTreeStructure(GeopoliticalLocationDbRep.FromJson(x)).First()), reader.GetSqlInt32(2).Value)).FirstOrDefault();

            });
        }
    }
}
