using System.Linq;
using System.Threading.Tasks;
using Utilities;
using static QWest.DataAcess.DAO;

namespace QWest.DataAcess.Mssql {
    class ImageImpl : IImage {
        private ConnectionWrapper _conn;
        public ImageImpl(ConnectionWrapper conn) {
            _conn = conn;
        }
        public Task<byte[]> Get(int id) {
            return _conn.Use("SELECT image_blob FROM images WHERE id = @id", async stmt => {
                stmt.Parameters.AddWithValue("@id", id);
                return (await stmt.ExecuteReaderAsync()).ToIterator(reader => reader.GetSqlBinary(0).Value).FirstOrDefault();
            });
        }
    }
}
