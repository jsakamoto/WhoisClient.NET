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
            Assert.ThrowsAsync<IOException>(new AsyncTestDelegate(action));

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
            Assert.ThrowsAsync<TimeoutException>(new AsyncTestDelegate(action));

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
    }
}
