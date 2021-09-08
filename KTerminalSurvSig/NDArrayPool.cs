using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace KTerminalNetworkBDD
{
    class NDArrayPool
    {
        private readonly Stack<NDArray> _pool;

        public int MaxSize { get; }

        public NDArrayPool(int maxSize)
        {
            MaxSize = maxSize;
            _pool = new Stack<NDArray>(maxSize);
        }

        public NDArrayPool()
        {
            MaxSize = int.MaxValue;
            _pool = new Stack<NDArray>();
        }

        public NDArray Pop(NDArray valuesSource)
        {
            NDArray a;
            if (_pool.Count > 0)
            {
                a = _pool.Pop();
                a.CopyValues(valuesSource);
            }
            else
            {
                a = new NDArray(valuesSource);
            }

            return a;
        }

        public void Push(NDArray item)
        {
            // Add item to pool if not full.
            if (_pool.Count < MaxSize)
            {
                _pool.Push(item);
            }
        }
    }
}
