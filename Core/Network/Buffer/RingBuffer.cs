using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core.Network.Buffer
{
    /// <summary>
    /// 배열 하나를 원형으로 재사용하는 버퍼. write/read pointer를 통해 메모리 낭비를 줄이며 데이터를 관리.
    /// </summary>
    public class RingBuffer
    {

        private byte[] _buffer;     // 실제 데이터를 저장하는 배열
        private int _readPos;       // 다음에 읽을 위치를 가르키는 포인터
        private int _writePos;      // 다음에 쓸 위치를 가르키는 포인터
        private int _capacity;      // 버퍼의 총 크기, [ Sentinel Slot을 위해 실제 사용 가능한 크기는 _capacity - 1 ]


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
                    return _writePos - _readPos;            // 일반적인 경우 

                return _capacity - _readPos + _writePos;    // write pointer가 read pointer를 넘어간 경우 (원형으로 돌아온 경우)
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
                        rightSpace -= 1; // Sentinel Slot을 위해 마지막 공간 하나는 항상 비워둬야 함

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
            _writePos = (_writePos + size) % _capacity; // 끝 도달시 처음으로 돌아가도록 원형으로 계산
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
