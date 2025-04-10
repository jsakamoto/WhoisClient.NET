using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Whois.NET
{
    /// <summary>
    /// A WhoisClient structure for quering whois servers.
    /// </summary>
    public class WhoisClient
    {
        /// <summary>
        /// The has referral.
        /// </summary>
        private static readonly Regex _hasReferralRegex = new Regex(
                @"(^ReferralServer:\W+whois://(?<refsvr>[^:\r\n]+)(:(?<port>\d+))?)|" +
                @"(^\s*(Registrar\s+)?Whois Server:\s*(?<refsvr>[^:\r\n]+)(:(?<port>\d+))?)|" +
                @"(^\s*refer:\s*(?<refsvr>[^:\r\n]+)(:(?<port>\d+))?)|" +
                @"(^\s*whois:\s*(?<refsvr>[^:\r\n]+)(:(?<port>\d+))?)|" +
                @"(^remarks:\W+.*(?<refsvr>whois\.[0-9a-z\-\.]+\.[a-z]{2,})(:(?<port>\d+))?)",
                RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Send WHOIS query to WHOIS server, re-query to referral servers recursive, and return the response from WHOIS server.
        /// </summary>
        /// <param name="query">domain name (ex."nic.ad.jp")or IP address (ex."192.41.192.40") to be queried.</param>
        /// <param name="server">FQDN of whois server (ex."whois.arin.net"). This parameter is optional (default value is null) to determine server automatically.</param>
        /// <param name="port">TCP port number to connect whois server. This parameter is optional, and default value is 43.</param>
        /// <param name="encoding">Encoding method to decode the result of query. This parameter is optional (default value is null) to using ASCII encoding.</param>
        /// <param name="timeout">A timespan to limit the connection attempt, in milliseconds.</param>
        /// <param name="retries">The number of times a connection will be attempted.</param>
        /// <returns>The strong typed result of query which responded from WHOIS server.</returns>
        [Obsolete("Use the 'Query(string query, WhoisQueryOptions options)' instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static WhoisResponse Query(string query, string server = null, int port = 43,
            Encoding encoding = null, int timeout = 2000, int retries = 3)
        {
            var options = new WhoisQueryOptions();
            options.Server = !string.IsNullOrEmpty(server) ? server : options.Server;
            options.Port = port;
            options.Encoding = encoding ?? options.Encoding;
            options.Timeout = timeout;
            options.Retries = retries;

            var servers = new List<EndPoint> { new EndPoint(options.Server, options.Port) };
            return QueryRecursive(query, servers, options);
        }

        /// <summary>
        /// Send WHOIS query to WHOIS server, re-query to referral servers recursive, and return the response from WHOIS server.
        /// </summary>
        /// <param name="query">domain name (ex."nic.ad.jp")or IP address (ex."192.41.192.40") to be queried.</param>
        /// <param name="options">The options for the whois query.</param>
        /// <returns>The strong typed result of query which responded from WHOIS server.</returns>
        public static WhoisResponse Query(string query, WhoisQueryOptions options)
        {
            options = options ?? new WhoisQueryOptions();
            var servers = new List<EndPoint> { new EndPoint(options.Server, options.Port) };
            return QueryRecursive(query, servers, options);
        }

        /// <summary>
        /// Send WHOIS query to WHOIS server, re-query to referral servers recursive, and return the response from WHOIS server.
        /// </summary>
        /// <param name="query">domain name (ex."nic.ad.jp")or IP address (ex."192.41.192.40") to be queried.</param>
        /// <param name="server">FQDN of whois server (ex."whois.arin.net"). This parameter is optional (default value is null) to determine server automatically.</param>
        /// <param name="port">TCP port number to connect whois server. This parameter is optional, and default value is 43.</param>
        /// <param name="encoding">Encoding method to decode the result of query. This parameter is optional (default value is null) to using ASCII encoding.</param>
        /// <param name="timeout">A timespan to limit the connection attempt, in milliseconds.</param>
        /// <param name="retries">The number of times a connection will be attempted.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>The strong typed result of query which responded from WHOIS server.</returns>
        [Obsolete("Use the 'QueryAsync(string query, WhoisQueryOptions options, CancellationToken token)' instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static async Task<WhoisResponse> QueryAsync(string query, string server = null, int port = 43,
            Encoding encoding = null, int timeout = 2000, int retries = 3, CancellationToken token = default(CancellationToken))
        {
            var options = new WhoisQueryOptions();
            options.Server = !string.IsNullOrEmpty(server) ? server : options.Server;
            options.Port = port;
            options.Encoding = encoding ?? options.Encoding;
            options.Timeout = timeout;
            options.Retries = retries;

            var servers = new List<EndPoint> { new EndPoint(options.Server, options.Port) };
            return await QueryRecursiveAsync(query, servers, options, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Send WHOIS query to WHOIS server, re-query to referral servers recursive, and return the response from WHOIS server.
        /// </summary>
        /// <param name="query">domain name (ex."nic.ad.jp")or IP address (ex."192.41.192.40") to be queried.</param>
        /// <param name="options">The options for the whois query.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>The strong typed result of query which responded from WHOIS server.</returns>
        public static async Task<WhoisResponse> QueryAsync(string query, WhoisQueryOptions options, CancellationToken token = default(CancellationToken))
        {
            options = options ?? new WhoisQueryOptions();
            var servers = new List<EndPoint> { new EndPoint(options.Server, options.Port) };
            return await QueryRecursiveAsync(query, servers, options, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Queries recursively to determine the proper endpoint for an IP or domain.
        /// </summary>
        /// <param name="query">The query for the whois server.</param>
        /// <param name="servers">The list of servers previously queried.</param>
        /// <param name="options">The options for the whois query.</param>
        /// <returns>A whois response structure containing the results of the whois queries.</returns>
        private static WhoisResponse QueryRecursive(string query, List<EndPoint> servers, IQueryOptions options)
        {
            var server = servers.Last();

            var rawResponse = string.Empty;
            var iteration = 0;

            // Continue to connect within the retries number
            while (string.IsNullOrWhiteSpace(rawResponse) && iteration <= options.Retries)
            {
                try
                {
                    iteration++;
                    rawResponse = RawQuery(GetQueryStatement(server, query), server, options);
                }
                catch (Exception) when (iteration <= options.Retries)
                {
                    rawResponse = null;
                }
            }

            if (HasReferral(rawResponse, server, out var refServer))
            {
                servers.Add(refServer);
                return QueryRecursive(query, servers, options);
            }
            else
                return new WhoisResponse(servers.Select(s => s.Host).ToArray(), rawResponse);
        }

        /// <summary>
        /// Queries recursively to determine the proper endpoint for an IP or domain.
        /// </summary>
        /// <param name="query">The query for the whois server.</param>
        /// <param name="servers">The list of servers previously queried.</param>
        /// <param name="options">The options for the whois query.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A whois response structure containing the results of the whois queries.</returns>
        private static async Task<WhoisResponse> QueryRecursiveAsync(string query, List<EndPoint> servers, IQueryOptions options, CancellationToken token)
        {
            var server = servers.Last();

            var rawResponse = string.Empty;
            var iteration = 0;

            // Continue to connect within the retries number
            while (string.IsNullOrWhiteSpace(rawResponse) && iteration <= options.Retries)
            {
                try
                {
                    iteration++;
                    rawResponse = await RawQueryAsync(GetQueryStatement(server, query), server, options, token).ConfigureAwait(false);
                }
                catch (Exception) when (iteration <= options.Retries)
                {
                    rawResponse = null;
                }
            }

            if (HasReferral(rawResponse, server, out var refServer))
            {
                servers.Add(refServer);
                return await QueryRecursiveAsync(query, servers, options, token).ConfigureAwait(false);
            }
            else
                return new WhoisResponse(servers.Select(s => s.Host).ToArray(), rawResponse);
        }

        /// <summary>
        /// Check if a response contains a referral.
        /// </summary>
        /// <param name="rawResponse">
        /// The raw response.
        /// </param>
        /// <param name="currentServer"></param>
        /// <param name="refServer">The output parameter for the referral server.</param>
        private static bool HasReferral(string rawResponse, EndPoint currentServer, out EndPoint refServer)
        {
            refServer = null;
            var refServerHost = "";
            var refServerPort = 43;

            // "ReferralServer: whois://whois.apnic.net"
            // "remarks:        at whois.nic.ad.jp. To obtain an English output"
            // "Registrar WHOIS Server: whois.markmonitor.com"
            var m2 = _hasReferralRegex.Match(rawResponse);
            if (!m2.Success) return false;

            refServerHost = m2.Groups[@"refsvr"].Value;
            refServerPort = m2.Groups["port"].Success ? int.Parse(m2.Groups["port"].Value) : refServerPort;
            if (currentServer.Host.ToLower() == refServerHost.ToLower()) return false;

            refServer = new EndPoint(refServerHost, refServerPort);
            return true;
        }

        /// <summary>
        /// Returns back the correct query for specific servers.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static string GetQueryStatement(EndPoint server, string query)
        {
            switch (server.Host)
            {
                case "whois.internic.net":
                case "whois.verisign-grs.com":
                    return $"domain {query}";
                case "whois.arin.net": // This fixes the 'Query term are ambiguous' message when querying arin. 
                    return $"n + {query}";
                default:
                    // Remove the "domain" command from other servers
                    return $"{query}";
            }
        }

        /// <summary>
        /// Send simple WHOIS query to WHOIS server, and return the response from WHOIS server.
        /// (No re-query to referral servers, and No parse the result of query.)
        /// </summary>
        /// <param name="query">domain name (ex."nic.ad.jp")or IP address (ex."192.41.192.40") to be queried.</param>
        /// <param name="server">FQDN of whois server (ex."whois.arin.net").</param>
        /// <param name="port">TCP port number to connect whois server. This parameter is optional, and default value is 43.</param>
        /// <param name="encoding">Encoding method to decode the result of query. This parameter is optional (default value is null) to using ASCII encoding.</param>
        /// <param name="timeout">A timespan to limit the connection attempt, in milliseconds.  Function returns empty string if it times out.</param>
        /// <returns>The raw data decoded by encoding parameter from the WHOIS server that responded, or an empty string if a connection cannot be established.</returns>
        [Obsolete("Use the 'RawQuery(string query, WhoisQueryOptions options)' instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static string RawQuery(string query, string server, int port = 43, Encoding encoding = null, int timeout = 2000)
        {
            var options = new WhoisQueryOptions();
            options.Server = server;
            options.Port = port;
            options.Encoding = encoding ?? options.Encoding;
            options.Timeout = timeout;
            return RawQuery(query, new EndPoint(options.Server, options.Port), options);
        }

        /// <summary>
        /// Send simple WHOIS query to WHOIS server, and return the response from WHOIS server.
        /// (No re-query to referral servers, and No parse the result of query.)
        /// </summary>
        /// <param name="query">domain name (ex."nic.ad.jp")or IP address (ex."192.41.192.40") to be queried.</param>
        /// <param name="options">The options for the whois query.</param>
        /// <returns>The raw data decoded by encoding parameter from the WHOIS server that responded, or an empty string if a connection cannot be established.</returns>
        public static string RawQuery(string query, WhoisQueryOptions options)
        {
            return RawQuery(query, new EndPoint(options.Server, options.Port), options ?? new WhoisQueryOptions());
        }

        /// <summary>
        /// Send simple WHOIS query to WHOIS server, and return the response from WHOIS server.
        /// (No re-query to referral servers, and No parse the result of query.)
        /// </summary>
        /// <param name="query">domain name (ex."nic.ad.jp")or IP address (ex."192.41.192.40") to be queried.</param>
        /// <param name="server">FQDN of whois server (ex."whois.arin.net") and its TCP port.</param>
        /// <param name="options">The options for the whois query.</param>
        /// <returns>The raw data decoded by encoding parameter from the WHOIS server that responded, or an empty string if a connection cannot be established.</returns>
        private static string RawQuery(string query, EndPoint server, IQueryOptions options)
        {
            TcpClient tcpClient;

            try
            {
                // Async connect
                var tcpClientTask = ConnectAsync(server, options);
                tcpClientTask.ConfigureAwait(false);

                // Wait at most timeout
                var success = tcpClientTask.Wait(TimeSpan.FromSeconds(options.Timeout));

                if (success)
                    tcpClient = tcpClientTask.GetAwaiter().GetResult();
                else
                {
                    Thread.Sleep(200);
                    return string.Empty;
                }
            }
            catch
            {
                Thread.Sleep(200);

                if (options.RethrowExceptions)
                    throw;

                return string.Empty;
            }

            var responseBytes = new List<byte>(4096);
            try
            {
                using (var s = tcpClient.GetStream())
                {
                    // Specify the timeouts in milliseconds
                    s.WriteTimeout = options.Timeout;
                    s.ReadTimeout = options.Timeout;

                    var queryBytes = Encoding.ASCII.GetBytes(query + "\r\n");
                    s.Write(queryBytes, 0, queryBytes.Length);
                    s.Flush();

                    const int buffSize = 8192;
                    var readBuff = new byte[buffSize];
                    var cbRead = default(int);
                    do
                    {
                        cbRead = s.Read(readBuff, 0, readBuff.Length);
                        responseBytes.AddRange(readBuff.Take(cbRead));
                        if (cbRead > 0)
                            Thread.Sleep(100);
                    } while (cbRead > 0);

                    return options.Encoding.GetString(responseBytes.ToArray());
                }
            }
            catch
            {
                tcpClient.Close();
                Thread.Sleep(200);

                if (options.RethrowExceptions)
                    throw;

                return options.Encoding.GetString(responseBytes.ToArray());
            }
            finally
            {
                tcpClient.Close();
            }
        }

        /// <summary>
        /// Send simple WHOIS query to WHOIS server, and return the response from WHOIS server.
        /// (No requery to referral servers, and No parse the result of query.)
        /// </summary>
        /// <param name="query">domain name (ex."nic.ad.jp")or IP address (ex."192.41.192.40") to be queried.</param>
        /// <param name="server">FQDN of whois server (ex."whois.arin.net").</param>
        /// <param name="port">TCP port number to connect whois server. This parameter is optional, and default value is 43.</param>
        /// <param name="encoding">Encoding method to decode the result of query. This parameter is optional (default value is null) to using ASCII encoding.</param>
        /// <param name="timeout">A timespan to limit the connection attempt, in milliseconds.  Function returns empty string if it times out.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>The raw data decoded by encoding parameter from the WHOIS server that responded, or an empty string if a connection cannot be established.</returns>
        [Obsolete("Use the 'RawQueryAsync(string query, WhoisQueryOptions options, CancellationToken token)' instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static async Task<string> RawQueryAsync(string query, string server, int port = 43,
            Encoding encoding = null, int timeout = 2000, CancellationToken token = default(CancellationToken))
        {
            var options = new WhoisQueryOptions();
            options.Server = server;
            options.Port = port;
            options.Encoding = encoding ?? options.Encoding;
            options.Timeout = timeout;
            return await RawQueryAsync(query, new EndPoint(options.Server, options.Port), options, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Send simple WHOIS query to WHOIS server, and return the response from WHOIS server.
        /// (No re-query to referral servers, and No parse the result of query.)
        /// </summary>
        /// <param name="query">domain name (ex."nic.ad.jp")or IP address (ex."192.41.192.40") to be queried.</param>
        /// <param name="options">The options for the whois query.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>The raw data decoded by encoding parameter from the WHOIS server that responded, or an empty string if a connection cannot be established.</returns>
        public static async Task<string> RawQueryAsync(string query, WhoisQueryOptions options, CancellationToken token = default(CancellationToken))
        {
            return await RawQueryAsync(query, new EndPoint(options.Server, options.Port), options ?? new WhoisQueryOptions(), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Send simple WHOIS query to WHOIS server, and return the response from WHOIS server.
        /// (No re-query to referral servers, and No parse the result of query.)
        /// </summary>
        /// <param name="query">domain name (ex."nic.ad.jp")or IP address (ex."192.41.192.40") to be queried.</param>
        /// <param name="server">FQDN of whois server (ex."whois.arin.net") and its TCP port.</param>
        /// <param name="options">The options for the whois query.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>The raw data decoded by encoding parameter from the WHOIS server that responded, or an empty string if a connection cannot be established.</returns>
        private static async Task<string> RawQueryAsync(string query, EndPoint server, IQueryOptions options, CancellationToken token)
        {
            TcpClient tcpClient;

            // Async connect
            try
            {
                tcpClient = await ConnectAsync(server, options, token).ConfigureAwait(false);
            }
            catch (SocketException)
            {
                await Task.Delay(200).ConfigureAwait(false);

                if (options.RethrowExceptions)
                    throw;

                return string.Empty;
            }

            var responseBytes = new List<byte>(4096);
            try
            {
                using (var s = tcpClient.GetStream())
                {
                    await InvokeAsync(async (cancellationToken) =>
                    {
                        var queryBytes = Encoding.ASCII.GetBytes(query + "\r\n");
                        await s.WriteAsync(queryBytes, 0, queryBytes.Length, cancellationToken).ConfigureAwait(false);
                        await s.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }, options.Timeout, token);

                    const int buffSize = 8192;
                    var readBuff = new byte[buffSize];
                    var cbRead = default(int);
                    do
                    {
                        await InvokeAsync(async (cancellationToken) =>
                        {
                            cbRead = await s.ReadAsync(readBuff, 0, buffSize, cancellationToken).ConfigureAwait(false);
                            responseBytes.AddRange(readBuff.Take(cbRead));
                            if (cbRead > 0)
                                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                        }, options.Timeout, token);
                    } while (cbRead > 0);

                    return options.Encoding.GetString(responseBytes.ToArray());
                }
            }
            catch (Exception)
            {
                tcpClient.Close();
                await Task.Delay(200).ConfigureAwait(false);

                if (options.RethrowExceptions)
                    throw;

                return options.Encoding.GetString(responseBytes.ToArray());
            }
            finally
            {
                tcpClient.Close();
            }
        }

        /// <summary>
        /// Invoke an asynchronous action with a timeout and cancellation token.
        /// </summary>
        /// <param name="action">An asynchronous action to invoke.</param>
        /// <param name="timeout">A timeout in milliseconds.</param>
        /// <param name="token">A cancellation token.</param>
        /// <remarks>
        /// On the .NET Framework 4.6.2, a cancellation token won't work in the NetworkStream.SendAsync and ReadAsync method.
        /// To avoid this issue, we use the combination of Task.Delay and Task.WhenAny to simulate a cancellation token.
        /// </remarks>
        private static async Task InvokeAsync(Func<CancellationToken, Task> action, int timeout, CancellationToken token)
        {
            using (var timeoutCancellation = new CancellationTokenSource(millisecondsDelay: timeout))
            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCancellation.Token))
            {
#if NETCOREAPP
                try
                {
                    await action(linked.Token);
                }
                catch (OperationCanceledException ex) when (!token.IsCancellationRequested)
                {
                    throw new TimeoutException("Socket operation timeout", ex);
                }
#else
                var mainTask = action(linked.Token);
                var timeoutTask = Task.Delay(Timeout.Infinite, timeoutCancellation.Token);
                var cancellationTask = Task.Delay(Timeout.Infinite, token);

                var firstCompletedTask = await Task.WhenAny(mainTask, timeoutTask, cancellationTask);

                if (firstCompletedTask == timeoutTask) throw new TimeoutException("Socket operation timeout");
                if (firstCompletedTask == cancellationTask) token.ThrowIfCancellationRequested();
#endif
            }
        }

        private static readonly Func<TcpConnectionArgs, Task<TcpClient>> DefaultConnectAsync = async args =>
        {
            if (string.IsNullOrWhiteSpace(args.Host)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(args.Host));
            if (args.Port <= 0 || args.Port > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(args.Port));

            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(args.Host, args.Port).ConfigureAwait(false);
            return tcpClient;
        };

        private static async Task<TcpClient> ConnectAsync(EndPoint server, IQueryOptions options, CancellationToken cancellationToken = default)
        {
            var tcpConnectionArgs = new TcpConnectionArgs
            {
                Host = server.Host,
                Port = server.Port,
                CancellationToken = CancellationToken.None
            };
            var connectAsync = options.ConnectAsync != null ? options.ConnectAsync : DefaultConnectAsync;
            return await connectAsync.Invoke(tcpConnectionArgs).ConfigureAwait(false);
        }
    }
}
