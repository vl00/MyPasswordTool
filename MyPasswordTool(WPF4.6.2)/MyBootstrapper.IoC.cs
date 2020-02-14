using Common;
using SilverEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static SimpleRedux.Global<MyPasswordTool.Models.AppState>;

namespace MyPasswordTool
{
    partial class MyBootstrapper
    {
        SimpleInstanceContainer root, csp;

        private void _ioc_Configure()
        {
            var cfg = new SimpleInstanceContainer.Config();
            root = cfg.CreateContainer();
            cfg.AddSingleton(cfg);
            csp = root.CreateScopeContainer();

            IoC.SetResolve((type, name) => _resolve_obj(type, name));
            IoC.SetResolveAll((type) => _resolve_all_obj(type));
            //IoC.SetInject((o) => { throw new NotImplementedException(); });

            add_to_spool(cfg);
        }

        private object _resolve_obj(Type type, string name)
        {
            if (csp.TryGet(type, name, out var o)) return o;
            return Activator.CreateInstance(type);
        }

        private IEnumerable<object> _resolve_all_obj(Type type)
        {
            return csp.GetAll(type).OfType<object>();
        }

        private void add_to_spool(SimpleInstanceContainer.Config cfg)
        {
            cfg.AddSingleton(Logger = new DebugLogger());

            cfg.AddSingleton(this);
            cfg.AddSingleton(Consts.app_exit, (s) => dpool);

            cfg.AddScoped((s) => @state?.DbConnInfo);
            cfg.AddScoped((s) => new Models.DalRepository());
            cfg.AddSingleton(s => new Models.CultureInfoComparer());

            cfg.AddScoped(s => new MainPage());

            cfg.AddSingleton((s) => new Lazy<MyPluign>(() => new StorePluign()));
            cfg.AddSingleton((s) => new Lazy<MyPluign>(() => new GCPluign()));
            cfg.AddSingleton((s) => new Lazy<MyPluign>(() => new BackgroundMsgPluign()));

            cfg.AddSingleton<Consts.MapKeyToViewType>((s) => key => Type.GetType(key.TrimStart("_$view:".ToCharArray())));
            cfg.AddSingleton<Consts.MapViewTypeToKey>((s) => type => $"_$view:{type.AssemblyQualifiedName}");

            cfg.AddSingleton((s) => new Func<MyFrame, TransitioningContentControl>(getTransitioningContentControl));
        }

        [@rg.SimpleRedux.AutoSubscribe(Consts.DyActType.before_unlock_ok)]
        void at_before_unlock_ok((bool is_first_unlock, string new_conn, string old_conn) _)
        {
            if (_.is_first_unlock || _.new_conn != _.old_conn)
            {
                csp?.Dispose();
                csp = root.CreateScopeContainer();
            }
        }
    }
}
