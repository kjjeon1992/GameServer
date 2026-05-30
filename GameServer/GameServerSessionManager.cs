using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Network.Session;

namespace GameServer
{
    public class GameServerSessionManager : SessionManager<GameServerSession>
    {
        public static GameServerSessionManager Instance { get; } = new();
    }
}
