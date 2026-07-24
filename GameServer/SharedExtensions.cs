using System;
using System.Collections.Concurrent;
using System.Generic;

namespace COServer
{
    public static class SharedExtensions
    {
        public static unsafe void CopyTo(this string s, void* pDest)
        {
            byte* Dest = (byte*)pDest;
            for (int i = 0; i < s.Length; i++)
            {
                Dest[i] = (byte)s[i];
            }
        }
        public static void Add<T, T2>(this ConcurrentDictionary<T, T2> dict, T key, T2 value)
        {
            dict[key] = value;
        }
        public static bool Remove<T, T2>(this ConcurrentDictionary<T, T2> dict, T key)
        {
            T2 val;
            return dict.TryRemove(key, out val);
        }
        public static void Iterate<T>(this T[] collection, Action<T> action)
        {
            foreach (var item in collection)
                action(item);
        }
    }
}
