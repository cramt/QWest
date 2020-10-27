using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using static Utilities.Utilities;
using RUser = Model.User;

namespace QWest.DataAcess {
    public static partial class DAO {
        public interface IPasswordResetToken {
            Task<string> NewToken(RUser user);
            Task<RUser> GetUser(string stringToken);
            Task DeleteToken(string stringToken);
        }
        public static IPasswordResetToken PasswordResetToken { get; set; } = new Mssql.PasswordResetTokenImpl(ConnectionWrapper.Instance);
    }
}
