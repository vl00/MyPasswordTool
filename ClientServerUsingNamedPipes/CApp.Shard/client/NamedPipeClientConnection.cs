using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    class NamedPipeClientConnectionFactory : IConnectionFactory
    {
        //private readonly ConcurrentBag<Connection>

        public async Task<Connection> WaitConnectionAsync(string address)
        {
            var _namedPipeClientStream = new NamedPipeClientStream(
                ".",
                address,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            await _namedPipeClientStream.ConnectAsync().ConfigureAwait(false);

            var connection = new NamedPipeClientConnection(this, _namedPipeClientStream);
            await connection.Open().ConfigureAwait(false);
            //connection.Closing.Register(() => { });

            return connection;
        }

        public void Dispose() { }
    }

    class NamedPipeClientConnection : Connection
    {
        private readonly NamedPipeClientStream _namedPipeClientStream;
        private readonly NamedPipeClientConnectionFactory _connectionFactory;

        public NamedPipeClientConnection(NamedPipeClientConnectionFactory connectionFactory, NamedPipeClientStream namedPipeClientStream)
        {
            _connectionFactory = connectionFactory;
            _namedPipeClientStream = namedPipeClientStream;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
            => _namedPipeClientStream.ReadAsync(buffer, offset, count, CancellationTokenSource.CreateLinkedTokenSource(Closing, cancellationToken).Token);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
            => _namedPipeClientStream.WriteAsync(buffer, offset, count, CancellationTokenSource.CreateLinkedTokenSource(Closing, cancellationToken).Token);

        public override Task FlushAsync(CancellationToken cancellationToken = default)
            => _namedPipeClientStream.FlushAsync(CancellationTokenSource.CreateLinkedTokenSource(Closing, cancellationToken).Token);

        public override async Task Open()
        {
            await base.Open().ConfigureAwait(false);
            if (!_namedPipeClientStream.IsConnected)
                await _namedPipeClientStream.ConnectAsync().ConfigureAwait(false);
        }

        public override void Close()
        {
            if (_namedPipeClientStream.IsConnected) _namedPipeClientStream.Close();
            base.Close();
        }

        public override void Dispose()
        {
            base.Dispose();
            _namedPipeClientStream.Dispose();
        }
    }
}