using System.Threading;
namespace System.Generic
{
    public class TimerRule<T>
    {
        internal Action<T, int> Action;
        internal int Period;
        internal bool Repeat;
        internal ThreadPriority Priority;

        public TimerRule(Action<T, int> action, int period, ThreadPriority priority = ThreadPriority.Normal)
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
