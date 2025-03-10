using System.Net;
using System.Net.Sockets;

namespace WhoisClient_NET.Test.Internals
{
    /// <summary>
    /// A mock server for testing purposes.
    /// </summary>
    internal class MockServer : IDisposable
    {
        private readonly TcpListener _listener;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Gets the port number the server is listening on.
        /// </summary>
        public int Port { get; }

        private readonly Func<TcpClient, CancellationToken, Task> _handleClientAsync;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockServer"/> class.
        /// </summary>
        /// <param name="handleClientAsync">A function to respond mocking data to a client.</param>
        public MockServer(Func<TcpClient, CancellationToken, Task> handleClientAsync)
        {
            this._listener = new TcpListener(IPAddress.Loopback, 0);
            this._listener.Start();
            this.Port = ((IPEndPoint)this._listener.LocalEndpoint).Port;
            Task.Run(() => this.AcceptClientsAsync(this._cts.Token));
            this._handleClientAsync = handleClientAsync;
        }

        /// <summary>
        /// Accepts incoming client connections asynchronously.
        /// </summary>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        private async Task AcceptClientsAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var client = await this._listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => this.HandleClientAsync(client, token));
                }
            }
            catch (ObjectDisposedException) when (token.IsCancellationRequested) { }
        }

        /// <summary>
        /// Handles a client connection asynchronously.
        /// </summary>
        /// <param name="client">The client to handle.</param>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using (client)
            {
                await this._handleClientAsync(client, token);
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="MockServer"/>.
        /// </summary>
        public void Dispose()
        {
            this._cts.Cancel();
            this._listener.Stop();
        }
    }
}