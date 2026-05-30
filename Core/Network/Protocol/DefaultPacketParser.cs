using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Network.TCP;

namespace Core.Network.Protocol
{
    public class DefaultPacketParser : IPacketParser
    {
        public int GetPacketSize(ArraySegment<byte> buffer)
        {
            if (buffer.Count < 2) 
                return 0;

            return BitConverter.ToUInt16(buffer.Array!, buffer.Offset);
        }
    }
}
