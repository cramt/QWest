using System.Threading.Tasks;

namespace QWest.DataAcess {
    public static partial class DAO {
        public interface IImage {
            Task<byte[]> Get(int id);
        }
        public static IImage Image { get; set; } = new Mssql.ImageImpl(ConnectionWrapper.Instance);
    }
}
