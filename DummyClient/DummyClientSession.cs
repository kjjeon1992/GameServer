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

        private void SendTestPacket()
        {
            string msg = "Hello Server!";
            byte[] data = Encoding.UTF8.GetBytes(msg);

            byte[] packet = new byte[2 + data.Length];
            ushort size = (ushort)packet.Length;
            BitConverter.TryWriteBytes(packet, size);
            data.CopyTo(packet, 2);

            SendAsync(new ArraySegment<byte>(packet));
        }

        protected override void OnConnected(EndPoint? endPoint)
        {
            Console.WriteLine($"Connected to Server : {endPoint}");

            SendTestPacket();
        }

        protected override void OnDisconnected(EndPoint? endPoint)
        {

            Console.WriteLine($"Disconnected : {endPoint}");
        }

        protected override void OnRecvPacket(ArraySegment<byte> packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Array!, packet.Offset + 2, packet.Count -2);
            Console.WriteLine($"Received : {msg}");
        }
    }
}
