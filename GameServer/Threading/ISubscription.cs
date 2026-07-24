
using System.Reflection;
using System.Threading;

namespace System
{
    internal abstract class ISubscription : IDisposable
    {
        internal static volatile int counter = int.MinValue;

        internal bool Viable, Enqueued;
        internal abstract void Invoke();
        internal Time32 NextInvokation;
        internal ThreadPriority Priority;
        protected int hashCode;

        public ISubscription()
        {
            counter++;
            this.hashCode = counter;

            this.Viable = true;
            this.Enqueued = false;
            this.Set(0);
        }
        ~ISubscription()
        {
            (this as IDisposable).Dispose();
        }
        internal bool Next
        {
            get { return Time32.Now > NextInvokation; }
        }

        internal void Set(int dueTime)
        {
            this.NextInvokation = Time32.Now.AddMilliseconds(dueTime);
        }

        void IDisposable.Dispose()
        {
            this.Viable = false;
            this.CleanUp();
        }

        internal abstract void CleanUp();
        internal abstract MethodInfo GetMethodInfo();
        internal abstract ThreadPriority GetPriority();

        public override int GetHashCode()
        {
            return this.hashCode;
        }
    }
}
