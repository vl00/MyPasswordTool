using SilverEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    class VirtualConnectionServer
    {
        private readonly SemaphoreSlim _writerSemaphore = new SemaphoreSlim(1);
        private readonly ConcurrentDictionary<long, Lazy<VirtualConnection>> _virtualConnections = new ConcurrentDictionary<long, Lazy<VirtualConnection>>();

        readonly Connection connection;
        readonly Server server;

        public static int MaxFrameBodyLength = 64 * 1024; //8 + 4 + 

        event Action<VirtualConnection> OnConnectionFrameStarted;

        private VirtualConnectionServer(Server server, Connection connection)
        {
            this.server = server;
            this.connection = connection;
        }

        public static Action<Action<VirtualConnection>> At(Server server)
        {
            return _rf;

            void _rf(Action<VirtualConnection> action)
            {
                server.OnClientConnected += _OnClientConnected_;

                void _OnClientConnected_(Connection connection)
                {
                    var ss = new VirtualConnectionServer(server, connection);
                    ss.OnConnectionFrameStarted += action;
                    ss.OnClientConnected();
                }
            }
        }

        async void OnClientConnected()
        {
            while (!server.Stopping.IsCancellationRequested)
            {
                try
                {
                    //await connection.Open().ConfigureAwait(false);
                    var data = await ReadNextFrameAsync().ConfigureAwait(false);
                    if (data.Id == null || data.BodyLength != (data.Body?.Length ?? -1))
                    {
                        connection.Close(); //CloseAllActiveStreams
                        return;
                    }

                    var id = data.Id.Value;
                    var v = _virtualConnections.GetOrAdd(id, (_) => new Lazy<VirtualConnection>(
                        () => new VirtualConnection(id, connection, WriteAsync, (c) => _virtualConnections.TryRemove(c.Id, out var __)), true));
                    var first = !v.IsValueCreated;
                    var virtualConnection = v.Value;
                    virtualConnection.AddDataToQueue(data.Body);
                    if (first)
                    {
                        OnConnectionFrameStarted?.Invoke(virtualConnection);
                    }
                }
                catch (Exception ex)
                {
                    if (!(ex is TaskCanceledException))
                        throw;
                }
            }
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
    }
}