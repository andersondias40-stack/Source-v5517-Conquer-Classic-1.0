namespace System.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading.Generic;
    using System.Windows.Forms;

    public class StandalonePool : IDisposable
    {
        internal volatile bool _Active;
        internal volatile bool Idle;
        internal SafeDictionary<int, Subscription> _Subscriptions;
        internal int _Threads;
        internal int _IdleThreads;
        internal int _InUseThreads;
        internal int _minimumPoolSize;
        internal int _maximumPoolSize;
        internal List<Thread> _Thread;
        internal object _TresholdLock;
        internal object _SubscriptionsLock;
        protected internal Thread propagationThread;
        internal Queue<Subscription> _Treshold;
        public const int SleepTime = 1;

        public StandalonePool(int minimumPoolSize = 6, int maximumPoolSize = 0x20)
        {          
            this.Idle = false;
            this._SubscriptionsLock = new object();
            this._TresholdLock = new object();
            this._Subscriptions = new SafeDictionary<int, Subscription>();
            this._Treshold = new Queue<Subscription>();
            this._Thread = new List<Thread>();
            this._minimumPoolSize = minimumPoolSize;
            this._maximumPoolSize = maximumPoolSize;
        }

        public void Clear()
        {
            lock (this._TresholdLock)
            {
                this._Treshold.Clear();
            }
        }

        ~StandalonePool()
        {
            this.Abort(false);
        }

        internal void Start()
        {
            if (!this.Idle)
            {
                Interlocked.Increment(ref this._Threads);
                Interlocked.Increment(ref this._IdleThreads);
                Thread item = new Thread(new ThreadStart(this.Work));
                this._Thread.Add(item);
                item.Priority = ThreadPriority.Normal;
                item.IsBackground = false;
                item.Start();
            }
        }

        internal void Reset_Thread()
        {
            if (!this.Idle)
            {
                foreach (Thread thread in this._Thread)
                {
                    if (!thread.IsBackground)
                    {
                        thread.IsBackground = true;
                        Interlocked.Decrement(ref this._Threads);
                        this._Thread.Remove(thread);
                        break;
                    }
                }
            }
        }

        internal void Abort(bool Abort)
        {
            if (!this.Idle)
            {
                this.Idle = true;
                this._Active = false;
                if (Abort)
                {
                    foreach (Thread thread in this._Thread)
                    {
                        thread.Abort();
                    }
                }
                this._Subscriptions.Clear();
                this._Subscriptions = null;
                this._Treshold = null;
                this._Thread = null;
            }
        }

        internal void Work()
        {
            Thread currentThread = Thread.CurrentThread;
            while (this._Active && !currentThread.IsBackground)
            {
                Subscription class2;
                Thread.Sleep(1);
                if (this.Dequeue(out class2))
                {
                    if (class2._Active)
                    {
                        Interlocked.Decrement(ref this._IdleThreads);
                        Interlocked.Increment(ref this._InUseThreads);
                        currentThread.Priority = class2.GetThreadPriority();
                        try
                        {
                            class2.Invoke();
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception.ToString());
                        }
                        finally
                        {
                            class2.running = false;
                        }
                        currentThread.Priority = ThreadPriority.Normal;
                        Interlocked.Decrement(ref this._InUseThreads);
                        Interlocked.Increment(ref this._IdleThreads);
                    }
                    else
                    {
                        this.Remove(class2.GetHashCode());
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
            lock (this._SubscriptionsLock)
            {
                this._Subscriptions.Remove(key);
            }
        }

        internal void method_6()
        {
            int num = this._InUseThreads;
            int num2 = this._Threads;
            if (((num == num2) || ((this._Treshold.Count / 10) >= num2)) && (num2 < this._maximumPoolSize))
            {
                this.Start();
            }
            if ((num <= (num2 / 4)) && (num2 > this._minimumPoolSize))
            {
                this.Reset_Thread();
            }
        }

        private void propagation()
        {
            while (this._Active)
            {
                Queue<Subscription> queue = new Queue<Subscription>();
                Queue<int> queue2 = new Queue<int>();
                lock (this._SubscriptionsLock)
                {
                    foreach (Subscription class2 in this._Subscriptions.Values)
                    {
                        if (class2._Active)
                        {
                            if (!class2.running && class2.method_0())
                            {
                                class2.running = true;
                                queue.Enqueue(class2);
                            }
                        }
                        else
                        {
                            queue2.Enqueue(class2.GetHashCode());
                        }
                    }
                    while (queue2.Count != 0)
                    {
                        this._Subscriptions.Remove(queue2.Dequeue());
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
                this.method_6();
                Thread.Sleep(1);
            }
        }

        public StandalonePool Run()
        {
            this._Active = true;
            for (int i = 0; i < this._minimumPoolSize; i++)
            {
                this.Start();
            }
            this.propagationThread = new Thread(new ThreadStart(this.propagation));
            this.propagationThread.Start();
            return this;
        }

        public IDisposable Subscribe(TimerRule instruction)
        {
            Subscription Subscription = null;
            lock (this._SubscriptionsLock)
            {
                Subscription = new LazySubscription(instruction);
                if (instruction is LazyDelegate)
                {
                    Subscription.AddMilliseconds(instruction._period);
                }
                this._Subscriptions[Subscription.GetHashCode()] = Subscription;
            }
            return Subscription;
        }

        public IDisposable Subscribe<T>(TimerRule<T> instruction, T param)
        {
            Subscription Subscription = null;
            lock (this._SubscriptionsLock)
            {
                Subscription = new ParamSubscription<T>(instruction, param);
                if (instruction is LazyDelegate<T>)
                {
                    Subscription.AddMilliseconds(instruction._period);
                }
                this._Subscriptions[Subscription.GetHashCode()] = Subscription;
            }
            return Subscription;
        }

        void IDisposable.Dispose()
        {
            this.Abort(true);
        }

        public override string ToString()
        {
            int count = this._Subscriptions.Count;
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

