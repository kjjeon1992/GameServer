using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Network.Protocol;
using Core.Network.Transport;

namespace GameServer
{
    public class GameServer
    {
        private Listener _listener;
        private int _port = 7777;
        private int _backlog = 100;
        
        public GameServer()
        {
            Init();
        }

        private void Init()
        {
            Console.WriteLine("Init Server Resources..");
            _listener = new Listener();
        }

        public async Task StartAsyncServer()
        {
            await _listener.StartAsync(_port, _backlog, ()=> new GameServerSession(new DefaultPacketParser()));
        }

        public async Task StopAsyncServer()
        {
            await _listener.StopAsync();

        }
    }
}
