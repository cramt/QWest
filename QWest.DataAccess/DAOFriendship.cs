using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QWest.DataAcess {
    public static partial class DAO {
        // friendship is magic
        public interface IFriendship {
            Task<List<User>> GetUsersFriends(User user);
            Task<List<User>> GetUsersFriends(int userId);
            Task AddFriendRequest(User from, User to);
            Task AddFriendRequest(User from, int to);
            Task AddFriendRequest(int from, User to);
            Task AddFriendRequest(int from, int to);
            Task<List<User>> GetFriendRequests(User user);
            Task<List<User>> GetFriendRequests(int userId);
            Task AcceptFriendRequest(User from, User to);
            Task AcceptFriendRequest(User from, int to);
            Task AcceptFriendRequest(int from, User to);
            Task AcceptFriendRequest(int from, int to);
        }
        public static IFriendship Friendship { get; set; } = new Mssql.FriendshipImpl(ConnectionWrapper.Instance);
    }
}
