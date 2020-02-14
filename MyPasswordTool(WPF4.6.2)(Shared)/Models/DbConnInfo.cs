using Microsoft.Data.Sqlite;
using SilverEx;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace MyPasswordTool.Models
{
    public class DbConnInfo : IDisposable
    {
        SqliteConnection _con;

        CultureInfoComparer comparer { get; } = IoC.Resolve<CultureInfoComparer>();

        public string Pwd { get; set; }

        public string DB { get; set; }

        public async Task<DbConnection> GetConnAsync()
        {
            if (_con == null)
            {
                var sb = new SqliteConnectionStringBuilder()
                {
                    DataSource = DB,
                    Mode = SqliteOpenMode.ReadWrite,
                };
                _con = new SqliteConnection(sb.ConnectionString);
                await _con.TryOpenAsync().ConfigureAwait(false);

                var cmd = _con.CreateCommand();
                cmd.CommandText = " select Quote($value) ";
                cmd.Parameters.AddWithValue("$value", Pwd);
                var k = await cmd.ExecuteScalarAsync().ConfigureAwait(false);

                cmd = _con.CreateCommand();
                cmd.CommandText = $" PRAGMA key={k} ";
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

                if (comparer != null)
                    _con.CreateCollation(Consts.Order.tccic, comparer.Compare);
            }

            await _con.TryOpenAsync().ConfigureAwait(false);

            return _con;
        }

        public void Dispose()
        {
            _con?.Dispose();
            _con = null;
        }

        //public static string Quote(string unsafeString)
        //{
        //    // TODO: Doesn't call sqlite3_mprintf("%Q", u) because we're waiting on https://github.com/ericsink/SQLitePCL.raw/issues/153
        //    if (unsafeString == null) return "NULL";
        //    var safe = unsafeString.Replace("'", "''");
        //    return "'" + safe + "'";
        //}
    }
}
