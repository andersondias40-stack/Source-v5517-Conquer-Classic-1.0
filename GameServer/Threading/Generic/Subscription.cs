
using System.Threading;

namespace System.Generic
{
    internal class Subscription<T> : ISubscription
    {
        private TimerRule<T> Instruction;
        private T Param;

        public Subscription(TimerRule<T> instruction, T param)
            : base()
        {
            Instruction = instruction;
            Priority = Instruction.Priority;
            Param = param;
        }

        internal override void Invoke()
        {
            if (Instruction != null)
            {
                Instruction.Action(Param, (int)Time32.Now.Value);
                //Console.WriteLine("PINVOKE {0} VLAUE {1}", Param, (int)Time32.Now.Value);
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
            Param = default(T);
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
