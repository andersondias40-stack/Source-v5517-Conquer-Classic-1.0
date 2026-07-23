namespace System
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public static class Extensions
    {
        public static void Add<T, T2>(this IDictionary<T, T2> dict, T key, T2 value)
        {
            dict[key] = value;
        }

        public static void Remove<T, T2>(this IDictionary<T, T2> dict, T key)
        {
            dict.Remove(key);
        }

        public static T[] Transform<T>(this object[] strs)
        {
            T[] localArray = new T[strs.Length];
            for (int i = 0; i < strs.Length; i++)
            {
                localArray[i] = (T) Convert.ChangeType(strs[i], typeof(T));
            }
            return localArray;
        }
    }
}

