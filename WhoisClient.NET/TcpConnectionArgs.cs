using System.Threading;

namespace Whois.NET
{
    /// <summary>
    /// Represents the arguments required for establishing a TCP connection.
    /// </summary>
    public class TcpConnectionArgs
    {
        /// <summary>
        /// Gets the host name or IP address of the server.
        /// </summary>
        public string Host { get; internal set; }

        /// <summary>
        /// Gets the TCP port number to connect to on the server.
        /// </summary>
        public int Port { get; internal set; }

        /// <summary>
        /// Gets the cancellation token to monitor for cancellation requests.
        /// </summary>
        public CancellationToken CancellationToken { get; internal set; }
    }
}
