using System.Net.Sockets;
using System.Text;
using NUnit.Framework;
using Whois.NET;
using WhoisClient_NET.Test.Internals;

namespace WhoisClient_NET.Test
{
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
    }
}
