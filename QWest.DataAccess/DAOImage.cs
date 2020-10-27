using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;

namespace QWest.DataAcess {
    public static partial class DAO {
        public interface IImage {
            Task<byte[]> Get(int id);
        }
        public static IImage Image { get; set; } = new Mssql.ImageImpl(ConnectionWrapper.Instance);
    }
}
