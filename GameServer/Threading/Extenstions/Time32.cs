using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System
{
    public struct Time32
    {
        private uint value;
        public readonly static Time32 NULL = new Time32(0);
        public static Stopwatch Clock;
        public static long GetClock => Clock.ElapsedMilliseconds;
        public static bool isCreate = false;

        public static Time32 Now
        {
            get
            {
                if (!isCreate)
                {
                    Create();
                }
                return new Time32(GetClock);
                // return new Time32((uint)Environment.TickCount);
            }
        }
        public static void Create()
        {
            isCreate = true;
            Clock = new Stopwatch();
            Clock.Start();
        }

        public uint Value
        {
            get
            {
                return value;
            }
            set { this.value = value; }
        }
        public Time32(uint Value)
        {
            value = Value;
        }
        public Time32(int Value)
        {
            value = (uint)Value;
        }
        public Time32(long Value)
        {
            value = (uint)Value;
        }

        public Time32 AddMilliseconds(int Amount)
        {
            return new Time32(this.value + Amount);
        }
        public int AllMilliseconds => GetHashCode();

        public Time32 AddSeconds(int Amount)
        {
            return AddMilliseconds(Amount * 1000);
        }
        public int AllSeconds()
        {
            return AllMilliseconds / 1000;
        }

        public Time32 AddMinutes(int Amount)
        {
            return AddSeconds(Amount * 60);
        }
  
        public int AllMinutes()
        {
            return AllSeconds() / 60;
        }

        public Time32 AddHours(int Amount)
        {
            return AddMinutes(Amount * 60);
        }
   
        public int AllHours()
        {
            return AllMinutes() / 60;
        }

        public Time32 AddDays(int Amount)
        {
            return AddHours(Amount * 24);
        }

        public int AllDays()
        {
            return AllHours() / 24;
        }


        public bool Next(int due = 0, int time = 0)
        {
            if (time == 0) time = (int)Environment.TickCount;
            return (value + due <= time);
        }
        public void Set(int due, int time = 0)
        {
            if (time == 0) time = (int)Environment.TickCount;
            value = (uint)(time + due);
        }
        public void SetSeconds(int due, int time = 0)
        {
            Set(due * 1000, time);
        }

        public override bool Equals(object obj)
        {
            if (obj is Time32)
                return ((Time32)obj == this);
            return base.Equals(obj);
        }
        public override string ToString()
        {
            return value.ToString();
        }
        public override int GetHashCode()
        {
            return (int)value;
        }

        public static bool operator ==(Time32 t1, Time32 t2)
        {
            return (t1.value == t2.value);
        }
        public static bool operator !=(Time32 t1, Time32 t2)
        {
            return (t1.value != t2.value);
        }
        public static bool operator >(Time32 t1, Time32 t2)
        {
            return (t1.value > t2.value);
        }
        public static bool operator <(Time32 t1, Time32 t2)
        {
            return (t1.value < t2.value);
        }
        public static bool operator >=(Time32 t1, Time32 t2)
        {
            return (t1.value >= t2.value);
        }
        public static bool operator <=(Time32 t1, Time32 t2)
        {
            return (t1.value <= t2.value);
        }
        public static Time32 operator -(Time32 t1, Time32 t2)
        {
            return new Time32(t1.value - t2.value);
        }
    }
}
