using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    internal interface IConnectionFactory : IDisposable
    {
        Task<Connection> WaitConnectionAsync(string address);
    }

    internal abstract class Connection : IDisposable
    {
        private CancellationTokenSource _close;

        public string Id { get; } = Guid.NewGuid().ToString();

        public CancellationToken Closing => _close.Token;

        public abstract Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);
        public abstract Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);
        public abstract Task FlushAsync(CancellationToken cancellationToken = default);

        public virtual Task Open()
        {
            _close = _close ?? new CancellationTokenSource();
            return Task.CompletedTask;
        }

        public virtual void Close()
        {
            _close?.Cancel();
            _close = null;
        }

        public virtual void Dispose()
        {
            if (_close?.IsCancellationRequested != true) _close?.Cancel();
            _close = null;
        }
    }
}