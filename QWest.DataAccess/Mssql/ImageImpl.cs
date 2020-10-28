﻿using Model.Geographic;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using static QWest.DataAcess.DAO;

namespace QWest.DataAcess.Mssql {
    class ImageImpl : IImage {
        private SqlConnection _conn;
        public ImageImpl(SqlConnection conn) {
            _conn = conn;
        }
        public async Task<byte[]> Get(int id) {
            SqlCommand stmt = _conn.CreateCommand("SELECT image_blob FROM images WHERE id = @id");
            stmt.Parameters.AddWithValue("@id", id);
            return (await stmt.ExecuteReaderAsync()).ToIterator(reader => reader.GetSqlBinary(0).Value).FirstOrDefault();
        }
    }
}