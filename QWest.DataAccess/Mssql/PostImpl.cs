using Model;
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
using RPost = Model.Post;
using RUser = Model.User;

namespace QWest.DataAcess.Mssql {
    class PostImpl : IPost {
        private SqlConnection _conn;
        public PostImpl(SqlConnection conn) {
            _conn = conn;
        }
        public async Task<List<RPost>> Get(RUser user) {
            if (user.Id == null) {
                throw new ArgumentException("tried to fetch posts of user " + user.Username + " but this user does not have an id");
            }
            return await GetByUserId(user.Id ?? 0);
        }
        public async Task<List<RPost>> GetByUserId(int userId) {
            SqlCommand stmt = _conn.CreateCommand(@"
select
posts.id, content, users_id, post_time, (select * from dbo.FetchGeopoliticalLocation(location) for json path) as location,
username, password_hash, email, description, session_cookie, 
(select STRING_AGG(images_id, ',') as images from posts_images where posts_images.posts_id = posts.id) as images
from
users inner join posts on users.id = posts.users_id
where users.id = @id");
            stmt.Parameters.AddWithValue("@id", userId);
            RUser user = null;
            return (await stmt.ExecuteReaderAsync()).ToIterator(reader => {
                if (user == null) {
                    user = new RUser(reader.GetSqlString(5).Value, reader.GetSqlBinary(6).Value, reader.GetSqlString(7).Value, reader.GetSqlString(8).NullableValue(), reader.GetSqlBinary(9).NullableValue(), reader.GetSqlInt32(2).Value);
                }
                return new RPost(reader.GetSqlString(1).Value, user, reader.GetSqlInt32(3).Value, reader.GetSqlString(10).NullableValue().MapValue(y => y.Split(',').Select(x => int.Parse(x)).ToList()).UnwrapOr(new List<int>()), reader.GetSqlString(4).NullableValue().MapValue(x => GeopoliticalLocationDbRep.ToTreeStructure(GeopoliticalLocationDbRep.FromJson(x)).First()), reader.GetSqlInt32(0).Value);
            }).ToList();
        }
        public async Task<RPost> Add(PostUpload post) {
            if (post.User.Id == null) {
                throw new ArgumentException("tried to make a post on user: " + post.User.Username + ", but that user doesnt have an id");
            }
            string statement = "" +
                "declare @post_id int;" +
                "insert into posts " +
                "(content, users_id, post_time, location)" +
                "values" +
                "(@content, @user_id, @post_time, @location);" +
                "" +
                "set @post_id = CAST(scope_identity() as int);" +
            string.Join("", post.Images.Select((_, i) => "" +
            "insert into images " +
            "(image_blob) " +
            "values " +
            "(@image_blob" + i + ");" +
            "" +
            "insert into posts_images" +
            "(posts_id, images_id)" +
            "values" +
            "(@post_id, (SELECT CAST(scope_identity() as int)));")) +


            "select " +
            "(select STRING_AGG(images_id, ',') as images from posts_images where posts_images.posts_id = @post_id) as images, " +
            "(select * from dbo.FetchGeopoliticalLocation(location) for json path) as location, " +
            "id " +
            "from posts " +
            "where posts.id = @post_id";
            SqlCommand stmt = _conn.CreateCommand(statement);
            stmt.Parameters.AddWithValue("@content", post.Contents);
            stmt.Parameters.AddWithValue("@user_id", post.User.Id);
            uint upostTime = (uint)post.PostTime.Subtract(Config.Config.Instance.StartDate).TotalSeconds;
            int postTime = unchecked((int)upostTime);
            stmt.Parameters.AddWithValue("@post_time", postTime);
            stmt.Parameters.AddWithValue("@location", post.Location ?? SqlInt32.Null);
            for (int i = 0; i < post.Images.Count; i++) {
                stmt.Parameters.AddWithValue("@image_blob" + i, post.Images[i]);
            }

            return (await stmt.ExecuteReaderAsync())
                .ToIterator(reader => new RPost(post.Contents, post.User, upostTime, reader.GetSqlString(0).NullableValue().MapValue(y => y.Split(',').Select(x => int.Parse(x)).ToList()).UnwrapOr(new List<int>()), reader.GetSqlString(1).NullableValue().MapValue(x => GeopoliticalLocationDbRep.ToTreeStructure(GeopoliticalLocationDbRep.FromJson(x)).First()), reader.GetSqlInt32(2).Value)).FirstOrDefault();
        }
    }
}
