using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QWest.DataAccess {
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
