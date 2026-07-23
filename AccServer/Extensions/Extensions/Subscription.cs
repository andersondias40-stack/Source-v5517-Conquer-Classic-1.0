using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

internal abstract class Subscription : IDisposable
{
    internal bool _Active;
    internal bool running;
    internal static volatile int HashCode = -2147483648;
    protected int _HashCode;
    internal Time32 Time;

    public Subscription()
    {
        HashCode++;
        this._HashCode = HashCode;
        this._Active = true;
        this.running = false;
        this.AddMilliseconds(0);
    }

    ~Subscription()
    {
        ((IDisposable) this).Dispose();
    }

    public override int GetHashCode()
    {
        return this._HashCode;
    }

    internal bool method_0()
    {
        return (Time32.Now > this.Time);
    }

    internal void AddMilliseconds(int Amount)
    {
        this.Time = Time32.Now.AddMilliseconds(Amount);
    }

    void IDisposable.Dispose()
    {
        this._Active = false;
        this.Dispose();
    }

    internal abstract void Invoke();
    internal abstract void Dispose();
    internal abstract MethodInfo GetMethod();
    internal abstract ThreadPriority GetThreadPriority();
}

