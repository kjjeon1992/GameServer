using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Core.Network.Transport
{
    /// <summary>
    /// 서버 소켓을 열고 클라이언트 연결 요청을 기다리고 수락하는 역할.
    /// </summary>
    public class Listener
    {
        private Socket? _listener;
        private IPEndPoint? _endPoint;
        private CancellationTokenSource? _cts;
        private Task? _acceptTask;
        private Func<Session.Session>? _sessionFactory;

        public Task StartAsync(int port, int backlog, Func<Session.Session> sessionFactoty)
        {
            _sessionFactory = sessionFactoty;

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _endPoint = new IPEndPoint(IPAddress.Any, port);
            _cts = new CancellationTokenSource();

            _listener.Bind(_endPoint);
            _listener.Listen(backlog);

            _acceptTask = AcceptLoopAsync(_cts.Token);

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            if (_listener == null)
                return Task.CompletedTask;

            _cts?.Cancel();
            _listener.Close();

            return Task.CompletedTask;
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            if(_listener == null ) 
                return;

            try
            {
                while(!token.IsCancellationRequested)
                {
                    Socket clientSocket = await _listener.AcceptAsync(token);
                    Console.WriteLine($"Connected Client : {clientSocket.RemoteEndPoint}");

                    // session 처리
                    Session.Session session = _sessionFactory!.Invoke();
                    session.Start(clientSocket);
                }
            }
            catch(OperationCanceledException)
            {
                // 정상 종료
            }
            catch(ObjectDisposedException)
            {
                // Listener가 Stop된 경우
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Accept Error : {ex.Message}");
            }
        }
    }
}
