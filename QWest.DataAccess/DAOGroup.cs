using Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;

namespace QWest.DataAccess {
    public static partial class DAO {
        public interface IGroup {
            Task<int> Create(string name, string description, List<int> members, DateTime? creation = null);
            Task<Group> Create(Group group);
            Task Update(Group group);
            Task Update(int id, string name, string description);
            Task UpdateMembers(int groupId, List<int> additions, List<int> subtractions);
            Task<IEnumerable<Group>> FetchUsersGroups(User user);
            Task<IEnumerable<Group>> FetchUsersGroups(int userId);
            Task<bool> IsMember(int groupId, int userId);
            Task<bool> IsMember(int groupId, User user);
            Task<bool> IsMember(Group group, int userId);
            Task<bool> IsMember(Group group, User user);
            Task<Group> Get(int id);
        }
        public static IGroup Group { get; set; } = new Mssql.GroupImpl(ConnectionWrapper.Instance);
    }
}
