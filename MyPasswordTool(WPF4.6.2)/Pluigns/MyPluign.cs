using System;

namespace MyPasswordTool
{
    public abstract class MyPluign : IDisposable
    {
        private bool is_init;
        private bool is_disposed;

        protected virtual void OnInit() { }
        protected virtual void OnDispose() { }

        public void Init()
        {
            var first = !is_init;
            if (first)
            {
                is_init = true;
                OnInit();
            }
        }

        public void Dispose()
        {
            if (is_disposed) return;
            is_disposed = true;
            OnDispose();
        }
    }
}