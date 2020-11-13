using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QWest.DataAcess {
    public static partial class DAO {
        public interface IPost {
            Task<List<Post>> Get(User user);
            Task<List<Post>> GetByUserId(int userId);
            Task<Post> Add(string contents, User user, List<byte[]> images, int? locationId);
            Task<IEnumerable<Post>> GetFeed(User user, int amount = 20, int offset = 0);
        }
        public static IPost Post { get; set; } = new Mssql.PostImpl(ConnectionWrapper.Instance);
    }
}
