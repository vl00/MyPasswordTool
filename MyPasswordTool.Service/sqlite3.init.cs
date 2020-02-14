using System;

namespace MyPasswordTool.Service
{
    public class Sqlite3_Bootstartup : IBootstartup
    {
        void IBootstartup.Init()
        {
            AppExt.InitSqlite3();
        }

        void IDisposable.Dispose() { }
    }
}