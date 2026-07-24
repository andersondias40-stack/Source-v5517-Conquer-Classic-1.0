using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public class MyList<T>
    {
        private T[] obj = new T[0];
        public List<T> m_list = new List<T>();
        public bool Update;

        private object SyncRoot = new object();
        public int Count => GetValues().Length;
        public T[] GetValues()
        {
            if (Update)
            {
                lock (SyncRoot)
                {
                    obj = m_list.ToArray();
                    Update = false;
                }
            }
            return obj;
        }
        public T this[int key]
        {
            get
            {
                try
                {
                    if (GetValues().Length <= key)
                        return default(T);
                    return GetValues()[key];
                }
                catch (Exception e)
                {
                    Console.WriteLine($"EX = {e.ToString()} /---/ the key {key}  /---/  the object {obj}  /---/ Value Length {GetValues().Length}");
                }
                return default(T);
            }
        }

        public void Add(T Obj)
        {
            lock (SyncRoot)
            {
                if (!m_list.Contains(Obj))
                    m_list.Add(Obj);
                Update = true;
            }
        }
        public void Remove(T Obj)
        {
            lock (SyncRoot)
            {
                m_list.Remove(Obj);
                Update = true;
            }
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                m_list.Clear();
                Update = true;
            }
        }
    }
}
