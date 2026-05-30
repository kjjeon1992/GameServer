using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Core.Network.Buffer;

namespace Core.Network.Session
{
    public abstract class Session
    {
        protected Socket? _socket;
        protected RingBuffer? _recvbuffer;
        protected SocketAsyncEventArgs? _recvArgs;

        private CancellationTokenSource? _cts;
        protected Task? _receivetask;

        public virtual void Start(Socket socket)
        {
            _socket = socket;
            _recvbuffer = new RingBuffer(1024 * 4);
            _recvArgs = new SocketAsyncEventArgs();
            _cts = new CancellationTokenSource();

            OnConnected(_socket.RemoteEndPoint);
            _receivetask = ReceiveLoopAsync(_cts.Token);
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            if (_socket == null)
                return;
            if (_recvbuffer == null)
                return;
            if (_recvArgs == null)
                return;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var seg = _recvbuffer.WriteSegment;
                    if (seg.Count == 0)
                        break;

                    _recvArgs.SetBuffer(seg.Array,seg.Offset,seg.Count);

                    int bytesRead = await _socket.ReceiveAsync(_recvArgs.Buffer,token);

                    if (bytesRead == 0)
                    {
                        Disconnect();
                        break;
                    }
                    _recvbuffer.OnWrite(bytesRead);

                    int processed = OnRecv(_recvbuffer.ReadSegment);
                    if(processed < 0)
                    {
                        Disconnect();
                        return;
                    }
                    _recvbuffer.OnRead(processed);
                    _recvbuffer.Clean();
                }
            }
            catch (OperationCanceledException)
            { 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Close();
            }
        }

        protected async Task SendAsync()
        { }

        protected virtual void Disconnect()
        {
            Close();
        }
        private void Close()
        {
            try
            {
                var endPoint = _socket?.RemoteEndPoint;

                Console.WriteLine($"Session Closed : {endPoint}");
                _cts?.Cancel();
                _socket?.Close();
            }
            catch { }
        }
        protected abstract void OnConnected(EndPoint? endPoint);
        protected abstract int OnRecv(ArraySegment<byte> buffer);
        protected abstract void OnDisconnected(EndPoint? endPoint);


    }
}
