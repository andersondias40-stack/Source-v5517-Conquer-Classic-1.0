namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;

    public class LazyDelegate : TimerRule
    {
        public LazyDelegate(Action<int> action, int dueTime, ThreadPriority priority = (ThreadPriority)2)
            : base(action, dueTime, priority)
        {
            base._Active = false;
        }
    }
}

