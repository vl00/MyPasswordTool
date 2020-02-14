using System;
using System.Threading;

namespace Common
{
    public class Disposable : IDisposable
    {
        private Action _dispose;
        private static readonly Action emptyAction = () => { };

        public Disposable(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            var a = Interlocked.Exchange(ref _dispose, emptyAction);
            a?.Invoke();
        }
    }
}