using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AccServer.MsgServer
{
    public struct Time32
    {
        private const string nowDebug = "The last value of Time32.Now was greater than the current value generated during this call. This is likely due to a reset in the 49.71 days period. See http://msdn.microsoft.com/en-us/library/dd757629(VS.85).aspx for more information.";

        public static Stopwatch Clock;

        public static bool isCreate = false;

        private uint value;

        private static uint lastValue;

        public static long GetClock
        {
            get
            {
                return Time32.Clock.ElapsedMilliseconds;
            }
        }

        public uint Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        public static Time32 Now
        {
            get
            {
                if (!Time32.isCreate)
                {
                    Time32.Create();
                }
                return new Time32(Time32.GetClock);
            }
        }

        public int AllMilliseconds
        {
            get
            {
                return this.GetHashCode();
            }
        }

        public int AllSeconds
        {
            get
            {
                return this.AllMilliseconds / 1000;
            }
        }

        public int AllMinutes
        {
            get
            {
                return this.AllSeconds / 60;
            }
        }

        public int AllHours
        {
            get
            {
                return this.AllMinutes / 60;
            }
        }

        public int AllDays
        {
            get
            {
                return this.AllHours / 24;
            }
        }

        public static void Create()
        {
            Time32.isCreate = true;
            Time32.Clock = new Stopwatch();
            Time32.Clock.Start();
        }

        public Time32(int Value)
        {
            this.value = (uint)Value;
        }

        public Time32(uint Value)
        {
            this.value = Value;
        }

        public Time32(long Value)
        {
            this.value = (uint)Value;
        }

        public Time32 AddMilliseconds(int Amount)
        {
            return new Time32((long)((ulong)this.value + (ulong)((long)Amount)));
        }

        public Time32 AddSeconds(int Amount)
        {
            return this.AddMilliseconds(Amount * 1000);
        }

        public Time32 AddMinutes(int Amount)
        {
            return this.AddSeconds(Amount * 60);
        }

        public Time32 AddHours(int Amount)
        {
            return this.AddMinutes(Amount * 60);
        }

        public Time32 AddDays(int Amount)
        {
            return this.AddHours(Amount * 24);
        }

        public override bool Equals(object obj)
        {
            bool result;
            if (obj is Time32)
            {
                result = ((Time32)obj == this);
            }
            else
            {
                result = base.Equals(obj);
            }
            return result;
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        public override int GetHashCode()
        {
            return (int)this.value;
        }

        public static bool operator ==(Time32 t1, Time32 t2)
        {
            return t1.value == t2.value;
        }

        public static bool operator !=(Time32 t1, Time32 t2)
        {
            return t1.value != t2.value;
        }

        public static bool operator >(Time32 t1, Time32 t2)
        {
            return t1.value > t2.value;
        }

        public static bool operator <(Time32 t1, Time32 t2)
        {
            return t1.value < t2.value;
        }

        public static bool operator >=(Time32 t1, Time32 t2)
        {
            return t1.value >= t2.value;
        }

        public static bool operator <=(Time32 t1, Time32 t2)
        {
            return t1.value <= t2.value;
        }

        public static Time32 operator -(Time32 t1, Time32 t2)
        {
            return new Time32(t1.value - t2.value);
        }

        public bool Next(int due = 0, int time = 0)
        {
            if (time == 0)
            {
                time = (int)Time32.timeGetTime().Value;
            }
            return (ulong)this.Value + (ulong)((long)due) <= (ulong)((long)time);
        }

        [DllImport("winmm.dll")]
        public static extern Time32 timeGetTime();
    }
}
