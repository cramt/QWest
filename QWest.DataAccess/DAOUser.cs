using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using RUser = Model.User;

namespace QWest.DataAcess {
    public static partial class DAO {
        public interface IUser {
            Task Add(RUser user);
            Task Update(RUser user);
            Task<IEnumerable<RUser>> GetByUsername(string username);
            Task<RUser> Get(int id);
            Task<RUser> GetBySessionCookie(byte[] sessionCookie);
            Task<RUser> GetBySessionCookie(string sessionCookie);
            Task<RUser> GetByEmail(string email);
            Task<RUser> SetNewSessionCookie(RUser user);
            Task UpdateProfilePicture(byte[] profilePicture, RUser user);
            Task<int> UpdateProfilePicture(byte[] profilePicture, int userId);
        }
        public static IUser User { get; set; } = new Mssql.UserImpl(ConnectionWrapper.Instance);
    }
}
