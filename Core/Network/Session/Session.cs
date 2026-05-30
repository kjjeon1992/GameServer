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
        protected RingBuffer? _recvBuffer;
        protected SendBuffer? _sendBuffer;
        protected SocketAsyncEventArgs? _recvArgs;

        private CancellationTokenSource? _cts;
        protected Task? _recvTask;

        public virtual void Start(Socket socket)
        {
            _socket = socket;
            _recvBuffer = new RingBuffer(1024 * 4);
            _sendBuffer = new SendBuffer();
            _recvArgs = new SocketAsyncEventArgs();
            _cts = new CancellationTokenSource();

            OnConnected(_socket.RemoteEndPoint);
            _recvTask = ReceiveLoopAsync(_cts.Token);
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            if (_socket == null)
                return;
            if (_recvBuffer == null)
                return;
            if (_recvArgs == null)
                return;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var seg = _recvBuffer.WriteSegment;
                    if (seg.Count == 0)
                        break;

                    _recvArgs.SetBuffer(seg.Array,seg.Offset,seg.Count);

                    int bytesRead = await _socket.ReceiveAsync(_recvArgs.Buffer,token);

                    if (bytesRead == 0)
                    {
                        Disconnect();
                        break;
                    }
                    _recvBuffer.OnWrite(bytesRead);

                    int processed = OnRecv(_recvBuffer.ReadSegment);
                    if(processed < 0)
                    {
                        Disconnect();
                        return;
                    }
                    _recvBuffer.OnRead(processed);
                    _recvBuffer.Clean();
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

        public void SendAsync(ArraySegment<byte> segment)
        {
            _sendBuffer?.Enqueue(segment);
            FlushSend();
        }

        private void FlushSend()
        {
            var pendingList = _sendBuffer?.GetPendingList();
            if (pendingList == null)
                return;

            _socket?.SendAsync(pendingList, SocketFlags.None).ContinueWith(OnSendCompleted);
        }
        

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

        private void OnSendCompleted(Task<int> task)
        {
            if(_sendBuffer == null)
                return;

            if (task.IsFaulted)
            {
                Console.WriteLine($"Send Error : {task.Exception}");
                Disconnect();
                return;
            }

            if (_sendBuffer.OnSendCompleted())
                FlushSend();
        }

        protected abstract void OnConnected(EndPoint? endPoint);
        protected abstract int OnRecv(ArraySegment<byte> buffer);
        protected abstract void OnDisconnected(EndPoint? endPoint);


    }
}
