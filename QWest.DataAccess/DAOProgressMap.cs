using Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using static Utilities.Utilities;

namespace QWest.DataAcess {
    public static partial class DAO {
        public interface IProgressMap {
            Task<ProgressMap> Get(User user);
            Task<ProgressMap> Get(ProgressMap map);
            Task<ProgressMap> Get(int id);
            Task<ProgressMap> GetByUserId(int userId);
            Task Update(int id, List<int> additions, List<int> subtractions);
            Task Update(ProgressMap map);
        }
        public static IProgressMap ProgressMap { get; set; } = new Mssql.ProgressMapImpl(ConnectionWrapper.Instance);
    }
}
