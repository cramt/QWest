using Model.Geographic;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace QWest.DataAcess {
    public static partial class DAO {
        public static class Geography {
            public static Task InsertBackup(IEnumerable<Country> countries) {
                return InsertBackup(countries, ConnectionWrapper.Instance);
            }
            public static async Task InsertBackup(IEnumerable<Country> countries, SqlConnection conn) {
                int i = 0;
                Func<string, Subdivision, List<(string name, object parameter)>, StringBuilder, string> recSubdivisionInsert = null;
                recSubdivisionInsert = (id, subdivision, preparedQueryParams, declarations) => {
                    i++;
                    preparedQueryParams.Add(("@alpha_2" + i, subdivision.Alpha2));
                    preparedQueryParams.Add(("@name" + i, subdivision.Name));
                    preparedQueryParams.Add(("@type" + i, subdivision.Type));
                    string q = "" +
                    "INSERT INTO geopolitical_location " +
                    "(alpha_2, name, type, super_id) " +
                    "VALUES " +
                    $"(CAST(@alpha_2{i} as CHAR(2)), @name{i}, @type{i}, {id}); ";
                    if (subdivision.Subdivisions.Count() != 0) {
                        declarations.Append($"@last_id{i} INT, ");
                        string thisid = "@last_id" + i;
                        q += "" +
                        $"SET @last_id{i} = CAST(scope_identity() as int); " +
                        string.Join("", subdivision.Subdivisions.Select(x => recSubdivisionInsert(thisid, x, preparedQueryParams, declarations)).ToArray());
                    }
                    return q;
                };
                IEnumerable<(List<(string name, object parameter)> preparedQueryParams, string declarations, string query)> rawQueries = countries.Select((country) => {
                    i++;
                    StringBuilder declarations = new StringBuilder();
                    declarations.Append($"@last_id{i} INT, ");
                    List<(string name, object parameter)> preparedQueryParams = new List<(string name, object parameter)>();
                    preparedQueryParams.Add(("@alpha_2" + i, country.Alpha2.ToCharArray()));
                    preparedQueryParams.Add(("@alpha_3" + i, country.Alpha3.ToCharArray()));
                    preparedQueryParams.Add(("@name" + i, country.Name));
                    preparedQueryParams.Add(("@official_name" + i, country.OfficialName ?? SqlString.Null));
                    preparedQueryParams.Add(("@common_name" + i, country.CommonName ?? SqlString.Null));
                    preparedQueryParams.Add(("@type" + i, country.Type));
                    preparedQueryParams.Add(("@numeric" + i, country.Numeric));
                    string thisid = "@last_id" + i;
                    string query = "" +
                    "INSERT INTO geopolitical_location " +
                    "(alpha_2, alpha_3, name, official_name, common_name, type, numeric) " +
                    "VALUES " +
                    $"(CAST(@alpha_2{i} as CHAR(2)), @alpha_3{i}, @name{i}, @official_name{i}, @common_name{i}, @type{i}, @numeric{i}); " +
                    $"SET @last_id{i} = CAST(scope_identity() as int); " +
                    string.Join("", country.Subdivisions.Select(x => recSubdivisionInsert(thisid, x, preparedQueryParams, declarations)).ToArray());
                    return (preparedQueryParams, declarations.ToString(), query);
                });
                IEnumerable<(List<(string name, object parameter)> preparedQueryParams, string declarations, string query)> queries = rawQueries.Aggregate(new List<(List<(string name, object parameter)> preparedQueryParams, string declarations, string query)> { (new List<(string name, object parameter)>(), "", "") }, (acc, x) => {
                    int totalAmount = acc[acc.Count - 1].preparedQueryParams.Count + x.preparedQueryParams.Count;
                    if (totalAmount > 2000) {
                        acc.Append(x);
                    }
                    else {
                        acc[acc.Count - 1] = (acc[acc.Count - 1].preparedQueryParams.Concat(x.preparedQueryParams).ToList(), acc[acc.Count - 1].declarations + x.declarations, acc[acc.Count - 1].query + x.query);
                    }
                    return acc;
                });
                await Task.WhenAll(queries.Select(x => {
                    SqlCommand stmt = conn.CreateCommand();
                    stmt.CommandText = "DECLARE " + x.declarations.Substring(0, x.declarations.Length - 2) + ";" + x.query;
                    foreach ((string name, object parameter) in x.preparedQueryParams) {
                        stmt.Parameters.AddWithValue(name, parameter);
                    }
                    return stmt.ExecuteNonQueryAsync();
                }).ToArray());
            }
        }
    }
}
