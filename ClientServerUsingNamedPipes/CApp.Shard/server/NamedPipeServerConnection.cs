using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    class NamedPipeServerConnectionFactory : IConnectionFactory
    {
        //private readonly ConcurrentBag<Connection>

        public async Task<Connection> WaitConnectionAsync(string address)
        {
            var _namedPipeServerStream = new NamedPipeServerStream(
                    //".",
                    address,
                    PipeDirection.InOut,
                    254,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous,
                    VirtualConnectionServer.MaxFrameBodyLength, VirtualConnectionServer.MaxFrameBodyLength);

            await _namedPipeServerStream.WaitForConnectionAsync().ConfigureAwait(false);

            var connection = new NamedPipeServerConnection(this, _namedPipeServerStream);
            await connection.Open().ConfigureAwait(false);
            //connection.Closing.Register(() => { });

            return connection;
        }

        public void Dispose() { }
    }

    class NamedPipeServerConnection : Connection
    {
        private readonly NamedPipeServerStream _namedPipeServerStream;
        private readonly NamedPipeServerConnectionFactory _connectionFactory;

        public NamedPipeServerConnection(NamedPipeServerConnectionFactory connectionFactory, NamedPipeServerStream namedPipeServerStream)
        {
            _connectionFactory = connectionFactory;
            _namedPipeServerStream = namedPipeServerStream;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
            => _namedPipeServerStream.ReadAsync(buffer, offset, count, CancellationTokenSource.CreateLinkedTokenSource(Closing, cancellationToken).Token);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
            => _namedPipeServerStream.WriteAsync(buffer, offset, count, CancellationTokenSource.CreateLinkedTokenSource(Closing, cancellationToken).Token);

        public override Task FlushAsync(CancellationToken cancellationToken = default)
            => _namedPipeServerStream.FlushAsync(CancellationTokenSource.CreateLinkedTokenSource(Closing, cancellationToken).Token);

        public override async Task Open()
        {
            await base.Open().ConfigureAwait(false);
            if (!_namedPipeServerStream.IsConnected)
                await _namedPipeServerStream.WaitForConnectionAsync().ConfigureAwait(false);
        }

        public override void Close()
        {
            if (_namedPipeServerStream.IsConnected) _namedPipeServerStream.Disconnect();
            base.Close();
        }

        public override void Dispose()
        {
            base.Dispose();
            _namedPipeServerStream.Dispose();
        }
    }
}