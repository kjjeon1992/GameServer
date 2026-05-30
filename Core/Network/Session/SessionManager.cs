using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Network.Session
{
    /// <summary>
    /// 연결된 세션들을 관리하는 제네릭 프레임워크. 세션 생성, 등록, 제거, 브로드캐스트 기능 제공.
    /// 제네릭 타입은 오직 Session을 상속한 타입만 허용.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SessionManager<T> where T: Session
    {
        private readonly ConcurrentDictionary<int, T> _sessions = new ConcurrentDictionary<int, T>();
        private int _sessionId = 0;
        private Func<T>? _factory;

        public void SetFactory(Func<T> factory) => _factory = factory;

        public T CreateSession()
        {
            int id = Interlocked.Increment(ref _sessionId);
            T session = _factory!.Invoke();
            _sessions[id] = session;
            return session;
        }

        public void Add(T session, Action<int> onIdAssinged)
        {
            int id = Interlocked.Increment(ref _sessionId);
            _sessions[id] = session;
            onIdAssinged(id);   // 여기서 id를 람다에 넘김
        }

        public void Remove(int id) => _sessions.TryRemove(id, out _);
        public void Broadcast(Action<T> action)
        {
            foreach (var s in _sessions.Values)
                action(s);
        }
    }
}
