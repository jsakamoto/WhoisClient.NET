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
                var stream = client.GetStream();
                await stream.WriteAsync(responseBytes, 0, firstHalfLength, token);
                await stream.FlushAsync(token);
                await Task.Delay(100, token);

                // Send the left of the response
                await stream.WriteAsync(responseBytes, firstHalfLength, lastHalfLength, token);
                await stream.FlushAsync(token);
                await Task.Delay(100, token);
            });

            // When: a query is made to the server
            var options = new WhoisQueryOptions { Server = "localhost", Port = server.Port, Encoding = Encoding.GetEncoding("iso-2022-jp") };
            var response = await WhoisClient.RawQueryAsync("example.jp", options);

            // Then: the response should be the same as the server's response
            response.Is("こちらはテスト文字列である");
        }
    }
}
