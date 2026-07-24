using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace COServer
{
    public class ThreadItem : ThreadBase
    {
        private Action Event;
        private int MaxProcessInvterval;
        private Action<int> sleep;
        public ThreadItem(int interval, Action _Event, Action<int> Sleep = null)
        {
            MaxProcessInvterval = interval;
            Event = _Event;
            sleep = Sleep;
        }
        protected override void OnInit()
        {
        }

        protected override bool OnProcces()
        {
            try
            {
                var Timer = Time32.Now;
                try
                {
                    if (Event != null)
                        Event();
                }
                finally
                {
                    var Timer2 = Time32.Now;
                    var id = MaxProcessInvterval - (Timer2.AllMilliseconds - Timer.AllMilliseconds);
                    if (id >= 0 && id <= MaxProcessInvterval)
                    {
                        if (sleep != null)
                            sleep(id);
                        else Thread.Sleep(id);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ex threading {e.ToString()}");
            }
            return true;
        }


    }
}
