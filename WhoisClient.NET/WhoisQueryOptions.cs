using System.Text;

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
        /// Gets or sets a timespan to limit the connection attempt, in seconds. The default value is 600 seconds.
        /// </summary>
        public int Timeout { get; set; } = 600;

        /// <summary>
        /// Gets or sets the number of times a connection will be attempted. The default value is 10.
        /// </summary>
        public int Retries { get; set; } = 10;

        /// <summary>
        /// Gets or sets whether rethrow any caught exceptions instead of swallowing them. The default value is false.
        /// </summary>
        public bool RethrowExceptions { get; set; } = false;
    }
}
