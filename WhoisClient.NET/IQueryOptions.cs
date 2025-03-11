using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Whois.NET
{
    internal interface IQueryOptions
    {
        /// <summary>
        /// Gets the encoding method to decode the result of query. This default value is ASCII encoding.
        /// </summary>
        Encoding Encoding { get; }

        /// <summary>
        /// Gets a timespan to limit the connection attempt, in milliseconds. The default value is 2000 msec.
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
        /// Gets the function to create a TCP connection to a whois server asynchronously.<br/>
        /// This property allows us to override a way of creating a connection.
        /// </summary>
        Func<TcpConnectionArgs, Task<TcpClient>> ConnectAsync { get; }
    }
}
