using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.Network.Session;
using Core.Network.TCP;

namespace DummyClient
{
    public class DummyClientSession : PacketSession
    {
        public DummyClientSession(IPacketParser parser) : base(parser) { }

        protected override void OnConnected(EndPoint? endPoint)
        {
            Console.WriteLine($"Connected to Server : {endPoint}");
        }

        protected override void OnDisconnected(EndPoint? endPoint)
        {

            Console.WriteLine($"Disconnected : {endPoint}");
        }

        protected override void OnRecvPacket(ArraySegment<byte> packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Array!, packet.Offset, packet.Count);
            Console.WriteLine($"Received{ msg}");
        }
    }
}
