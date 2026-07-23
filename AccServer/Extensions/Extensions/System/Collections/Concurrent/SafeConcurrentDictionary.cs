namespace System.Collections.Concurrent
{
    using System;
    using System.Reflection;

    public class SafeConcurrentDictionary<T, T2> : ConcurrentDictionary<T, T2>
    {
        public new T2 this[T key]
        {
            get
            {
                if (base.ContainsKey(key))
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
    }
}

