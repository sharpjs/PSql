using System.Net;
using System.Net.Sockets;

namespace PSql.Tests
{
    internal static class TcpPort
    {
        public static bool IsListening(ushort port)
        {
            const int TimeoutMs = 1000;

            try
            {
                using var client = new TcpClient();

                return client.ConnectAsync(IPAddress.Loopback, port).Wait(TimeoutMs)
                    && client.Connected;
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}
