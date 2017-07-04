using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Whois.NET
{
    internal static class TcpClientExtension
    {
        public static void Close(this TcpClient tcpClient)
        {
            tcpClient.Dispose();
        }
    }
}
