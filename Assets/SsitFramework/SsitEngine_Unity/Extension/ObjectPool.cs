using System;
using System.Collections.Generic;

namespace SsitEngine.Unity.Pool
{
    public class ObjectPool<T>
    {
        private readonly Action<T> clearer;
        private readonly Func<T> creator;
        private readonly Queue<T> queue;

        public ObjectPool( int initialCount, Func<T> creator, Action<T> clearer = null )
        {
            if (initialCount < 0)
                throw new ArgumentOutOfRangeException("initialCount");

            if (creator == null)
                throw new ArgumentNullException("creator");

            queue = new Queue<T>(initialCount);

            this.creator = creator;
            this.clearer = clearer;
        }

        public int AvailableCount => queue.Count;

        /// <summary>
        ///     从对象池获得一个对象实例。
        /// </summary>
        public T Acquire()
        {
            if (queue.Count != 0)
                return queue.Dequeue();

            return creator();
        }

        /// <summary>
        ///     把指定的对象实例归还对象池。
        /// </summary>
        public void Release( T obj )
        {
            if (clearer != null)
                clearer(obj);

            queue.Enqueue(obj);
        }

        public void Clear()
        {
            if (typeof(T) == typeof(IDisposable))
                while (queue.Count != 0)
                {
                    var obj = queue.Dequeue();

                    ((IDisposable) obj).Dispose();
                }
            else
                queue.Clear();

            queue.TrimExcess();
        }
    }
}