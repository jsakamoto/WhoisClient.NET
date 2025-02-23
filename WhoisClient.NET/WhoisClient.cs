﻿using System;
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
        /// <param name="timeout">A timespan to limit the connection attempt, in seconds.</param>
        /// <param name="retries">The number of times a connection will be attempted.</param>
        /// <returns>The strong typed result of query which responded from WHOIS server.</returns>
        [Obsolete("Use the 'Query(string query, WhoisQueryOptions options)' instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static WhoisResponse Query(string query, string server = null, int port = 43,
            Encoding encoding = null, int timeout = 600, int retries = 10)
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
        /// <param name="timeout">A timespan to limit the connection attempt, in seconds.</param>
        /// <param name="retries">The number of times a connection will be attempted.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>The strong typed result of query which responded from WHOIS server.</returns>
        [Obsolete("Use the 'QueryAsync(string query, WhoisQueryOptions options, CancellationToken token)' instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static async Task<WhoisResponse> QueryAsync(string query, string server = null, int port = 43,
            Encoding encoding = null, int timeout = 600, int retries = 10, CancellationToken token = default(CancellationToken))
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
            while (string.IsNullOrWhiteSpace(rawResponse) && iteration < options.Retries)
            {
                try
                {
                    iteration++;
                    rawResponse = RawQuery(GetQueryStatement(server, query), server, options);
                }
                catch (Exception) when (iteration < options.Retries)
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
            while (string.IsNullOrWhiteSpace(rawResponse) && iteration < options.Retries)
            {
                try
                {
                    iteration++;
                    rawResponse = await RawQueryAsync(GetQueryStatement(server, query), server, options, token).ConfigureAwait(false);
                }
                catch (Exception) when (iteration < options.Retries)
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
        /// <param name="timeout">A timespan to limit the connection attempt, in seconds.  Function returns empty string if it times out.</param>
        /// <returns>The raw data decoded by encoding parameter from the WHOIS server that responded, or an empty string if a connection cannot be established.</returns>
        [Obsolete("Use the 'RawQuery(string query, WhoisQueryOptions options)' instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static string RawQuery(string query, string server, int port = 43, Encoding encoding = null, int timeout = 600)
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
            return RawQuery(query, new EndPoint(options.Server, options.Port), options);
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
            var tcpClient = new TcpClient();

            try
            {
                // Async connect
                var t = tcpClient.ConnectAsync(server.Host, server.Port);
                t.ConfigureAwait(false);

                // Wait at most timeout
                var success = t.Wait(TimeSpan.FromSeconds(options.Timeout));

                if (!success)
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

            var res = new StringBuilder();
            try
            {
                using (var s = tcpClient.GetStream())
                {
                    // Specify the timeouts in milliseconds
                    s.WriteTimeout = options.Timeout * 1000;
                    s.ReadTimeout = options.Timeout * 1000;

                    var queryBytes = Encoding.ASCII.GetBytes(query + "\r\n");
                    s.Write(queryBytes, 0, queryBytes.Length);
                    s.Flush();

                    const int buffSize = 8192;
                    var readBuff = new byte[buffSize];
                    var cbRead = default(int);
                    do
                    {
                        cbRead = s.Read(readBuff, 0, readBuff.Length);
                        res.Append(options.Encoding.GetString(readBuff, 0, cbRead));
                        if (cbRead > 0 || res.Length == 0) Thread.Sleep(100);
                    } while (cbRead > 0 || res.Length == 0);

                    return res.ToString();
                }
            }
            catch
            {
                tcpClient.Close();
                Thread.Sleep(200);

                if (options.RethrowExceptions)
                    throw;

                return res.ToString();
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
        /// <param name="timeout">A timespan to limit the connection attempt, in seconds.  Function returns empty string if it times out.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>The raw data decoded by encoding parameter from the WHOIS server that responded, or an empty string if a connection cannot be established.</returns>
        [Obsolete("Use the 'RawQueryAsync(string query, WhoisQueryOptions options, CancellationToken token)' instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static async Task<string> RawQueryAsync(string query, string server, int port = 43,
            Encoding encoding = null, int timeout = 600, CancellationToken token = default(CancellationToken))
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
            return await RawQueryAsync(query, new EndPoint(options.Server, options.Port), options, token).ConfigureAwait(false);
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
            var tcpClient = new TcpClient();

            // Async connect
            try
            {
                await tcpClient.ConnectAsync(server.Host, server.Port).ConfigureAwait(false);
            }
            catch (SocketException)
            {
                await Task.Delay(200).ConfigureAwait(false);

                if (options.RethrowExceptions)
                    throw;

                return string.Empty;
            }

            var res = new StringBuilder();
            try
            {
                using (var s = tcpClient.GetStream())
                {
                    // Specify the timeouts in milliseconds
                    s.WriteTimeout = options.Timeout * 1000;
                    s.ReadTimeout = options.Timeout * 1000;

                    var queryBytes = Encoding.ASCII.GetBytes(query + "\r\n");
                    await s.WriteAsync(queryBytes, 0, queryBytes.Length, token).ConfigureAwait(false);
                    await s.FlushAsync(token).ConfigureAwait(false);

                    const int buffSize = 8192;
                    var readBuff = new byte[buffSize];
                    var cbRead = default(int);
                    do
                    {
                        cbRead = await s.ReadAsync(readBuff, 0, Math.Min(buffSize, tcpClient.Available), token).ConfigureAwait(false);
                        res.Append(options.Encoding.GetString(readBuff, 0, cbRead));
                        if (cbRead > 0 || res.Length == 0) await Task.Delay(100, token).ConfigureAwait(false);
                    } while (cbRead > 0 || res.Length == 0);

                    return res.ToString();
                }
            }
            catch (Exception)
            {
                tcpClient.Close();
                await Task.Delay(200).ConfigureAwait(false);

                if (options.RethrowExceptions)
                    throw;

                return res.ToString();
            }
            finally
            {
                tcpClient.Close();
            }
        }
    }
}
