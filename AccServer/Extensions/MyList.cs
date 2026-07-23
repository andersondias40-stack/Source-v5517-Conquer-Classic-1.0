using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Threading;
public class MyList<T>
{
    private T[] objects = new T[0];

    public List<T> m_List = new List<T>();

    public bool Update;

    public object SyncRoot = new object();

    public int Count
    {
        get
        {
            return this.GetValues().Length;
        }
    }

    public T this[int key]
    {
        get
        {
            T result;
            try
            {
                if (this.GetValues().Length <= key)
                {
                    result = default(T);
                    return result;
                }
                result = this.GetValues()[key];
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine(string.Concat(new object[]
					{
						key,
						" ",
						this.objects.Length,
						" ",
						this.GetValues().Length
					}));
            }
            result = default(T);
            return result;
        }
    }

    public void Add(T obj)
    {
        lock (this.SyncRoot)
        {
            this.m_List.Add(obj);
            this.Update = true;
        }
    }

    public T[] GetValues()
    {
        if (this.Update)
        {
            lock (this.SyncRoot)
            {
                this.objects = this.m_List.ToArray();
                this.Update = false;
            }
        }
        return this.objects;
    }

    public void Remove(T obj)
    {
        lock (this.SyncRoot)
        {
            this.m_List.Remove(obj);
            this.Update = true;
        }
    }

    public void Clear()
    {
        lock (this.SyncRoot)
        {
            this.m_List.Clear();
            this.Update = true;
        }
    }
}