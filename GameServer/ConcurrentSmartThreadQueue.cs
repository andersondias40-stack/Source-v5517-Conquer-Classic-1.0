
using System;
using System.Collections.Concurrent;

namespace COServer
{
    public abstract class ConcurrentSmartThreadQueue<T>
    {
        public ConcurrentQueue<T> Queues;
        public ThreadItem thread;

        public ConcurrentSmartThreadQueue(int Processors)
        {
            Queues = new ConcurrentQueue<T>();
        }
        protected abstract void OnDequeue(T Value, int time);

        public bool Finish()
        {
            while (Queues.Count > 0)
            {
                System.Threading.Thread.Sleep(1);
            }
            System.Threading.Thread.Sleep(500);
            return true;
        }
        public void Start(int period)
        {
            thread = new ThreadItem(period, Work);
            thread.Open();
        }

        public void Work()
        {
            try
            {
                Time32 timer = Time32.Now;
                T obj;
                while (Queues.TryDequeue(out obj))
                {
                    OnDequeue(obj, timer.AllMilliseconds);
                }
            }
            catch (Exception e) { Console.WriteException(e); }
        }

        public virtual void Enqueue(T obj)
        {
            Queues.Enqueue(obj);
        }
        public int Count { get { return Queues.Count; } }
    }
}
