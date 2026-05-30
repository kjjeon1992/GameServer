using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Core.Network.Session;
using System.Net;

namespace Core.Network.Transport
{
    /// <summary>
    /// 서버에 연결 요청을 보내고 연결이 성공하면 세션을 시작하는 역할.
    /// </summary>
    public class Connector
    {
        private Func<Session.Session>? _sessionFactory;
        Socket? _socket;
        SocketAsyncEventArgs? _args;

        public void Connect(IPEndPoint endPoint, Func<Session.Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;

            _socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _args = new SocketAsyncEventArgs();
            _args.RemoteEndPoint = endPoint;
            _args.Completed += OnConnectCompleted;

            bool pending = _socket.ConnectAsync(_args);
            if (!pending)
                OnConnectCompleted(_socket, _args);
        }

        private void OnConnectCompleted(object? sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError != SocketError.Success)
            {
                Console.WriteLine($"Connect Failed : {args.SocketError}");
                return;
            }

            Session.Session session = _sessionFactory!.Invoke();
            session.Start(args.ConnectSocket!);
        }
    }
}
