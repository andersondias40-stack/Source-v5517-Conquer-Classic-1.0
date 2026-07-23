namespace System.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading.Generic;
    using System.Windows.Forms;

    public class StaticPool : IDisposable
    {
        internal volatile bool Running;
        internal volatile bool Sleeping;
        internal SafeDictionary<int, Subscription> Subscriptions;
        internal int _Threads;
        internal int _IdleThreads;
        internal int _InUseThreads;
        internal int __minimumPoolSize;
        internal int _maximumPoolSize;
        internal List<Thread> threads;
        internal object _TresholdLock;
        internal object SubscriptionsLock;
        protected internal Thread propagationThread;
        internal Queue<Subscription> _Treshold;
        public const int SleepTime = 1;

        public StaticPool(int maximumPoolSize = 0x20)
        {         
            this.Sleeping = false;
            this.SubscriptionsLock = new object();
            this._TresholdLock = new object();
            this.Subscriptions = new SafeDictionary<int, Subscription>();
            this._Treshold = new Queue<Subscription>();
            this.threads = new List<Thread>();
            this.__minimumPoolSize = maximumPoolSize;
            this._maximumPoolSize = maximumPoolSize;
        }

        public void Clear()
        {
            lock (this._TresholdLock)
            {
                this._Treshold.Clear();
            }
        }

        ~StaticPool()
        {
            this.Abort(false);
        }

        [SecuritySafeCritical]
        internal void Start()
        {
            if (!this.Sleeping)
            {
                Interlocked.Increment(ref this._Threads);
                Interlocked.Increment(ref this._IdleThreads);
                Thread item = new Thread(new ThreadStart(this.Work), 0x100000);
                this.threads.Add(item);
                item.Priority = ThreadPriority.Normal;
                item.Start();
            }
        }

        internal void Abort(bool Abort)
        {
            if (!this.Sleeping)
            {
                this.Sleeping = true;
                this.Running = false;
                if (Abort)
                {
                    foreach (Thread thread in this.threads)
                    {
                        thread.Abort();
                    }
                }
                this.Subscriptions.Clear();
                this.Subscriptions = null;
                this._Treshold = null;
                this.threads = null;
            }
        }

        internal void Work()
        {
            Thread currentThread = Thread.CurrentThread;
            while (this.Running)
            {
                Subscription Subscription;
                Thread.Sleep(1);
                if (this.Dequeue(out Subscription))
                {
                    if (Subscription._Active)
                    {
                        Interlocked.Decrement(ref this._IdleThreads);
                        Interlocked.Increment(ref this._InUseThreads);
                        currentThread.Priority = Subscription.GetThreadPriority();
                        try
                        {
                            Subscription.Invoke();
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception.ToString());
                        }
                        finally
                        {
                            Subscription.running = false;
                        }
                        currentThread.Priority = ThreadPriority.Normal;
                        Interlocked.Decrement(ref this._InUseThreads);
                        Interlocked.Increment(ref this._IdleThreads);
                    }
                    else
                    {
                        this.Remove(Subscription.GetHashCode());
                    }
                }
            }
            Interlocked.Decrement(ref this._IdleThreads);
        }

        internal bool Dequeue(out Subscription Subscription)
        {
            Subscription = null;
            lock (this._TresholdLock)
            {
                if (this._Treshold.Count != 0)
                {
                    Subscription class2 = this._Treshold.Dequeue();
                    Subscription = class2;
                }
            }
            return (Subscription != null);
        }

        internal void Remove(int key)
        {
            lock (this.SubscriptionsLock)
            {
                this.Subscriptions.Remove(key);
            }
        }

        private void Propagation()
        {
            while (this.Running)
            {
                Queue<Subscription> queue = new Queue<Subscription>();
                Queue<int> queue2 = new Queue<int>();
                lock (this.SubscriptionsLock)
                {
                    foreach (Subscription Subscription in this.Subscriptions.Values)
                    {
                        if (Subscription._Active)
                        {
                            if (!Subscription.running && Subscription.method_0())
                            {
                                Subscription.running = true;
                                queue.Enqueue(Subscription);
                            }
                        }
                        else
                        {
                            queue2.Enqueue(Subscription.GetHashCode());
                        }
                    }
                    while (queue2.Count != 0)
                    {
                        this.Subscriptions.Remove(queue2.Dequeue());
                    }
                }
                if (queue.Count != 0)
                {
                    lock (this._TresholdLock)
                    {
                        while (queue.Count != 0)
                        {
                            this._Treshold.Enqueue(queue.Dequeue());
                        }
                    }
                }
                Thread.Sleep(1);
            }
        }

        public StaticPool Run()
        {
            this.Running = true;
            for (int i = 0; i < this.__minimumPoolSize; i++)
            {
                this.Start();
            }
            this.propagationThread = new Thread(new ThreadStart(this.Propagation));
            this.propagationThread.Start();
            return this;
        }

        public IDisposable Subscribe(TimerRule instruction)
        {
            Subscription class2 = null;
            lock (this.SubscriptionsLock)
            {
                class2 = new LazySubscription(instruction);
                if (instruction is LazyDelegate)
                {
                    class2.AddMilliseconds(instruction._period);
                }
                this.Subscriptions[class2.GetHashCode()] = class2;
            }
            return class2;
        }

        public IDisposable Subscribe<T>(TimerRule<T> instruction, T param)
        {
            Subscription Subscription = null;
            lock (this.SubscriptionsLock)
            {
                Subscription = new ParamSubscription<T>(instruction, param);
                if (instruction is LazyDelegate<T>)
                {
                    Subscription.AddMilliseconds(instruction._period);
                }
                this.Subscriptions[Subscription.GetHashCode()] = Subscription;
            }
            return Subscription;
        }

        void IDisposable.Dispose()
        {
            this.Abort(true);
        }

        public override string ToString()
        {
            int count = this.Subscriptions.Count;
            int num = this._Treshold.Count;
            return string.Format("{0} waiting exec, {1} subscriptions, {2} threads: {3} in use, {4} idle", new object[] { num, count, this._Threads, this._InUseThreads, this._IdleThreads });
        }

        public int IdleThreads
        {
            get
            {
                return this._IdleThreads;
            }
        }

        public int InUseThreads
        {
            get
            {
                return this._InUseThreads;
            }
        }

        public int Threads
        {
            get
            {
                return this._Threads;
            }
        }

        public int Treshold
        {
            get
            {
                return this._Treshold.Count;
            }
        }
    }
}

