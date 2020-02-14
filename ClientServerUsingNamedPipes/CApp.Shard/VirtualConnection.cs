using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    internal class VirtualConnection : IDisposable
    {
        class Factory<T> : IDisposable
        {
            readonly SemaphoreSlim semaphore;
            readonly IProducerConsumerCollection<T> queue;

            public Factory(IProducerConsumerCollection<T> producerConsumerCollection)
            {
                queue = producerConsumerCollection ?? throw new ArgumentNullException(nameof(producerConsumerCollection));
                semaphore = new SemaphoreSlim(1);
            }

            public bool TryAdd(T item)
            {
                var b = queue.TryAdd(item);
                if (b) semaphore.Release();
                return b;
            }

            public async Task<T> TakeAsync(CancellationToken cancellationToken = default)
            {
                //T item;
                //while (!queue.TryTake(out item))
                //{
                //    await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                //}
                //return item;

                while (true)
                {
                    await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                    if (queue.TryTake(out var item))
                        return item;
                }
            }

            public void Dispose()
            {
                semaphore.Dispose();
            }
        }

        readonly Factory<byte[]> _receivedDataQueue = new Factory<byte[]>(new ConcurrentQueue<byte[]>());
        readonly Func<long, byte[], CancellationToken, Task> writer;
        Action<VirtualConnection> disposeAction;

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

            return _receivedDataQueue.TakeAsync(cancellationToken);
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
            _receivedDataQueue.Dispose();
            disposeAction(this);
            disposeAction = null;
        }

        public void AddDataToQueue(byte[] data)
        {
            _receivedDataQueue.TryAdd(data);
        }
    }
}