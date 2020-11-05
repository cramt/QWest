using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QWest.DataAcess {
    public static partial class DAO {
        public interface IPost {
            Task<List<Post>> Get(User user);
            Task<List<Post>> GetByUserId(int userId);
            Task<Post> Add(PostUpload post);
        }
        public static IPost Post { get; set; } = new Mssql.PostImpl(ConnectionWrapper.Instance);
    }
}
