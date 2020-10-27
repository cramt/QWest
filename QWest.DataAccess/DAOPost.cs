using Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;
using RPost = Model.Post;
using RUser = Model.User;

namespace QWest.DataAcess {
    public static partial class DAO {
        public interface IPost {
            Task<List<RPost>> Get(RUser user);
            Task<List<RPost>> GetByUserId(int userId);
            Task<RPost> Add(PostUpload post);
        }
        public static IPost Post { get; set; } = new Mssql.PostImpl(ConnectionWrapper.Instance);
    }
}
