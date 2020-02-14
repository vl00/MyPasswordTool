using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    internal class VirtualConnectionClient : IDisposable
    {
        private readonly SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _writerSemaphore = new SemaphoreSlim(1);
        private readonly ConcurrentDictionary<long, Lazy<VirtualConnection>> _virtualConnections = new ConcurrentDictionary<long, Lazy<VirtualConnection>>();
        long _nextInnerStreamId;

        Connection connection;
        readonly Client client;
        private readonly CancellationTokenSource _disposalCancellatonToken = new CancellationTokenSource();
        private bool _disposedValue = false;

        public static int MaxFrameBodyLength = 64 * 1024; //8 + 4 + 

        public VirtualConnectionClient(Client client)
        {
            this.client = client;
        }

        public VirtualConnection OpenVirtualConnection()
        {
            connection.Closing.ThrowIfCancellationRequested();

            var id = Interlocked.Increment(ref _nextInnerStreamId);
            var v = new Lazy<VirtualConnection>(
                () => new VirtualConnection(id, connection, WriteAsync, (c) => _virtualConnections.TryRemove(c.Id, out var __)), true);

            if (!_virtualConnections.TryAdd(id, v))
                throw new Exception($"id:{id} is exists");

            return v.Value;
        }

        async void Start()
        {
            try
            {
                while (!_disposalCancellatonToken.IsCancellationRequested)
                {
                    var data = await ReadNextFrameAsync().ConfigureAwait(false);
                    if (data.Id == null || data.BodyLength != (data.Body?.Length ?? -1))
                    {
                        connection.Close(); //CloseAllActiveStreams
                        return;
                    }

                    var id = data.Id.Value;
                    if (!_virtualConnections.TryGetValue(id, out var v))
                        throw new Exception($"read loop:: id:{id} is not exists");

                    var virtualConnection = v.Value;
                    virtualConnection.AddDataToQueue(data.Body);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is TaskCanceledException || ex is ObjectDisposedException))
                {
                    connection.Close();
                    throw ex;
                }
            }
        }

        public async Task ComunicationAsync(Func<VirtualConnection, Task> func)
        {
            if (connection == null)
            {
                await _connectionSemaphore.WaitAsync();
                try
                {
                    if (connection == null)
                    {
                        connection = await client.NewConnection();
                        Start();
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    _connectionSemaphore.Release();
                }
            }
            await connection.Open();
            await func(OpenVirtualConnection());
        }

        async Task<(long? Id, int BodyLength, byte[] Body)> ReadNextFrameAsync()
        {
            var frameHeaderBuffer = await ReadExactLength(connection, 12).ConfigureAwait(false);
            if (frameHeaderBuffer == null)
            {
                return (null, -1, null); // Underlying stream was closed
            }

            var id = BitConverter.ToInt64(frameHeaderBuffer, 0);
            var frameBodyLength = BitConverter.ToInt32(frameHeaderBuffer, 8);
            if (frameBodyLength > MaxFrameBodyLength)
            {
                throw new InvalidDataException("Illegal frame length: " + frameBodyLength);
            }

            var frameBody = await ReadExactLength(connection, frameBodyLength).ConfigureAwait(false);
            //if (frameBody == null)
            //{
            //    return (null, frameBodyLength, null); // Underlying stream was closed
            //}

            return (id, frameBodyLength, frameBody);
        }

        static async Task<byte[]> ReadExactLength(Connection connection, int lengthToRead)
        {
            var buffer = lengthToRead > -1 ? new byte[lengthToRead] : null;
            for (var totalBytesRead = 0; totalBytesRead < lengthToRead;)
            {
                var chunkLengthRead = await connection.ReadAsync(buffer, totalBytesRead, lengthToRead - totalBytesRead).ConfigureAwait(false);
                if (chunkLengthRead == 0)
                {
                    // Underlying stream was closed
                    return null;
                }
                totalBytesRead += chunkLengthRead;
            }
            return buffer;
        }

        async Task WriteAsync(long frameId, byte[] data, CancellationToken cancellationToken)
        {
            var count = data != null ? data.Length : -1;
            if (count > MaxFrameBodyLength)
                throw new InvalidDataException("Illegal frame length: " + count);

            connection.Closing.ThrowIfCancellationRequested();

            await _writerSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var nextChunkBodyLength = Math.Min(MaxFrameBodyLength, count);
                await connection.WriteAsync(BitConverter.GetBytes(frameId), 0, 8, cancellationToken).ConfigureAwait(false);
                await connection.WriteAsync(BitConverter.GetBytes(nextChunkBodyLength), 0, 4, cancellationToken).ConfigureAwait(false);
                if (nextChunkBodyLength > 0) await connection.WriteAsync(data, 0, nextChunkBodyLength, cancellationToken).ConfigureAwait(false);
                await connection.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _writerSemaphore.Release();
            }
        }

        public void Dispose()
        {
            if (!_disposedValue)
            {
                _disposedValue = true;

                _disposalCancellatonToken.Cancel();
                connection?.Dispose();
            }
        }
    }
}