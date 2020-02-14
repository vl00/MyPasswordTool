using Common;
using MyPasswordTool.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MyPasswordTool
{
    using static SimpleRedux.Global<AppState>;

    public partial class StorePluign
    {
        void before(AppState _, object action)
        {
            _.ActionContexts.Push((action, new List<Task>()));
        }

        void after(AppState _)
        {
            _.prevdb = null;
            _.TryPopActionContext();
        }

        void add_reducers()
        {
            @action<(bool, string, string)>(Consts.DyActType.before_unlock_ok, at_before_unlock_ok);
            @action<LockActionMessage>(on_LockActionMessage);
            @action<TagTreeStoreData>(on_TagTreeStoreData);
            @action<PaListScrollData>(on_PaListScrollData);
            @action<ChangeSearchText>(on_ChangeSearchText);
            @action<PaListStoreData>(on_PaListStoreData);
            @action<string>(Consts.DyActType.palsOrderChanged, on_palsOrderChanged);
        }

        AppState at_before_unlock_ok(AppState appState, (bool is_first_unlock, string new_conn, string old_conn) _)
        {
            if (_.new_conn != _.old_conn)
            {
                appState.JX = jx_load2(Path.Combine("cache", get_cache_file(_.new_conn)));
            }
            return appState;
        }

        AppState on_LockActionMessage(AppState appState, LockActionMessage action)
        {
            if (action?.IsLock == false)
            {
                if (appState.DbConnInfo?.DB != action.DbInfo.DB) appState.DbConnInfo?.Dispose();
                appState.DbConnInfo = action.DbInfo;
            }

            {// $appstate->JX
                if (!action.IsLock)
                {
                    var db1 = (string)appState.JX?["db"];
                    var db2 = action.DbInfo?.DB;
                    appState.prevdb = db1;
                    if (db1 != db2)
                    {
                        if (db2 == null) appState.JX = null;
                        else
                        {
                            var jx = new JObject { ["db"] = db2 };
                            jx.Merge(jx_load2(Path.Combine("cache", get_cache_file(db2))));
                            appState.JX = jx;
                        }
                    }
                }
            }
            {// $appstate->JX->db
                if (!action.IsLock)
                {
                    var j = appState.JX = appState.JX ?? new JObject();
                    var db = action.DbInfo?.DB;
                    if (db != null) j["db"] = JToken.FromObject(db);
                    else j["db"]?.Remove();
                    appState.JX = j;
                }
            }

            return appState;
        }

        AppState on_TagTreeStoreData(AppState appState, TagTreeStoreData action)
        {
            //$appstate->JX->TagTree

            appState.JX = appState.JX ?? new JObject();
            var j = appState.JX["TagTree"] = appState.JX["TagTree"] ?? new JObject();

            j["Selected"] = action.Selected;
            j["Expanded"] = action.Expanded;

            appState.JX["TagTree"] = j;
            return appState;
        }

        AppState on_PaListScrollData(AppState appState, PaListScrollData action)
        {
            // $appstate->JX->PaList->voff
            appState.JX = appState.JX ?? new JObject();
            var j = appState.JX["PaList"] = appState.JX["PaList"] ?? new JObject();

            {
                if ((action.Voff ?? 0) == 0) j["voff"]?.Parent.Remove();
                else j["voff"] = action.Voff;
            }
            
            appState.JX["PaList"] = j;
            return appState;
        }

        AppState on_PaListStoreData(AppState appState, PaListStoreData action)
        {
            // $appstate->JX->PaList
            appState.JX = appState.JX ?? new JObject();
            var j = appState.JX["PaList"] = appState.JX["PaList"] ?? new JObject();

            {
                foreach (var _j in JObject.FromObject(action, JsonSerializer.Create(JsonNetExtensions.SerializerSettings)).Properties())
                    j[_j.Name] = _j?.Value;
            }

            appState.JX["PaList"] = j;
            return appState;
        }

        AppState on_ChangeSearchText(AppState appState, ChangeSearchText action)
        {
            // $appstate->JX->searchtext
            var jx = appState.JX = appState.JX ?? new JObject();
            if (action.Text.IsNullOrEmpty() || action.Text.Trim().IsNullOrEmpty())
                jx["searchtext"]?.Parent?.Remove();
            else
                jx["searchtext"] = action.Text;

            appState.JX = jx;
            return appState;
        }

        // Consts.DyActType.palsOrderChanged
        AppState on_palsOrderChanged(AppState appState, string action)
        {
            // $appstate->JX->PaList->Order
            appState.JX = appState.JX ?? new JObject();
            var j = appState.JX["PaList"] = appState.JX["PaList"] ?? new JObject();

            {
                if (string.IsNullOrEmpty(action) || action == Consts.Order.UpdateTime) j["Order"]?.Parent.Remove();
                else j["Order"] = action;
            }

            appState.JX["PaList"] = j;
            return appState;
        }
    }
}