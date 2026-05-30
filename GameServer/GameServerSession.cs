using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.Network.Session;
using Core.Network.TCP;

namespace GameServer
{
    public class GameServerSession : PacketSession
    {
        public int SessionId { get; set; }

        public GameServerSession(IPacketParser parser) : base(parser) { }

        protected override void OnConnected(EndPoint? endPoint)
        {
            Console.WriteLine($"Connected : {endPoint}");
            GameServerSessionManager.Instance.Add(this, id=> SessionId = id);
        }

        protected override void OnDisconnected(EndPoint? endPoint)
        {
            Console.WriteLine($"Disconnected : {endPoint}");
            GameServerSessionManager.Instance.Remove(SessionId);
        }

        protected override void OnRecvPacket(ArraySegment<byte> packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Array!, packet.Offset+2, packet.Count-2);
            Console.WriteLine($"Received : {msg}");

            SendAsync(packet);
        }
    }
}
