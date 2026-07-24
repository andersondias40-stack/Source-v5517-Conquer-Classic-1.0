// SafeDictionary<T1,T2>
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace System
{
    public class SafeDictionary<T1, T2> : Dictionary<T1, T2>
    {
        public object SyncRoot = null;

        public bool Update = false;

        public T2[] MyArray = new T2[0];

        public new T2 this[T1 key]
        {
            get
            {
                if (ContainsKey(key))
                {
                    return base[key];
                }
                return default(T2);
            }
            set
            {
                base[key] = value;
            }
        }

        public SafeDictionary()
        {
            MyArray = new T2[0];
            SyncRoot = new object();
        }

        public SafeDictionary(int cap)
            : base(cap)
        {
            SyncRoot = new object();
        }

        public new void Add(T1 key, T2 value)
        {
            try
            {
                Monitor.Enter(SyncRoot);
                base[key] = value;
                Update = true;
            }
            finally
            {
                Monitor.Exit(SyncRoot);
            }
        }

        public new void Remove(T1 key)
        {
            try
            {
                Monitor.Enter(SyncRoot);
                base.Remove(key);
                Update = true;
            }
            finally
            {
                Monitor.Exit(SyncRoot);
            }
        }

        public T2[] GetValues()
        {
            if (Update)
            {
                try
                {
                    Monitor.Enter(SyncRoot);
                    Update = false;
                    MyArray = base.Values.ToArray();
                }
                finally
                {
                    Monitor.Exit(SyncRoot);
                }
                return MyArray;
            }
            return MyArray;
        }

        public new void Clear()
        {
            lock (SyncRoot)
            {
                base.Clear();
            }
            Update = true;
        }
    }
}