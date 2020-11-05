using Model;
using System.Threading.Tasks;

namespace QWest.DataAcess {
    public static partial class DAO {
        public interface IPasswordResetToken {
            Task<string> NewToken(User user);
            Task<User> GetUser(string stringToken);
            Task DeleteToken(string stringToken);
        }
        public static IPasswordResetToken PasswordResetToken { get; set; } = new Mssql.PasswordResetTokenImpl(ConnectionWrapper.Instance);
    }
}
