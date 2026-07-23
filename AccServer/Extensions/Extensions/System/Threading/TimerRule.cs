namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;

    public class TimerRule
    {
        internal Action<int> _action;
        internal bool _Active;
        internal int _period;
        internal ThreadPriority _ThreadPriority;

        public TimerRule(Action<int> action, int period, ThreadPriority priority = ThreadPriority.Normal)
        {
            this._action = action;
            this._period = period;
            this._Active = true;
            this._ThreadPriority = priority;
        }

        ~TimerRule()
        {
            this._action = null;
        }
    }
}

