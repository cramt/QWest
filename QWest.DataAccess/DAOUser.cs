using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QWest.DataAcess {
    public static partial class DAO {
        public interface IUser {
            Task Add(User user);
            Task Update(User user);
            Task<List<User>> GetByUsername(string username);
            Task<User> Get(int id);
            Task<User> GetBySessionCookie(byte[] sessionCookie);
            Task<User> GetBySessionCookie(string sessionCookie);
            Task<User> GetByEmail(string email);
            Task<User> SetNewSessionCookie(User user);
            Task UpdateProfilePicture(byte[] profilePicture, User user);
            Task<int> UpdateProfilePicture(byte[] profilePicture, int userId);
        }
        public static IUser User { get; set; } = new Mssql.UserImpl(ConnectionWrapper.Instance);
    }
}
