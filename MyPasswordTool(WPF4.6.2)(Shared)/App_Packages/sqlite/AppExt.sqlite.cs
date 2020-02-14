using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace MyPasswordTool
{
    public static partial class AppExt
    {
        public static void InitSqlite3()
        {
            //SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlcipher());
            SQLitePCL.Batteries_V2.Init();
        }

        public static void TryOpen(this DbConnection connection)
        {
            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                connection.Open();
        }

        public static Task TryOpenAsync(this DbConnection connection)
        {
            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                return connection.OpenAsync();
            return Task.CompletedTask;
        }
    }
}