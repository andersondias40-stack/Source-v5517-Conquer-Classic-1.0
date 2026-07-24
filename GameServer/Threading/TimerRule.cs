using System.Threading;
namespace System
{
    public class TimerRule
    {
        internal Action<int> Action;
        internal int Period;
        internal bool Repeat;
        internal ThreadPriority Priority;
        public TimerRule(Action<int> action, int period, ThreadPriority priority = ThreadPriority.Normal)
        {
            this.Action = action;
            this.Period = period;
            this.Repeat = true;
            this.Priority = priority;
        }
        ~TimerRule()
        {
            this.Action = null;
        }
    }
}
