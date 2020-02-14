using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRedux
{
    public static partial class Global<T>
    {
        static ActionReducer<T> _actionReducer;
        static Store<T> _store;
        static Func<IList<Task>> _ftasks;

        public static Store<T> @store => _store;
        public static T @state => _store.GetState();

        public static void Export(Store<T> store) => _store = store;

        public static void Export(ActionReducer<T> actionReducer) => _actionReducer = actionReducer;

        public static void Export(Func<IList<Task>> ftasks) => _ftasks = ftasks;

        public static void @dispatch<TPayload>(string type, TPayload payload) => @dispatch(DyAction.Create(type, payload));

        public static void @dispatch(object action) => @dispatchAsync(action);

        public static Task @dispatchAsync<TPayload>(string type, TPayload payload) => @dispatchAsync(DyAction.Create(type, payload));

        public static Task @dispatchAsync(object action)
        {
            var o = _store.Dispatch(action);
            if (o is Task t) return t;
            return Task.CompletedTask;
        }

        public static IList<Task> @tasks => _ftasks();

        public static Middleware<T> TasksMiddleware(Action<T, object> before, Action<T> after)
        {
            return _;

            Func<Dispatcher, Dispatcher> _(Dispatcher dispatch, Func<T> getState)
            {
                return next => action => __(next, action, null);

                Task __(Dispatcher next, object action, IList<Task> ts)
                {
                    before?.Invoke(getState(), action);
                    ts = @tasks;
                    var o = next(action);
                    if (o is Task t) ts.Add(t);
                    after?.Invoke(getState());
                    return Task.WhenAll(ts);
                }
            }
        }

        public static Action @action<TAction>(Func<T, TAction, T> fn) => _actionReducer.OnAction(fn);

        public static Action @action<TPayload>(string type, Func<T, TPayload, T> fn) => _actionReducer.OnAction(type, fn);

        public static UnSubscribe @subscribe<TAction>(Func<TAction, Task> func) => @subscribe(typeof(TAction).FullName, func);

        public static UnSubscribe @subscribe<TAction>(Action<TAction> action) => @subscribe(typeof(TAction).FullName, action);

        public static UnSubscribe @subscribe<TPayload>(string type, Action<TPayload> action)
        {
            return @subscribe<TPayload>(type, (_) =>
            {
                action(_);
                return null;
            });
        }

        public static UnSubscribe @subscribe<TPayload>(string type, Func<TPayload, Task> func)
        {
            var _un = _actionReducer.OnAction<TPayload>(type, fa);
            return () => _un();

            T fa(T _, TPayload payload)
            {
                UnSubscribe un = null;
                un = _store.Subscribe(fs);
                return _;

                void fs()
                {
                    un?.Invoke();
                    un = null;
                    var t = func(payload);
                    if (t != null) @tasks?.Add(t);
                }
            }
        }

        public static UnSubscribe @subscribe(Action action) => _store.Subscribe(action);

        public static UnSubscribe @subscribe(Func<Task> func)
        {
            return @subscribe(() => 
            {
                var t = func();
                if (t != null) @tasks?.Add(t);
            });
        }
    }
}