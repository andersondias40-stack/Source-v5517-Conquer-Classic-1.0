
using System.Threading;

namespace System
{
    internal class Subscription : ISubscription
    {
        private TimerRule Instruction;

        public Subscription(TimerRule instruction)
            : base()
        {
            Instruction = instruction;
            Priority = instruction.Priority;
        }

        internal override void Invoke()
        {
            if (Instruction != null)
            {
                Instruction.Action((int)Time32.Now.Value);
                //in case it self-distructs
                if (Instruction != null)
                {
                    if (!Instruction.Repeat)
                        (this as IDisposable).Dispose();
                    else
                        Set(Instruction.Period);
                }
            }
        }

        internal override void CleanUp()
        {
            Instruction = null;
        }

        internal override System.Reflection.MethodInfo GetMethodInfo()
        {
            return Instruction.Action.Method;
        }

        internal override ThreadPriority GetPriority()
        {
            return Priority;
        }
    }
}
