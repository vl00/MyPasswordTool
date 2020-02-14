using Common;
using MyPasswordTool.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SilverEx;
using SimpleRedux;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MyPasswordTool
{
    using static rg.SimpleRedux;
    using static SimpleRedux.Global<AppState>;

    public partial class StorePluign : MyPluign
    {
        static readonly string current_cache = Path.Combine(Directory.GetCurrentDirectory(), "cache", ".cache");

        protected override void OnInit()
        {
            var ar = new ActionReducer<AppState>();
            var store = new Store<AppState>(new AppState());
            store.ReplaceReducer(ar.Reducer);
            store.ApplyMiddlewares(TasksMiddleware(before, after), Middlewares.Thunk, Middlewares.AsyncTask);

            var cfg = IoC.Resolve<SimpleInstanceContainer.Config>();
            cfg.AddSingleton<IStore>(store);
            cfg.AddSingleton(store);
            Export(store);
            Export(ar);
            Export(() => store.GetState().Tasks);

            @state.JX = jx_load();
            @state.DbConnInfo = new DbConnInfo { DB = (string)(@state.JX?["db"]) };

            add_reducers();
            this.TryInit();
        }

        protected override void OnDispose()
        {
            this.TryClearup();
            jx_save(@state.JX.ToJson());
        }

        private static JToken jx_load()
        {
            if (!File.Exists(current_cache)) return null;
            var cache = File.ReadAllText(current_cache);
            return jx_load2(Path.Combine("cache", cache));
        }

        private static JObject jx_load2(string path)
        {
            if (!File.Exists(path)) return new JObject();
            using (var fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    var s = sr.ReadToEnd();
                    return JObject.Parse(string.IsNullOrEmpty(s) ? "{}" : s);
                }
            }
        }

        private static void jx_save(string json)
        {
            if (AppPlatform.IsDebuggerAttached) LogManager.GetLogger(null).Info($"cache={json}");
            if (!Directory.Exists("cache")) Directory.CreateDirectory("cache");
            var cache = get_cache_file();
            if (json == null)
            {
                File.Delete(current_cache);
                File.Delete(Path.Combine("cache", cache));
            }
            else
            {
                File.WriteAllText(current_cache, cache, Encoding.UTF8);
                File.WriteAllText(Path.Combine("cache", cache), json, Encoding.UTF8);
            }
        }

        [AutoSubscribe]
        void on_lock(LockActionMessage _)
        {
            if (_.IsLock)
            {
                var json = @state.JX.ToJson();
                Task.Run(() => jx_save(json));
            }
        }

        static string get_cache_file() => get_cache_file(@state.DbConnInfo.DB);

        static string get_cache_file(string f) => $"{Path.GetFileName(f)}.cache";
    }
}