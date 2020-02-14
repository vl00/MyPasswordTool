using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public static class AsyncHelper
    {
        public static Task<T> RunAsTask<T>(TaskCompletionSource<T> tcs, Action<TaskCompletionSource<T>> action)
        {
            if (tcs == null) throw new ArgumentNullException(nameof(tcs));
            if (action == null) throw new ArgumentNullException(nameof(action));
            action.Invoke(tcs);
            return tcs.Task;
        }

        public static Task<T> RunAsTask<T>(Action<TaskCompletionSource<T>> action)
        {
            return RunAsTask(new TaskCompletionSource<T>(), action);
        }

        public static void RunInBatch(Func<Task> func)
        {
            func?.Invoke();
        }
    }
}