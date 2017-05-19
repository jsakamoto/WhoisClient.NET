using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Whois.NET
{
    internal static class Thread
    {

        public static void Sleep(int milliseconds)
        {
            Task.Delay(milliseconds).Wait();
        }
    }
}
