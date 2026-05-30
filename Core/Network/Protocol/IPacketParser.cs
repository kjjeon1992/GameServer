using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Network.TCP
{
    public interface IPacketParser
    {
        int GetPacketSize(ArraySegment<byte> buffer);
    }
}
