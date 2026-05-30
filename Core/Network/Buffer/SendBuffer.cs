using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Network.Buffer
{
    public class SendBuffer
    {
        private readonly Queue<ArraySegment<byte>> _sendQueue = new();
        private readonly List<ArraySegment<byte>> _pendingList = new();
        private readonly object _lock = new();
        private bool _isSending = false;

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
