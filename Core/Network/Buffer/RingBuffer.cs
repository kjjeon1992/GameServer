using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core.Network.Buffer
{
    public class RingBuffer
    {
        private byte[] _buffer;
        private int _readPos;
        private int _writePos;
        private int _capacity;

        public RingBuffer(int capacity)
        {
            _buffer = new byte[capacity];
            _capacity = capacity;
        }

        public int DataSize 
        {
            get
            {
                if (_writePos >= _readPos)
                    return _writePos - _readPos;

                return _capacity - _readPos + _writePos;
            }
        }

        public int FreeSize 
        { 
            get 
            {
                return _capacity - DataSize - 1;
            } 
        }

        public ArraySegment<byte> WriteSegment
        {
            get
            {
                if(_writePos >= _readPos)
                {
                    int rightSpace = _capacity - _writePos;

                    if (_readPos == 0)
                        rightSpace -= 1;

                    return new ArraySegment<byte>(_buffer,_writePos, rightSpace);
                }
                else
                {
                    return new ArraySegment<byte>(_buffer, _writePos, _readPos - _writePos - 1);
                }
            }
        }

        public ArraySegment<byte> ReadSegment
        {
            get 
            {
                if (_writePos >= _readPos)
                {
                    return new ArraySegment<byte>(_buffer, _readPos, _writePos - _readPos);
                }
                else
                {
                    return new ArraySegment<byte>(_buffer, _readPos, _capacity - _readPos);
                }

            }
        }

        public void OnWrite(int size)
        {
            _writePos = (_writePos + size) % _capacity;
        }

        public void OnRead(int size)
        {
            _readPos = (_readPos + size) % _capacity;
        }

        public void Clean()
        {
            if(DataSize == 0)
            {
                _readPos = _writePos = 0;
            }
        }
    }
}
