using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QWest.DataAccess {
    public static partial class DAO {
        // friendship is magic
        public interface IFriendship {
            Task<List<User>> GetUsersFriends(User user);
            Task<List<User>> GetUsersFriends(int userId);
            Task<bool> AddFriendRequest(User from, User to);
            Task<bool> AddFriendRequest(User from, int to);
            Task<bool> AddFriendRequest(int from, User to);
            Task<bool> AddFriendRequest(int from, int to);
            Task<List<User>> GetFriendRequests(User user);
            Task<List<User>> GetFriendRequests(int userId);
            Task<bool> AcceptFriendRequest(User from, User to);
            Task<bool> AcceptFriendRequest(User from, int to);
            Task<bool> AcceptFriendRequest(int from, User to);
            Task<bool> AcceptFriendRequest(int from, int to);
        }
        public static IFriendship Friendship { get; set; } = new Mssql.FriendshipImpl(ConnectionWrapper.Instance);
    }
}
