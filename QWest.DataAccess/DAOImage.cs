using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;

namespace QWest.DataAcess {
    public static partial class DAO {
        public static class Image {
            public static async Task<byte[]> Get(int id) {
                SqlCommand stmt = ConnectionWrapper.CreateCommand("SELECT image_blob FROM images WHERE id = @id");
                stmt.Parameters.AddWithValue("@id", id);
                return (await stmt.ExecuteReaderAsync()).ToIterator(reader => reader.GetSqlBinary(0).Value).FirstOrDefault();
            }
        }
    }
}
