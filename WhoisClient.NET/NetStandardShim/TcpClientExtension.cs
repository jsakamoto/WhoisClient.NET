using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

#if !NET45
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
#endif