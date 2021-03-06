﻿using Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using static QWest.DataAccess.DAO;
using static QWest.DataAccess.Mssql.GeographyImpl;
using static QWest.DataAccess.Mssql.UserImpl;

namespace QWest.DataAccess.Mssql {
    internal class PostImpl : IPost {
        [Serializable]
        internal class PostDbRep : IDbRep<Post> {
            public const string SELECT_ORDER = @"
posts.id, content, users_id, groups_id, post_time, (SELECT * FROM dbo.FetchGeopoliticalLocation(location) FOR JSON PATH) AS location,
username, password_hash, email, users.description, session_cookie, profile_picture,
name, creation_time, groups.description,
(SELECT STRING_AGG(images_id, ',') AS images FROM posts_images WHERE posts_images.posts_id = posts.id) AS images,
(SELECT id, username, password_hash, email, session_cookie, description, profile_picture 
FROM users INNER JOIN users_groups ON users.id = users_id WHERE groups_id = groups.id FOR JSON PATH) AS member
";
            public int Id { get; }
            public string Content { get; }
            public int? UserId { get; }
            public int? GroupId { get; }
            public int PostTime { get; }
            public string Location { get; }
            public string Username { get; }
            public byte[] PasswordHash { get; }
            public string Email { get; }
            public string UserDescription { get; }
            public byte[] SessionCookie { get; }
            public int? ProfilePicture { get; }
            public string Name { get; }
            public int? CreationTime { get; }
            public string GroupDescription { get; }
            public string Images { get; }
            public string Members { get; }
            public PostDbRep(SqlDataReader reader) {
                int i = 0;
                Id = reader.GetSqlInt32(i++).Value;
                Content = reader.GetSqlString(i++).Value;
                UserId = reader.GetSqlInt32(i++).NullableValue();
                GroupId = reader.GetSqlInt32(i++).NullableValue();
                PostTime = reader.GetSqlInt32(i++).Value;
                Location = reader.GetSqlString(i++).NullableValue();
                Username = reader.GetSqlString(i++).NullableValue();
                PasswordHash = reader.GetSqlBinary(i++).NullableValue();
                Email = reader.GetSqlString(i++).NullableValue();
                UserDescription = reader.GetSqlString(i++).NullableValue();
                SessionCookie = reader.GetSqlBinary(i++).NullableValue();
                ProfilePicture = reader.GetSqlInt32(i++).NullableValue();
                Name = reader.GetSqlString(i++).NullableValue();
                CreationTime = reader.GetSqlInt32(i++).NullableValue();
                GroupDescription = reader.GetSqlString(i++).NullableValue();
                Images = reader.GetSqlString(i++).NullableValue();
                Members = reader.GetSqlString(i++).NullableValue();
            }

            public Post ToModel() {
                User userAuthor = null;
                Group groupAuthor = null;
                if (UserId != null) {
                    userAuthor = new User(Username, PasswordHash, Email, UserDescription, SessionCookie, UserId) {
                        ProfilePicture = ProfilePicture
                    };
                }
                else if (GroupId != null) {
                    groupAuthor = new Group(Name, (int)CreationTime, GroupDescription, null, Members.MapValue(UserDbRep.FromJson).Select(x => x.ToModel()), GroupId);
                }
                else {
                    throw new ArgumentException("in this post the author is neither a user or group");
                }
                return new Post(Content, userAuthor, groupAuthor, PostTime, Images.MapValue(x => x.Split(',').Select(y => int.Parse(y)).ToList()), Location.MapValue(x => GeopoliticalLocationDbRep.ToTreeStructureFirst(GeopoliticalLocationDbRep.FromJson(x))), Id);
            }
        }
        private ConnectionWrapper _conn;
        public PostImpl(ConnectionWrapper conn) {
            _conn = conn;
        }
        public async Task<List<Post>> GetByUser(User user) {
            if (user.Id == null) {
                throw new ArgumentException("tried to fetch posts of user " + user.Username + " but this user does not have an id");
            }
            return await GetByUserId((int)user.Id);
        }
        public async Task<List<Post>> GetByUserId(int userId) {
            string query = $@"
SELECT
{PostDbRep.SELECT_ORDER}
FROM
posts 
LEFT JOIN users 
ON 
users.id = posts.users_id
LEFT JOIN groups
ON
groups.id = posts.groups_id
WHERE users.id = @id";
            return (await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@id", userId);
                return (await stmt.ExecuteReaderAsync()).ToIterator(reader => new PostDbRep(reader));
            })).SelectPar(x => x.ToModel()).ToList();
        }

        public Task<Post> Add(string contents, Group group, List<byte[]> images, int? locationId) {
            return AddGroupAuthor(contents, (int)group.Id, images, locationId);
        }

        public async Task<Post> AddGroupAuthor(string contents, int groupId, List<byte[]> images, int? locationId) {
            string query = $@"
DECLARE @post_id INT;
INSERT INTO posts
(content, groups_id, post_time, location)
VALUES
(@content, @group_id, @post_time, @location);
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
{PostDbRep.SELECT_ORDER}
FROM posts
LEFT JOIN users
ON
users.id = posts.users_id
LEFT JOIN groups
ON
groups.id = posts.groups_id
WHERE posts.id = @post_id
";
            return (await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@content", contents);
                stmt.Parameters.AddWithValue("@group_id", groupId);
                uint upostTime = DateTime.Now.ToUint();
                int postTime = upostTime.ToSigned();
                stmt.Parameters.AddWithValue("@post_time", postTime);
                stmt.Parameters.AddWithValue("@location", locationId ?? SqlInt32.Null);
                for (int i = 0; i < images.Count; i++) {
                    stmt.Parameters.AddWithValue("@image_blob" + i, images[i]);
                }

                return (await stmt.ExecuteReaderAsync())
                    .ToIterator(reader => new PostDbRep(reader));
            })).First().ToModel();
        }

        public Task<Post> Add(string contents, User user, List<byte[]> images, int? locationId) {
            return AddUserAuthor(contents, (int)user.Id, images, locationId);
        }

        public async Task<Post> AddUserAuthor(string contents, int userId, List<byte[]> images, int? locationId) {
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
{PostDbRep.SELECT_ORDER}
FROM posts
LEFT JOIN users
ON
users.id = posts.users_id
LEFT JOIN groups
ON
groups.id = posts.groups_id
WHERE posts.id = @post_id
";
            return (await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@content", contents);
                stmt.Parameters.AddWithValue("@user_id", userId);
                uint upostTime = DateTime.Now.ToUint();
                int postTime = upostTime.ToSigned();
                stmt.Parameters.AddWithValue("@post_time", postTime);
                stmt.Parameters.AddWithValue("@location", locationId ?? SqlInt32.Null);
                for (int i = 0; i < images.Count; i++) {
                    stmt.Parameters.AddWithValue("@image_blob" + i, images[i]);
                }

                return (await stmt.ExecuteReaderAsync())
                    .ToIterator(reader => new PostDbRep(reader));

            })).First().ToModel();
        }

        public async Task<IEnumerable<Post>> GetFeed(User user, int amount = 20, int offset = 0) {
            return await GetFeedByUserId((int)user.Id, amount, offset);
        }
        public async Task<IEnumerable<Post>> GetFeedByUserId(int id, int amount = 20, int offset = 0) {
            if (amount < 1) {
                throw new ArgumentException("cant fetch amount of posts less than 1");
            }
            string query = $@"
SELECT 
{PostDbRep.SELECT_ORDER}
FROM posts 
LEFT JOIN users
ON 
users.id = posts.users_id
LEFT JOIN groups
ON
groups.id = posts.groups_id
WHERE
users.id = @user_id
OR 
users.id IN (
    SELECT 
    left_user_id
    FROM
    users_friendships
    WHERE
    right_user_id = @user_id
)
OR
groups.id IN (
    SELECT
    id
    FROM
    groups
    INNER JOIN
    users_groups
    ON
    groups.id = users_groups.groups_id
    WHERE
    users_groups.users_id = @user_id
)
OR
groups.id IN (
    SELECT
    id
    FROM
    groups
    INNER JOIN
    users_groups
    ON
    groups.id = users_groups.groups_id
    INNER JOIN
    users_friendships
    ON
    left_user_id = users_groups.users_id
    WHERE
    right_user_id = @user_id
)

ORDER BY id DESC
OFFSET {offset} ROWS 
FETCH NEXT {amount} ROWS ONLY;
";
            return (await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@user_id", id);
                Dictionary<int, User> userMap = new Dictionary<int, User>();
                return (await stmt.ExecuteReaderAsync()).ToIterator(reader => new PostDbRep(reader));
            })).SelectPar(x => x.ToModel());
        }

        public async Task Update(Post post) {
            if (post.Id == null) {
                throw new ArgumentException("Editing a post needs a post ID");
            }
            string query = @"
UPDATE posts
SET content = @content
WHERE id = @post_id";
            await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@content", post.Contents);
                stmt.Parameters.AddWithValue("@post_id", post.Id);
                await stmt.ExecuteNonQueryAsync();
                return true;
            });
        }

        public Task<bool> IsAuthor(User user, Post post) {
            if (user.Id == null) {
                throw new ArgumentException("User must have ID");
            }
            return IsAuthor((int)user.Id, post);
        }

        public Task<bool> IsAuthor(int userId, Post post) {
            if (post.Id == null) {
                throw new ArgumentException("Post must have ID");
            }

            return IsAuthor(userId, (int)post.Id);
        }

        public Task<bool> IsAuthor(User user, int postId) {
            if (user.Id == null) {
                throw new ArgumentException("User must have ID");
            }

            return IsAuthor((int)user.Id, postId);
        }

        public async Task<bool> IsAuthor(int userId, int postId) {
            string query = @"
SELECT
COUNT(*)
FROM posts
LEFT JOIN users
ON
users.id = posts.users_id
LEFT JOIN groups
ON
groups.id = posts.groups_id
WHERE
(users.id = @user_id
OR
groups.id = (
    SELECT groups_id
    FROM users_groups
    WHERE users_id = @user_id)
)
AND
posts.id = @post_id";
            return (await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@user_id", userId);
                stmt.Parameters.AddWithValue("@post_id", postId);
                return (await stmt.ExecuteReaderAsync()).ToIterator(reader => reader.GetSqlInt32(0).Value);
            })).First() == 1;
        }

        public async Task<IEnumerable<Post>> GetGroupFeed(Group group, int amount = 20, int offset = 0) {
            return await GetGroupFeedById((int)group.Id, amount, offset);
        }

        public async Task<IEnumerable<Post>> GetGroupFeedById(int id, int amount = 20, int offset = 0) {
            string query = $@"
SELECT
{PostDbRep.SELECT_ORDER}
FROM
posts 
LEFT JOIN groups
ON
groups.id = groups_id
LEFT JOIN users
ON
users.id = users_id
WHERE
groups_id = @group_id

ORDER BY id DESC
OFFSET {offset} ROWS 
FETCH NEXT {amount} ROWS ONLY;
";
            return (await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@group_id", id);
                return (await stmt.ExecuteReaderAsync()).ToIterator(reader => new PostDbRep(reader));
            })).Select(x => x.ToModel()).ToList();
        }

        public async Task<Post> Get(int id) {
            string query = $@"
SELECT
{PostDbRep.SELECT_ORDER}
FROM
posts 
LEFT JOIN groups
ON
groups.id = groups_id
LEFT JOIN users
ON
users.id = users_id
WHERE
posts.id = @id
";
            return (await _conn.Use(query, async stmt => {
                stmt.Parameters.AddWithValue("@id", id);
                return (await stmt.ExecuteReaderAsync()).ToIterator(x => new PostDbRep(x));
            })).FirstOrDefault().MapValue(x => x.ToModel());
        }
    }
}
