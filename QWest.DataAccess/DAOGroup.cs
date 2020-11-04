using Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;

namespace QWest.DataAcess {
    public static partial class DAO {
        public interface IGroup {
            Task<Group> Create(Group group);
            Task Update(Group group);
            Task AddMember(Group group, User member);
            Task RemoveMember(Group group, User member);
        }
        public static IGroup Group { get; set; } = new Mssql.GroupImpl(ConnectionWrapper.Instance);
    }
}
