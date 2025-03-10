using System.Text;

namespace Whois.NET
{
    internal interface IQueryOptions
    {
        /// <summary>
        /// Gets the encoding method to decode the result of query. This default value is ASCII encoding.
        /// </summary>
        Encoding Encoding { get; }

        /// <summary>
        /// Gets a timespan to limit the connection attempt, in seconds. The default value is 600 seconds.
        /// </summary>
        int Timeout { get; }

        /// <summary>
        /// Gets the number of times a connection will be attempted. The default value is 10.
        /// </summary>
        int Retries { get; }

        /// <summary>
        /// Gets whether rethrow any caught exceptions instead of swallowing them. The default value is false.
        /// </summary>
        bool RethrowExceptions { get; }

        /// <summary>
        /// Gets or sets connection manager instance. Replace it if you need to create a connection in specific way, for example, over SOCKS proxy.
        /// </summary>
        ITcpConnector TcpConnector { get; }
    }
}
