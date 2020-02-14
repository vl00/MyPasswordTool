using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Common
{
    internal class VirtualConnection : IDisposable
    {
        private readonly BufferBlock<byte[]> _receivedDataQueue = new BufferBlock<byte[]>();
        private readonly Func<long, byte[], CancellationToken, Task> writer;
        private Action<VirtualConnection> disposeAction;
        //private bool _isDisposed => disposeAction != null;

        public long Id { get; }
        public Connection Connection { get; }

        internal VirtualConnection(long id, Connection connection, Func<long, byte[], CancellationToken, Task> writer, Action<VirtualConnection> disposeAction)
        {
            Id = id;
            Connection = connection;
            this.writer = writer;
            this.disposeAction = disposeAction;
        }

        public Task<byte[]> ReadAsync(CancellationToken cancellationToken = default)
        {
            if (Connection.Closing.IsCancellationRequested)
                throw new InvalidOperationException($"{nameof(Connection)} is closed");

            return _receivedDataQueue.ReceiveAsync(cancellationToken);
        }

        public Task WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
        {
            if (Connection.Closing.IsCancellationRequested)
                throw new InvalidOperationException($"{nameof(Connection)} is closed");

            return writer(Id, buffer, cancellationToken);
        }

        public void Dispose()
        {
            if (disposeAction == null) return;
            disposeAction(this);
            disposeAction = null;
        }

        public async void AddDataToQueue(byte[] data)
        {
            await _receivedDataQueue.SendAsync(data).ConfigureAwait(false);
        }
    }
}