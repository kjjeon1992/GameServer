using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Network.Buffer
{
    /// <summary>
    /// SendBuffer는 전송할 데이터를 Lock 기반 Queue 형태로 관리.
    /// </summary>
    public class SendBuffer
    {
        private readonly Queue<ArraySegment<byte>> _sendQueue = new();      // 전송 대기 중인 데이터들을 저장하는 큐
        private readonly List<ArraySegment<byte>> _pendingList = new();     // 현재 전송 중인 데이터들을 저장하는 리스트
        private readonly object _lock = new();
        private bool _isSending = false;                                    // 현재 전송 중인지 여부 Flush가 동시에 돌지 않도록 디펜딩

        public void Enqueue(ArraySegment<byte> segment)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(segment);
            }
        }

        public List<ArraySegment<byte>>? GetPendingList()
        {
            lock (_lock)
            {
                // 이미 전송 중이면 null 반환하여 Flush가 동시에 돌지 않도록 함
                if (_isSending)
                    return null;

                if (_sendQueue.Count == 0)
                    return null;

                while (_sendQueue.Count > 0)
                {
                    _pendingList.Add(_sendQueue.Dequeue());
                }
                _isSending = true;
                return _pendingList;
            }
        }

        public bool OnSendCompleted()
        {
            lock (_lock)
            {
                _pendingList.Clear();

                if (_sendQueue.Count > 0)
                    return true;

                _isSending = false;
                return false;
            }
        }
    }
}
