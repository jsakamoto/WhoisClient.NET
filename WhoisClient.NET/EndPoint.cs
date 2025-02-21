namespace Whois.NET
{
    internal class EndPoint
    {
        public string Host { get; }

        public int Port { get; }

        public EndPoint(string host, int port)
        {
            this.Host = host;
            this.Port = port;
        }
    }
}
