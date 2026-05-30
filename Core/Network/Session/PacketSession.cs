using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Network.TCP;

namespace Core.Network.Session
{
    /// <summary>
    /// Session을 상속 받아 패킷 단위로 데이터를 처리하는 클래스.
    /// </summary>
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
                // 헤더 최소 크기 확인
                if (buffer.Count < 2)
                    break;

                // parser에게 패킷 크기 확인 요청
                int packetSize = _parser.GetPacketSize(buffer);

                if (packetSize < 0) // 데이터 부족
                    return -1;

                if (packetSize == 0) // 잘못된 데이터
                    break;

                // 패킷 전체 크기 확인
                if (buffer.Count < packetSize)
                    break;

                // 완성된 패킷 하나 전달
                OnRecvPacket(new ArraySegment<byte>(buffer.Array!, buffer.Offset , packetSize));

                // 처리한 만큼 누적하고 버퍼 슬라이드
                processLen += packetSize;

                buffer = new ArraySegment<byte>(buffer.Array!, buffer.Offset+packetSize, buffer.Count - packetSize);
            }

            return processLen;
        }

        protected abstract void OnRecvPacket(ArraySegment<byte> packet);
    }
}
