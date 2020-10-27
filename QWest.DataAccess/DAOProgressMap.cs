using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using static Utilities.Utilities;
using RProgressMap = Model.ProgressMap;
using RUser = Model.User;

namespace QWest.DataAcess {
    public static partial class DAO {
        public interface IProgressMap {
            Task<RProgressMap> Get(RUser user);
            Task<RProgressMap> Get(RProgressMap map);
            Task<RProgressMap> Get(int id);
            Task<RProgressMap> GetByUserId(int userId);
            Task Update(int id, List<int> additions, List<int> subtractions);
            Task Update(RProgressMap map);
        }
        public static IProgressMap ProgressMap { get; set; } = new Mssql.ProgressMapImpl(ConnectionWrapper.Instance);
    }
}
