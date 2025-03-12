using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Whois.NET
{
    /// <summary>
    /// Option parameters for whois query.
    /// </summary>
    public class WhoisQueryOptions : IQueryOptions
    {
        /// <summary>
        /// Gets or sets the FQDN of whois server. This default value is "whois.iana.org".
        /// </summary>
        public string Server { get; set; } = "whois.iana.org";

        /// <summary>
        /// Gets or sets the TCP port number to connect whois server. This default value is 43.
        /// </summary>
        public int Port { get; set; } = 43;

        /// <summary>
        /// Gets or sets the encoding method to decode the result of query. This default value is ASCII encoding.
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.ASCII;

        /// <summary>
        /// Gets or sets a timespan to limit the connection attempt, in milliseconds. The default value is 2000 msec.
        /// </summary>
        public int Timeout { get; set; } = 2000;

        /// <summary>
        /// Gets or sets the number of times a connection will be attempted. The default value is 3.
        /// </summary>
        public int Retries { get; set; } = 3;

        /// <summary>
        /// Gets or sets whether rethrow any caught exceptions instead of swallowing them. The default value is false.
        /// </summary>
        public bool RethrowExceptions { get; set; } = false;

        /// <summary>
        /// Gets or sets the function to create a TCP connection to a whois server asynchronously.<br/>
        /// Replace it if you need to create a connection in a specific way, for example, over a SOCKS proxy.
        /// </summary>
        public Func<TcpConnectionArgs, Task<TcpClient>> ConnectAsync { get; set; }
    }
}
