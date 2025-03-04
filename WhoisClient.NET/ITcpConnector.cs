using System.Net.Sockets;
using System.Threading.Tasks;

namespace Whois.NET
{
    /// <summary>
    /// TCP connection manager
    /// </summary>
    public interface ITcpConnector
    {
        /// <summary>
        /// Connect to specified endpoint and return the resulting <see cref="TcpClient"/>
        /// </summary>
        /// <param name="server">Server hostname or address</param>
        /// <param name="port">TCP port</param>
        /// <returns><see cref="TcpClient"/> connected to the specified endpoint</returns>
        Task<TcpClient> ConnectAsync(string server, int port);
    }
}
