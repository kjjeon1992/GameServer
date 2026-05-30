using System.Net;
using Core.Network.Protocol;
using Core.Network.Transport;

namespace DummyClient
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            int port = 7777;
            IPEndPoint endPint = new IPEndPoint(IPAddress.Loopback, port);

            Connector connector = new Connector();
            connector.Connect(endPint, () => new DummyClientSession(new DefaultPacketParser()));

            while (true)
                Thread.Sleep(1000);
        }
    }
}