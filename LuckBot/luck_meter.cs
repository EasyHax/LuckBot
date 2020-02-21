using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuckBot
{
    class luck_meter
    {
        public static int get_percent(DateTime birth, DateTime base_date = new DateTime())
        {
            ///if (birth == new DateTime(2000, 7, 12))
            //    return 7;

            var a = birth;
            var b = base_date == new DateTime() ? DateTime.Now : base_date;

            if (a.Month == b.Month && a.Day == b.Day)
                return 99;

            var c = Math.Abs(a.DayOfWeek - b.DayOfWeek);
            var s = a.Year + b.Year + "" + (a.Month + b.Month - 2) + "" + (a.Day + b.Day) * (c > 1 ? c * 13 : 3);
            var e = (int)(long.Parse(s) % 99);

            if (e < 23)
            {
                var f = a.Month + b.Month + (int)a.DayOfWeek + (int)b.DayOfWeek - 2;
                e += f > 9 ? f : 9;
            }

            return e;
        }
    }
}
