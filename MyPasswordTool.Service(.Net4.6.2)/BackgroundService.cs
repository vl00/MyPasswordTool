using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using MyPasswordTool.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPasswordTool.Service
{
    public partial class BackgroundService
    {
        private static readonly Task<object> undefinedTask = Task.FromResult(new object());

        private static readonly Dictionary<string, Delegate> tododict = new Dictionary<string, Delegate>
        {
            //[null] = null, //error
            [""] = null,
        };

        //// ?? not reactive for services if define static.ctor
        //static BackgroundService()  
        //{
        //    undefinedTask = Task.FromResult(new object());
        //    tododict = new Dictionary<string, Delegate>
        //    {
        //        [""] = null,
        //    };
        //}

        protected static Delegate @void(string type, Action<BackgroundService, BackgroundMessage> action) => tododict[type] = action;
        protected static Delegate @func(string type, Func<BackgroundService, BackgroundMessage, Task> func) => tododict[type] = func;
        protected static Delegate @task(string type, Func<BackgroundService, BackgroundMessage, Task<object>> func) => tododict[type] = func;
        protected static Delegate @avoid(string type, Func<BackgroundService, BackgroundMessage, Task> action) => @void(type, (self, msg) => Task.Run(() => action(self, msg)));

        public Task<object> HandleBackgroundMessageWithResultAsync(BackgroundMessage msg)
        {
            var type = msg?.Token;
            Task<object> r = null;
            if (tododict.TryGetValue(type, out var dofunc))
            {
                r = r ?? dof<Action<BackgroundService, BackgroundMessage>>(dofunc, f => f(this, msg));
                r = r ?? dof<Func<BackgroundService, BackgroundMessage, Task>>(dofunc, f => f(this, msg));
                r = r ?? dof<Func<BackgroundService, BackgroundMessage, Task<object>>>(dofunc, f => f(this, msg));
            }
            return r ?? undefinedTask;
        }

        static Task<object> dof<TDelegate>(Delegate f, Action<TDelegate> todo) where TDelegate : class
        {
            if (f is TDelegate d && todo != null)
            {
                todo(d);
                return undefinedTask;
            }
            return null;
        }

        static Task<object> dof<TDelegate>(Delegate f, Func<TDelegate, Task<object>> todo) where TDelegate : class
        {
            if (f is TDelegate d && todo != null)
                return todo(d) ?? undefinedTask;
            return null;
        }
    }

    public partial class BackgroundService
    {
        private const string str_del_not_in_tree = "del_not_in_tree";
        private const string str_test_simpainfo = "_test_simpainfo";
        private const string str_vacuum = "db_vacuum";

        static readonly object __f_Test =
            @void("test", (instance, msg) => instance.Test(msg));
        public void Test(BackgroundMessage msg)
        {
            LogManager.GetLogger(typeof(BackgroundService)).Info($"test ok:{msg.ToJson()}");
        }

        static readonly object __f_Test1 =
            @func("test1", (instance, msg) => instance.Test1(msg));
        public async Task Test1(BackgroundMessage msg)
        {
            LogManager.GetLogger(typeof(BackgroundService)).Info($"test1 ok:{msg.ToJson()}");
            await undefinedTask;
        }

        static readonly object __f_Test2 =
            @task("test2", (instance, msg) => instance.Test2(msg));
        public async Task<object> Test2(BackgroundMessage msg)
        {
            LogManager.GetLogger(typeof(BackgroundService)).Info($"test2 ok:{msg.ToJson()}");
            return await undefinedTask;
        }

        static readonly object __f_vacuum = 
            @func(str_vacuum, (instance, msg) =>
                instance.vacuum((string)msg.Message?["db"], (string)msg.Message?["pwd"]));
        public async Task vacuum(string db, string pwd)
        {
            using (var x = new DbConnInfo() { DB = db, Pwd = pwd })
            {
                var con = await x.GetConnAsync();
                await con.ExecuteAsync("vacuum");
            }
        }

        static readonly object __f_Exit =
            @avoid("app_exit", (instance, msg) =>
                instance.Exit((string)msg.Message?["db"], (string)msg.Message?["pwd"]));
        public async Task Exit(string db, string pwd)
        {
            LogManager.GetLogger(this.GetType()).Info("app exiting ...");
            if (db != null)
            {
                try { await vacuum(db, pwd); }
                catch { }
            }
            Program.CheckCanExit(() =>
            {
                LogManager.GetLogger(this.GetType()).Info("app exited ok");
            });
        }
    
        static readonly object __f_CleanTree = 
            @func(str_del_not_in_tree, (instance, msg) => 
                instance.CleanTree((string)msg.Message?["db"], (string)msg.Message?["pwd"]));
        public Task CleanTree(string db, string pwd)
        {
            using (var x = new DbConnInfo() { DB = db, Pwd = pwd })
                return CleanTree(x);
        }
        async Task CleanTree(DbConnInfo x)
        {
            var sql = @"
WITH RECURSIVE tb('ID','Name','PID','Order','HasChild') AS (
    SELECT * from PaTag where PID>0 and PID not in (select ID from PaTag)
    UNION ALL
    SELECT a.* FROM PaTag a INNER JOIN tb b ON a.PID=b.ID order by a.PID,a.ID
) select * from tb;
";
            var con = await x.GetConnAsync();
            var ts = (await con.QueryAsync<PaTag>(sql)).Select(t => t.ID).ToArray();
            if (ts.Length != 0)
            {
                await Task.WhenAll(
                        con.ExecuteAsync($"delete from PaTag where ID in ({string.Join(",", ts)});"),
                        con.ExecuteAsync($"delete from PaInfoTag where TagID in ({string.Join(",", ts)});")
                    );
                await con.ExecuteAsync("vacuum");
            }
        }

        static readonly object __f_deltag = 
            @func("deltag", (instance, msg) => 
                instance.DelTag(
                    (int)msg.Message["ID"],
                    (int)msg.Message["PID"],
                    (string)msg.Message["db"],
                    (string)msg.Message["pwd"]
                ));
        public async Task DelTag(int id, int pid, string db, string pwd)
        {
            using (var x = new DbConnInfo() { DB = db, Pwd = pwd })
            {
                var con = await x.GetConnAsync();

                var t1 = Task.Run(async delegate
                {
                    var c = await con.ExecuteScalarAsync<int>("SELECT count(1) from PaTag WHERE PID=@pid and ID<>@id", new { pid, id });
                    await con.ExecuteAsync("update PaTag set HasChild=@c where ID=@pid", new { pid, c = (c > 0 ? 1 : 0) });
                    await con.ExecuteAsync("delete from PaTag where ID=@id;", new { id });
                });
                var t2 = con.ExecuteAsync("delete from PaInfoTag where TagID=@id;", new { id });
                await Task.WhenAll(t1, t2);

                await CleanTree(x);
            }
        }
    }
}