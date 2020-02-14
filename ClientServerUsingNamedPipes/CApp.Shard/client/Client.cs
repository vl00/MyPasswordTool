using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    class Client
    {
        private readonly string _address;
        private readonly IConnectionFactory _connectionFactory;

        public Client(string address, IConnectionFactory connectionFactory)
        {
            _address = address;
            _connectionFactory = connectionFactory;
        }

        public Task<Connection> NewConnection()
        {
            return _connectionFactory.WaitConnectionAsync(_address);
        }

        public void Dispose()
        {
            _connectionFactory?.Dispose();
        }
    }
}