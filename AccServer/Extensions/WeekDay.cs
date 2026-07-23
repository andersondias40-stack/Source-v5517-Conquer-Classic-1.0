// * Created by TQServer
// * Copyright © 2015-2016
// * DarkTao

using System;

namespace AccServer
{
    public class WeekDay
    {
        private uint day;
        public const uint
        Sunday = 1 << 0,
        Monday = 1 << 1,
        Tuesday = 1 << 2,
        Wednesday = 1 << 3,
        Thursday = 1 << 4,
        Friday = 1 << 5,
        Saturday = 1 << 6,
        Everyday = Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday;
        public static implicit operator WeekDay(uint day)
        {
            return new WeekDay() { day = day };
        }
        public bool Contains(DayOfWeek day)
        {
            uint flag = 0;
            if (day == DayOfWeek.Friday) flag = Friday;
            if (day == DayOfWeek.Monday) flag = Monday;
            if (day == DayOfWeek.Saturday) flag = Saturday;
            if (day == DayOfWeek.Sunday) flag = Sunday;
            if (day == DayOfWeek.Thursday) flag = Thursday;
            if (day == DayOfWeek.Tuesday) flag = Tuesday;
            if (day == DayOfWeek.Wednesday) flag = Wednesday;
            return ((this.day & flag) == flag);
        }
    }
}