using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Network.TCP;

namespace Core.Network.Session
{
    public abstract class PacketSession : Session
    {
        private readonly IPacketParser _parser;

        protected PacketSession(IPacketParser parser)
        {
            _parser = parser;
        }

        protected sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;

            while(true)
            {
                if (buffer.Count < 2)
                    break;

                int packetSize = _parser.GetPacketSize(buffer);

                if (packetSize < 0)
                    return -1;

                if (packetSize == 0)
                    break;

                if (buffer.Count < packetSize)
                    break;

                OnRecvPacket(new ArraySegment<byte>(buffer.Array!, buffer.Offset , packetSize));

                processLen += packetSize;

                buffer = new ArraySegment<byte>(buffer.Array!, buffer.Offset+packetSize, buffer.Count - packetSize);
            }

            return processLen;
        }

        protected abstract void OnRecvPacket(ArraySegment<byte> packet);
    }
}
