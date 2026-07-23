namespace System.Threading.Generic
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class TimerRule<T>
    {
        internal Action<T, int> _Action;
        internal bool _Active;
        internal int _period;
        internal ThreadPriority _ThreadPriority;

        public TimerRule(Action<T, int> action, int period, ThreadPriority priority = ThreadPriority.Normal)
        {
            this._Action = action;
            this._period = period;
            this._Active = true;
            this._ThreadPriority = priority;
        }

        ~TimerRule()
        {
            this._Action = null;
        }
    }
}

