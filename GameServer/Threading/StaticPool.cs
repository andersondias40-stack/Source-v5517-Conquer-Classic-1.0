using System.Collections.Generic;
using System.Diagnostics;

namespace System
{
    using System.Security;
    using System.Threading;
    using iThread = System.Threading.Thread;

    public class StaticPool : IDisposable
    {
        /// <summary>
        /// Not to be modified unless knowing correctly what you are doing.
        /// </summary>
        public static int SleepTime = 1;

        internal object qSyncRoot, dSyncRoot;
        internal Dictionary<int, ISubscription> subscribers;
        internal Queue<ISubscription> queue;

        internal ThreadPriority basePriority = ThreadPriority.Normal;

        internal List<iThread> pool;
        protected internal iThread propagationThread;
        internal volatile bool doWork, disposed;
        internal int threads, idleThreads, inUseThreads;
        internal int minimumThreadCount, maximumThreadCount;

        public int Threads { get { return threads; } }
        public int IdleThreads { get { return idleThreads; } }
        public int InUseThreads { get { return inUseThreads; } }
        public int Treshold { get { return queue.Count; } }

        private static bool IsHandlerCreated = false;
        public StaticPool(int maximumPoolSize = 32, ThreadPriority Priority = ThreadPriority.Normal)
        {
            if (!IsHandlerCreated)
            {
                IsHandlerCreated = true;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }
            this.disposed = false;
            Debug.Assert(maximumPoolSize > -1);

            this.dSyncRoot = new object();
            this.qSyncRoot = new object();

            this.subscribers = new Dictionary<int, ISubscription>();
            this.queue = new Queue<ISubscription>();
            this.pool = new List<iThread>();

            this.minimumThreadCount = maximumPoolSize;
            this.maximumThreadCount = maximumPoolSize;

            this.basePriority = Priority;
        }
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // unhandle exection to do
        }
        ~StaticPool()
        {
            this.cleanUp(false);
        }
        void IDisposable.Dispose()
        {
            this.cleanUp(true);
        }

        [SecuritySafeCritical]
        internal void enrollWorker()
        {
            if (this.disposed) return;

            Interlocked.Increment(ref this.threads);
            Interlocked.Increment(ref this.idleThreads);
            iThread thread = new iThread(this.work);//1 MB
            this.pool.Add(thread);
            thread.Priority = basePriority;
            thread.Start();
        }

        internal void cleanUp(bool forcefully)
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.doWork = false;
                if (forcefully)
                    foreach (var thread in this.pool)
                        ThreadEx.Abort(thread);
                this.subscribers.Clear();
                this.subscribers = null;
                this.queue = null;
                this.pool = null;
            }
        }

        internal void work()
        {
            var thread = iThread.CurrentThread;
            while (thread == null)
                thread = iThread.CurrentThread;
            while (this.doWork)
            {
                iThread.Sleep(SleepTime);
                ISubscription sub;
                if (tryDequeue(out sub))
                {
                    if (sub.Viable)
                    {
                        Interlocked.Decrement(ref this.idleThreads);
                        Interlocked.Increment(ref this.inUseThreads);

                        thread.Priority = sub.GetPriority();

                        try { sub.Invoke(); }
                        catch (Exception e) { Console.WriteLine(e); }
                        finally { sub.Enqueued = false; }

                        thread.Priority = basePriority;

                        Interlocked.Decrement(ref this.inUseThreads);
                        Interlocked.Increment(ref this.idleThreads);
                    }
                    else
                        removeSubscriber(sub.GetHashCode());
                }
                sub = null;
            }
            Interlocked.Decrement(ref this.idleThreads);
        }

        internal bool tryDequeue(out ISubscription sub)
        {
            sub = null;
            lock (this.qSyncRoot)
            {
                if (this.queue.Count != 0)
                {
                    var mDeq = this.queue.Dequeue();
                    sub = mDeq;
                }
            }
            return sub != null;
        }
        internal void removeSubscriber(int hash)
        {
            lock (this.dSyncRoot)
                this.subscribers.Remove(hash);
        }

        public IDisposable Subscribe(TimerRule instruction)
        {
            ISubscription sub = null;
            lock (this.dSyncRoot)
            {
                sub = new Subscription(instruction);
                if (instruction is LazyDelegate) sub.Set(instruction.Period);
                this.subscribers[sub.GetHashCode()] = sub;
            }
            return sub;
        }

        public IDisposable Subscribe<T>(Generic.TimerRule<T> instruction, T param)
        {
            ISubscription sub = null;
            lock (this.dSyncRoot)
            {
                sub = new Generic.Subscription<T>(instruction, param);
                if (instruction is Generic.LazyDelegate<T>) sub.Set(instruction.Period);
                this.subscribers[sub.GetHashCode()] = sub;
            }
            return sub;
        }

        public StaticPool Run()
        {
            this.doWork = true;
            for (int i = 0; i < this.minimumThreadCount; i++)
                this.enrollWorker();
            this.propagationThread = new iThread(this.propagate);
            this.propagationThread.Start();
            return this;
        }

        private void propagate()
        {
            while (this.doWork)
            {
                Queue<ISubscription> qSubs = new Queue<ISubscription>();
                Queue<int> hashes = new Queue<int>();
                lock (this.dSyncRoot)
                {
                    foreach (var sub in this.subscribers.Values)
                    {
                        if (sub.Viable)
                        {
                            if (!sub.Enqueued && sub.Next)
                            {
                                sub.Enqueued = true;
                                qSubs.Enqueue(sub);
                            }
                        }
                        else
                        {
                            hashes.Enqueue(sub.GetHashCode());
                        }
                    }
                    while (hashes.Count != 0)
                        this.subscribers.Remove(hashes.Dequeue());
                }
                if (qSubs.Count != 0)
                    lock (this.qSyncRoot)
                        while (qSubs.Count != 0)
                            this.queue.Enqueue(qSubs.Dequeue());

                iThread.Sleep(SleepTime);
            }
        }

        public void Clear()
        {
            lock (this.qSyncRoot)
                this.queue.Clear();
        }

        public override string ToString()
        {
            int count, subs;
            subs = this.subscribers.Count;
            count = this.queue.Count;

            return string.Format("{0} waiting exec, {1} subscriptions, {2} threads: {3} in use, {4} idle", count, subs, this.threads, this.inUseThreads, this.idleThreads);
        }
    }
}
