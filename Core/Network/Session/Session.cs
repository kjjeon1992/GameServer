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
    /// <summary>
    /// 소켓에서 데이터를 받아 RingBuffer에 쌓고 파생 클래스에 넘겨주는 수신 담당.
    /// </summary>
    public abstract class Session
    {
        protected Socket? _socket;                  // 실제 통신에 사용되는 소켓
        protected RingBuffer? _recvBuffer;          // 수신된 데이터를 임시로 저장하는 버퍼
        protected SendBuffer? _sendBuffer;          
        protected SocketAsyncEventArgs? _recvArgs;  // 소켓 작업에 쓸 인자 객체
                                                    
        private CancellationTokenSource? _cts;      // 루프 종료 신호
        protected Task? _recvTask;                  // 수신 루프를 실행하는 Task

        public virtual void Start(Socket socket)
        {
            _recvBuffer = new RingBuffer(1024 * 4);         // 4KB 버퍼로 초기화
            _sendBuffer = new SendBuffer();
            _recvArgs = new SocketAsyncEventArgs();
            _cts = new CancellationTokenSource();

            OnConnected(_socket.RemoteEndPoint);            // 연결된 클라이언트의 EndPoint 정보를 전달
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
                    // 쓸 수 있는 공간 확인
                    var seg = _recvBuffer.WriteSegment;

                    if (seg.Count == 0)
                        break;

                    // 해당 구간을 소켓 수신 버퍼로 지정
                    _recvArgs.SetBuffer(seg.Array,seg.Offset,seg.Count);

                    // 데이터 올 때까지 대기
                    int bytesRead = await _socket.ReceiveAsync(_recvArgs.Buffer,token);

                    if (bytesRead == 0) // 정상적으로 연결이 종료
                    {
                        Disconnect();
                        break;
                    }
                     // write pointer 이동
                    _recvBuffer.OnWrite(bytesRead);

                    // 파생 클래스에 넘기고 처리한 만큼 read pointer 이동
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
            catch (OperationCanceledException) {}
            catch (Exception ex) {Console.WriteLine(ex.ToString());}
            finally {Close();}
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

            if (_sendBuffer.OnSendCompleted())          // Queue에 남은 데이터가 있으면 Flush 재호출
                FlushSend();
        }

        protected abstract void OnConnected(EndPoint? endPoint);
        protected abstract int OnRecv(ArraySegment<byte> buffer);
        protected abstract void OnDisconnected(EndPoint? endPoint);


    }
}
