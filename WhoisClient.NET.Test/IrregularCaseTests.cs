using System;
using System.Net.Sockets;
using System.Text;
using NUnit.Framework;
using Whois.NET;
using WhoisClient_NET.Test.Internals;

namespace WhoisClient_NET.Test
{
    [Parallelizable(ParallelScope.Children)]
    public class IrregularCaseTests
    {
        [Test]
        public async Task Interrupt_Middle_of_MultiBytesEncodedStrings_Test()
        {
#if NETCOREAPP
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            // Given: a server that sends a multi-byte encoded string
            using var server = new MockServer(async (client, token) =>
            {
                var responseBytes = Encoding.GetEncoding("iso-2022-jp").GetBytes("こちらはテスト文字列である");
                var firstHalfLength = responseBytes.Length / 2;
                var lastHalfLength = responseBytes.Length - firstHalfLength;

                // Send the first half of the response
                client.Client.Send(responseBytes.Take(firstHalfLength).ToArray());
                await Task.Delay(100, token);

                // Send the left of the response
                client.Client.Send(responseBytes.Skip(firstHalfLength).ToArray());
                await Task.Delay(100, token);

                // Shutdown the sending connection
                client.Client.Shutdown(SocketShutdown.Send);
                await Task.Delay(int.MaxValue, token);
            });

            // When: a query is made to the server
            var response = await WhoisClient.RawQueryAsync("example.jp", options: new()
            {
                Server = "localhost",
                Port = server.Port,
                Encoding = Encoding.GetEncoding("iso-2022-jp"),
                RethrowExceptions = true
            });

            // Then: the response should be the same as the server's response
            response.Is("こちらはテスト文字列である");
        }

        [Test]
        public async Task Abort_Before_Respond_Anything_Test()
        {
            // Given: a server that does not respond
            using var server = new MockServer(async (client, token) =>
            {
                client.Client.Shutdown(SocketShutdown.Send);
                await Task.Delay(int.MaxValue, token);
            });

            // When: a query is made to the server
            using var cts = new CancellationTokenSource(millisecondsDelay: 3000);
            var response = await WhoisClient.RawQueryAsync("example.jp", options: new()
            {
                Server = "localhost",
                Port = server.Port,
                RethrowExceptions = true
            }, cts.Token);

            // Then: the response should be empty
            response.Is(string.Empty);
        }

        [Test]
        public async Task Default_Timeout_Test()
        {
            // Given: a server that never shuts down
            using var server = new MockServer((client, token) => Task.Delay(int.MaxValue, token));

            // When: a query is made to the server
            using var cts = new CancellationTokenSource(millisecondsDelay: 5000);
            var response = await Task.Run(() =>
                WhoisClient.RawQuery("example.jp", options: new()
                {
                    Server = "localhost",
                    Port = server.Port,
                }), cts.Token);

            // Then: the response should be empty
            response.Is(string.Empty);
            cts.Token.IsCancellationRequested.IsFalse();
        }

        [Test]
        public async Task Default_Timeout_Async_Test()
        {
            // Given: a server that never shuts down
            using var server = new MockServer((client, token) => Task.Delay(int.MaxValue, token));

            // When: a query is made to the server
            using var cts = new CancellationTokenSource(millisecondsDelay: 5000);
            var response = await WhoisClient.RawQueryAsync("example.jp", options: new()
            {
                Server = "localhost",
                Port = server.Port,
            }, cts.Token);

            // Then: the response should be empty
            response.Is(string.Empty);
            cts.Token.IsCancellationRequested.IsFalse();
        }

        [Test]
        public void Default_Timeout_WithExceptionRethrow_Test()
        {
            // Given: a server that never shuts down
            using var server = new MockServer((client, token) => Task.Delay(int.MaxValue, token));

            // When: a query is made to the server
            using var cts = new CancellationTokenSource(millisecondsDelay: 5000);
            var action = () => Task.Run(() =>
                WhoisClient.RawQuery("example.jp", options: new()
                {
                    Server = "localhost",
                    Port = server.Port,
                    RethrowExceptions = true
                }), cts.Token);

            // Then: IOException should be thrown
            Assert.ThrowsAsync<IOException>(action);

            cts.Token.IsCancellationRequested.IsFalse();
        }

        [Test]
        public void Default_Timeout_WithExceptionRethrow_Async_Test()
        {
            // Given: a server that never shuts down
            using var server = new MockServer((client, token) => Task.Delay(int.MaxValue, token));

            // When: a query is made to the server
            using var cts = new CancellationTokenSource(millisecondsDelay: 5000);
            var action = () => WhoisClient.RawQueryAsync("example.jp", options: new()
            {
                Server = "localhost",
                Port = server.Port,
                RethrowExceptions = true
            }, cts.Token);

            // Then: TimeoutException should be thrown
            Assert.ThrowsAsync<TimeoutException>(action);

            cts.Token.IsCancellationRequested.IsFalse();
        }

        [Test]
        public async Task Default_RetryCount_Test()
        {
            // Given: a server that disconnects immediately
            var connectionCount = 0;
            using var server = new MockServer(async (client, token) =>
            {
                connectionCount++;
                client.Client.Shutdown(SocketShutdown.Send);
                await Task.Delay(int.MaxValue, token);
            });

            // When: a query is made to the server
            using var cts = new CancellationTokenSource(millisecondsDelay: 10000);
            var response = await Task.Run(() =>
                WhoisClient.Query("example.jp", options: new()
                {
                    Server = "localhost",
                    Port = server.Port
                }), cts.Token);

            // Then: the number of attempts to connect should be 4 ( 1st + 3 retries )
            connectionCount.Is(4);
            response.Raw.Is(string.Empty);
            cts.Token.IsCancellationRequested.IsFalse();
        }

        [Test]
        public async Task Default_RetryCount_Async_Test()
        {
            // Given: a server that disconnects immediately
            var connectionCount = 0;
            using var server = new MockServer(async (client, token) =>
            {
                connectionCount++;
                client.Client.Shutdown(SocketShutdown.Send);
                await Task.Delay(int.MaxValue, token);
            });

            // When: a query is made to the server
            using var cts = new CancellationTokenSource(millisecondsDelay: 10000);
            var response = await WhoisClient.QueryAsync("example.jp", options: new()
            {
                Server = "localhost",
                Port = server.Port
            }, cts.Token);

            // Then: the number of attempts to connect should be 4 ( 1st + 3 retries )
            connectionCount.Is(4);
            response.Raw.Is(string.Empty);
            cts.Token.IsCancellationRequested.IsFalse();
        }

        [Test]
        public async Task WhoisServer_With_HttpsUrl_Should_Not_Follow_As_Referral_Test()
        {
            // Given: a server that returns a response with an HTTPS URL in the "Registrar WHOIS Server" field
            var whoisResponse =
                "Domain Name: EXAMPLE.ORG\r\n" +
                "Registry Domain ID: 123456789_DOMAIN_ORG-VRSN\r\n" +
                "Registrar WHOIS Server: https://rdap.example.org/\r\n" +
                "Registrar URL: http://www.example.org\r\n" +
                "Registrant Organization: Example Org\r\n" +
                "Registrar: Example Registrar, Inc.\r\n";
            using var server = new MockServer(async (client, token) =>
            {
                await Task.Delay(200, token);
                var responseBytes = Encoding.ASCII.GetBytes(whoisResponse);
                client.Client.Send(responseBytes);
                await Task.Delay(100, token);
                client.Client.Shutdown(SocketShutdown.Send);
                await Task.Delay(int.MaxValue, token);
            });

            // When: a query is made to the server
            using var cts = new CancellationTokenSource(millisecondsDelay: 10000);
            var response = await WhoisClient.QueryAsync("example.org", options: new()
            {
                Server = "localhost",
                Port = server.Port,
                Retries = 0,
                RethrowExceptions = true
            }, cts.Token);

            // Then: the response should contain the WHOIS data, not be empty
            // (Previously, "https" would be extracted as a referral server, causing the response to be lost)
            response.Raw.Is(whoisResponse);
            response.RespondedServers.Length.Is(1);
            response.RespondedServers[0].Is("localhost");
        }

        [Test]
        public async Task WhoisServer_With_HttpUrl_Should_Not_Follow_As_Referral_Http_Test()
        {
            // Given: a server that returns a response with an HTTP URL in the "Registrar WHOIS Server" field
            var whoisResponse =
                "Domain Name: EXAMPLE.ORG\r\n" +
                "Registrar WHOIS Server: http://whois.registrar.example.com\r\n" +
                "Registrant Organization: Example Org\r\n";
            using var server = new MockServer(async (client, token) =>
            {
                await Task.Delay(200, token);
                var responseBytes = Encoding.ASCII.GetBytes(whoisResponse);
                client.Client.Send(responseBytes);
                await Task.Delay(100, token);
                client.Client.Shutdown(SocketShutdown.Send);
                await Task.Delay(int.MaxValue, token);
            });

            // When: a query is made to the server
            using var cts = new CancellationTokenSource(millisecondsDelay: 10000);
            var response = await WhoisClient.QueryAsync("example.org", options: new()
            {
                Server = "localhost",
                Port = server.Port,
                Retries = 0,
                RethrowExceptions = true
            }, cts.Token);

            // Then: HTTP URLs should not be treated as WHOIS server referrals
            response.Raw.Is(whoisResponse);
            response.RespondedServers.Length.Is(1);
            response.RespondedServers[0].Is("localhost");
        }

        [Test]
        public async Task Empty_Refer_Should_Not_Follow_Next_Line_As_Referral_Test()
        {
            // Given: a server that returns a response with empty "refer:" and "whois:" fields, like IANA does for the .DIGITAL TLD
            var whoisResponse =
                "% IANA WHOIS server\n" +
                "% for more information on IANA, visit http://www.iana.org\n" +
                "% This query returned 1 object\n" +
                "\n" +
                "refer:        \n" +
                "\n" +
                "domain:       DIGITAL\n" +
                "\n" +
                "organisation: Binky Moon, LLC\n" +
                "\n" +
                "whois:        \n" +
                "\n" +
                "status:       ACTIVE\n";
            using var server = new MockServer(async (client, token) =>
            {
                await Task.Delay(200, token);
                client.Client.Send(Encoding.ASCII.GetBytes(whoisResponse));
                await Task.Delay(100, token);
                client.Client.Shutdown(SocketShutdown.Send);
                await Task.Delay(int.MaxValue, token);
            });

            // When: a query is made to the server
            using var cts = new CancellationTokenSource(millisecondsDelay: 10000);
            var response = await WhoisClient.QueryAsync("example.digital", options: new()
            {
                Server = "localhost",
                Port = server.Port,
                Retries = 0,
                RethrowExceptions = true
            }, cts.Token);

            // Then: the response should contain the WHOIS data, not be empty
            // (Previously, "domain" on the next line would be extracted as a referral server, causing the response to be lost)
            response.Raw.Is(whoisResponse);
            response.RespondedServers.Length.Is(1);
            response.RespondedServers[0].Is("localhost");
        }

        [Test]
        public async Task Empty_WhoisServer_Should_Not_Follow_Next_Line_As_Referral_Test()
        {
            // Given: a server that returns a response with an empty "Registrar WHOIS Server" field
            var whoisResponse =
                "Domain Name: EXAMPLE.DIGITAL\r\n" +
                "Registrar WHOIS Server: \r\n" +
                "Registrar URL: http://www.example.com\r\n" +
                "Registrant Organization: Example Org\r\n";
            using var server = new MockServer(async (client, token) =>
            {
                await Task.Delay(200, token);
                client.Client.Send(Encoding.ASCII.GetBytes(whoisResponse));
                await Task.Delay(100, token);
                client.Client.Shutdown(SocketShutdown.Send);
                await Task.Delay(int.MaxValue, token);
            });

            // When: a query is made to the server
            using var cts = new CancellationTokenSource(millisecondsDelay: 10000);
            var response = await Task.Run(() => WhoisClient.Query("example.digital", options: new()
            {
                Server = "localhost",
                Port = server.Port,
                Retries = 0,
                RethrowExceptions = true
            }), cts.Token);

            // Then: the response should contain the WHOIS data, not be empty
            response.Raw.Is(whoisResponse);
            response.RespondedServers.Length.Is(1);
            response.RespondedServers[0].Is("localhost");
        }
    }
}
