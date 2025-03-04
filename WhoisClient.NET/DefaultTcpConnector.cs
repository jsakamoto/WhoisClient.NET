using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Whois.NET
{
    internal sealed class DefaultTcpConnector : ITcpConnector
    {
        public static DefaultTcpConnector Instance { get; } = new DefaultTcpConnector();

        private DefaultTcpConnector()
        { }

        public async Task<TcpClient> ConnectAsync(string server, int port)
        {
            if (string.IsNullOrWhiteSpace(server))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(server));
            }

            if (port <= 0 || port > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            var tcpClient = new TcpClient();

            await tcpClient.ConnectAsync(server, port);

            return tcpClient;
        }
    }
}
