using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace COServer
{
    public abstract class ThreadBase
    {
        private Thread _thread;
        private bool Alive = false;

        public ThreadBase()
        {
            _thread = new Thread(ThreadProcc);
            _thread.Priority = ThreadPriority.Highest;
        }
        public void Open()
        {
            if (!Alive)
            {
                Alive = true;
                _thread.Start();
            }
        }
        protected abstract void OnInit();
        protected abstract bool OnProcces();
        public void ThreadProcc()
        {
            OnInit();
            while (Alive)
            {
                try
                {
                    if (!OnProcces())
                    {
                        Close();
                        return;
                    }
                }
                catch
                {
                }    
            }
        }

        private void Close()
        {
            if (Alive)
            {
                Alive = false;
                ThreadEx.Abort(_thread);
            }
        }
    }
}
