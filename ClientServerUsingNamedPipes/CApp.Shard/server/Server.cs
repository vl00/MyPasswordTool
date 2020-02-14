using SilverEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    class Server
    {
        private readonly CancellationTokenSource _disposalCancellatonToken = new CancellationTokenSource();
        private bool _disposedValue = false;

        private readonly string _address;
        private readonly ILogger _log;
        private readonly IConnectionFactory _connectionFactory;

        public CancellationToken Stopping => _disposalCancellatonToken.Token;

        public event Action<Connection> OnClientConnected;
        public event Action<Connection> OnClientDisconnected;

        public Server(string address, ILogger log, IConnectionFactory connectionFactory)
        {
            _address = address;
            _log = log;
            _connectionFactory = connectionFactory;
        }

        public async void Start()
        {
            while (!Stopping.IsCancellationRequested)
            {
                try
                {
                    var connection = await _connectionFactory.WaitConnectionAsync(_address).ConfigureAwait(false);

                    connection.Closing.Register(() => On_ClientDisconnected(connection));
                    //Stopping.Register(connection.Dispose);

                    On_ClientConnected(connection);
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            }
        }

        public void Dispose()
        {
            if (!_disposedValue)
            {
                _disposedValue = true;

                _disposalCancellatonToken.Cancel();

                _connectionFactory.Dispose();
                OnClientConnected = null;
                OnClientDisconnected = null;
            }
        }

        void On_ClientConnected(Connection connection) => OnClientConnected?.Invoke(connection);
        void On_ClientDisconnected(Connection connection) => OnClientDisconnected?.Invoke(connection);
    }
}