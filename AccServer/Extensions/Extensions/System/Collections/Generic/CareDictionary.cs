namespace System.Collections.Generic
{
    using System;    
    using System.Reflection;

    public class CareDictionary<T, T2> : SafeDictionary<T, T2>
    {
        public CareDictionary()
        {
        
        }

        public CareDictionary(int nulledNumber)
        {
       
        }

        public new void Add(T key, T2 value)
        {
            base[key] = value;
        }

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

