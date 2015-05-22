using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime;
using System.Security;
using System.Runtime.Serialization;

namespace MyCmn
{
    /// <summary>
    /// 可以替换 System.DateTime . MyDate 主要用于格式化显示,默认显示为:yyyy-MM-dd
    /// </summary>
    /// DateTime 与 MyDate 可互相转换. 用法:
    /// <example>
    /// <code>
    ///        MyDate dt = DateTime.Now;
    ///        Console.WriteLine(dt.GetDateTime().ToString());
    ///          
    ///  // or
    ///       DateTime dt = new MyDate(DateTime.Now);
    /// </code>
    /// </example>
    [TypeConverter(typeof(MyDateConverter))]
    [Serializable]
    public struct MyDate : IComparable, IFormattable, IComparable<MyDate>, IEquatable<MyDate>, IConvertible, ISerializable
    {
        public static readonly MyDate Year2000 = new MyDate(2000, 1, 1);

        public static Func<MyDate, string> RenderFormat;

        internal DateTime date;

        public MyDate(string Date)
        {
            this.date = Date.AsDateTime();
        }

        public MyDate(DateTime Date)
        {
            this.date = Date;
        }

        public static implicit operator MyDate(DBNull value)
        {
            return new MyDate(DateTime.MinValue);
        }

        public static implicit operator MyDate(DateTime Date)
        {
            return new MyDate(Date);
        }

        public static implicit operator DateTime(MyDate Date)
        {
            return Date.date;
        }

        public override string ToString()
        {
            if (RenderFormat != null) return RenderFormat(this);
            else return date.ToString();
        }


        // 摘要:
        //     表示 System.DateTime 的最大可能值。此字段为只读。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     System.DateTime.MaxValue 位于当前区域性的默认日历或指定区域性的默认日历的范围之外。
        public static readonly MyDate MaxValue;
        //
        // 摘要:
        //     表示 System.DateTime 的最小可能值。此字段为只读。
        public static readonly MyDate MinValue;

        static MyDate()
        {
            MinValue = new MyDate(DateTime.MinValue);
        }
        //
        // 摘要:
        //     将 System.DateTime 结构的新实例初始化为指定的计时周期数。
        //
        // 参数:
        //   ticks:
        //     以 100 纳秒为单位表示的日期和时间。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     ticks 小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate(long ticks)
        {
            date = MinValue;
        }
        //
        // 摘要:
        //     将 System.DateTime 结构的新实例初始化为指定的计时周期数以及协调世界时 (UTC) 或本地时间。
        //
        // 参数:
        //   ticks:
        //     以 100 纳秒为单位表示的日期和时间。
        //
        //   kind:
        //     枚举值之一，该值指示 ticks 是指定了本地时间、协调世界时 (UTC)，还是两者皆未指定。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     ticks 小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        //
        //   System.ArgumentException:
        //     kind 不是 System.DateTimeKind 值之一。
        public MyDate(long ticks, DateTimeKind kind)
        {
            date = new DateTime(ticks, kind);
        }
        //
        // 摘要:
        //     将 System.DateTime 结构的新实例初始化为指定的年、月和日。
        //
        // 参数:
        //   year:
        //     年（1 到 9999）。
        //
        //   month:
        //     月（1 到 12）。
        //
        //   day:
        //     日（1 到 month 中的天数）。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     year 小于 1 或大于 9999。- 或 -month 小于 1 或大于 12。- 或 -day 小于 1 或大于 month 中的天数。
        //
        //   System.ArgumentException:
        //     指定的参数计算为小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate(int year, int month, int day)
        {
            date = new DateTime(year, month, day);
        }
        //
        // 摘要:
        //     将 System.DateTime 结构的新实例初始化为指定日历的指定年、月和日。
        //
        // 参数:
        //   year:
        //     年（1 到 calendar 中的年数）。
        //
        //   month:
        //     月（1 到 calendar 中的月数）。
        //
        //   day:
        //     日（1 到 month 中的天数）。
        //
        //   calendar:
        //     用于解释 year、month 和 day 的日历。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     calendar 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     year 不在 calendar 所支持的范围内。- 或 -month 小于 1 或大于 calendar 中的月数。- 或 -day 小于 1
        //     或大于 month 中的天数。
        //
        //   System.ArgumentException:
        //     指定的参数计算为小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate(int year, int month, int day, Calendar calendar)
        {
            date = new DateTime(year, month, day, calendar);
        }
        //
        // 摘要:
        //     将 System.DateTime 结构的新实例初始化为指定的年、月、日、小时、分钟和秒。
        //
        // 参数:
        //   year:
        //     年（1 到 9999）。
        //
        //   month:
        //     月（1 到 12）。
        //
        //   day:
        //     日（1 到 month 中的天数）。
        //
        //   hour:
        //     小时（0 到 23）。
        //
        //   minute:
        //     分（0 到 59）。
        //
        //   second:
        //     秒（0 到 59）。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     year 小于 1 或大于 9999。- 或 -month 小于 1 或大于 12。- 或 -day 小于 1 或大于 month 中的天数。-
        //     或 -hour 小于 0 或大于 23。- 或 -minute 小于 0 或大于 59。- 或 -second 小于 0 或大于 59。
        //
        //   System.ArgumentException:
        //     指定的参数计算为小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate(int year, int month, int day, int hour, int minute, int second)
        {
            date = new DateTime(year, month, day, hour, minute, second);
        }
        //
        // 摘要:
        //     将 System.DateTime 结构的新实例初始化为指定日历的指定年、月、日、小时、分钟和秒。
        //
        // 参数:
        //   year:
        //     年（1 到 calendar 中的年数）。
        //
        //   month:
        //     月（1 到 calendar 中的月数）。
        //
        //   day:
        //     日（1 到 month 中的天数）。
        //
        //   hour:
        //     小时（0 到 23）。
        //
        //   minute:
        //     分（0 到 59）。
        //
        //   second:
        //     秒（0 到 59）。
        //
        //   calendar:
        //     用于解释 year、month 和 day 的日历。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     calendar 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     year 不在 calendar 所支持的范围内。- 或 -month 小于 1 或大于 calendar 中的月数。- 或 -day 小于 1
        //     或大于 month 中的天数。- 或 -hour 小于 0 或大于 23- 或 -minute 小于 0 或大于 59。- 或 -second 小于
        //     0 或大于 59。
        //
        //   System.ArgumentException:
        //     指定的参数计算为小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate(int year, int month, int day, int hour, int minute, int second, Calendar calendar)
        {
            date = new DateTime(year, month, day, hour, minute, second, calendar);
        }
        //
        // 摘要:
        //     将 System.DateTime 结构的新实例初始化为指定年、月、日、小时、分钟、秒和协调世界时 (UTC) 或本地时间。
        //
        // 参数:
        //   year:
        //     年（1 到 9999）。
        //
        //   month:
        //     月（1 到 12）。
        //
        //   day:
        //     日（1 到 month 中的天数）。
        //
        //   hour:
        //     小时（0 到 23）。
        //
        //   minute:
        //     分（0 到 59）。
        //
        //   second:
        //     秒（0 到 59）。
        //
        //   kind:
        //     枚举值之一，该值指示 year、month、day、hour、minute 和 second 是指定了本地时间、协调世界时 (UTC)，还是两者皆未指定。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     year 小于 1 或大于 9999。- 或 -month 小于 1 或大于 12。- 或 -day 小于 1 或大于 month 中的天数。-
        //     或 -hour 小于 0 或大于 23。- 或 -minute 小于 0 或大于 59。- 或 -second 小于 0 或大于 59。
        //
        //   System.ArgumentException:
        //     指定的时间参数的取值小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。- 或 -kind
        //     不是 System.DateTimeKind 值之一。
        public MyDate(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
        {
            date = new DateTime(year, month, day, hour, minute, second, kind);
        }
        //
        // 摘要:
        //     将 System.DateTime 结构的新实例初始化为指定的年、月、日、小时、分钟、秒和毫秒。
        //
        // 参数:
        //   year:
        //     年（1 到 9999）。
        //
        //   month:
        //     月（1 到 12）。
        //
        //   day:
        //     日（1 到 month 中的天数）。
        //
        //   hour:
        //     小时（0 到 23）。
        //
        //   minute:
        //     分（0 到 59）。
        //
        //   second:
        //     秒（0 到 59）。
        //
        //   millisecond:
        //     毫秒（0 到 999）。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     year 小于 1 或大于 9999。- 或 -month 小于 1 或大于 12。- 或 -day 小于 1 或大于 month 中的天数。-
        //     或 -hour 小于 0 或大于 23。- 或 -minute 小于 0 或大于 59。- 或 -second 小于 0 或大于 59。- 或 -millisecond
        //     小于 0 或大于 999。
        //
        //   System.ArgumentException:
        //     指定的参数计算为小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate(int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            date = new DateTime(year, month, day, hour, minute, second, millisecond);
        }
        //
        // 摘要:
        //     将 System.DateTime 结构的新实例初始化为指定日历的指定年、月、日、小时、分钟、秒和毫秒。
        //
        // 参数:
        //   year:
        //     年（1 到 calendar 中的年数）。
        //
        //   month:
        //     月（1 到 calendar 中的月数）。
        //
        //   day:
        //     日（1 到 month 中的天数）。
        //
        //   hour:
        //     小时（0 到 23）。
        //
        //   minute:
        //     分（0 到 59）。
        //
        //   second:
        //     秒（0 到 59）。
        //
        //   millisecond:
        //     毫秒（0 到 999）。
        //
        //   calendar:
        //     用于解释 year、month 和 day 的日历。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     calendar 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     year 不在 calendar 所支持的范围内。- 或 -month 小于 1 或大于 calendar 中的月数。- 或 -day 小于 1
        //     或大于 month 中的天数。- 或 -hour 小于 0 或大于 23。- 或 -minute 小于 0 或大于 59。- 或 -second
        //     小于 0 或大于 59。- 或 -millisecond 小于 0 或大于 999。
        //
        //   System.ArgumentException:
        //     指定的参数计算为小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar)
        {
            date = new DateTime(year, month, day, hour, minute, second, millisecond, calendar);
        }
        //
        // 摘要:
        //     将 System.DateTime 结构的新实例初始化为指定年、月、日、小时、分钟、秒、毫秒和协调世界时 (UTC) 或本地时间。
        //
        // 参数:
        //   year:
        //     年（1 到 9999）。
        //
        //   month:
        //     月（1 到 12）。
        //
        //   day:
        //     日（1 到 month 中的天数）。
        //
        //   hour:
        //     小时（0 到 23）。
        //
        //   minute:
        //     分（0 到 59）。
        //
        //   second:
        //     秒（0 到 59）。
        //
        //   millisecond:
        //     毫秒（0 到 999）。
        //
        //   kind:
        //     枚举值之一，该值指示 year、month、day、hour、minute、second 和 millisecond 是指定了本地时间、协调世界时
        //     (UTC)，还是两者皆未指定。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     year 小于 1 或大于 9999。- 或 -month 小于 1 或大于 12。- 或 -day 小于 1 或大于 month 中的天数。-
        //     或 -hour 小于 0 或大于 23。- 或 -minute 小于 0 或大于 59。- 或 -second 小于 0 或大于 59。- 或 -millisecond
        //     小于 0 或大于 999。
        //
        //   System.ArgumentException:
        //     指定的时间参数的取值小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。- 或 -kind
        //     不是 System.DateTimeKind 值之一。
        public MyDate(int year, int month, int day, int hour, int minute, int second, int millisecond, DateTimeKind kind)
        {
            date = new DateTime(year, month, day, hour, minute, second, millisecond, kind);
        }
        //
        // 摘要:
        //     将 System.DateTime 结构的新实例初始化为指定日历的指定年、月、日、小时、分钟、秒、毫秒和协调世界时 (UTC) 或本地时间。
        //
        // 参数:
        //   year:
        //     年（1 到 calendar 中的年数）。
        //
        //   month:
        //     月（1 到 calendar 中的月数）。
        //
        //   day:
        //     日（1 到 month 中的天数）。
        //
        //   hour:
        //     小时（0 到 23）。
        //
        //   minute:
        //     分（0 到 59）。
        //
        //   second:
        //     秒（0 到 59）。
        //
        //   millisecond:
        //     毫秒（0 到 999）。
        //
        //   calendar:
        //     用于解释 year、month 和 day 的日历。
        //
        //   kind:
        //     枚举值之一，该值指示 year、month、day、hour、minute、second 和 millisecond 是指定了本地时间、协调世界时
        //     (UTC)，还是两者皆未指定。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     calendar 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     year 不在 calendar 所支持的范围内。- 或 -month 小于 1 或大于 calendar 中的月数。- 或 -day 小于 1
        //     或大于 month 中的天数。- 或 -hour 小于 0 或大于 23。- 或 -minute 小于 0 或大于 59。- 或 -second
        //     小于 0 或大于 59。- 或 -millisecond 小于 0 或大于 999。
        //
        //   System.ArgumentException:
        //     指定的时间参数的取值小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。- 或 -kind
        //     不是 System.DateTimeKind 值之一。
        public MyDate(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar, DateTimeKind kind)
        {
            date = new DateTime(year, month, day, hour, minute, second, millisecond, calendar, kind);
        }

        // 摘要:
        //     将指定的日期和时间与另一个指定的日期和时间相减，返回一个时间间隔。
        //
        // 参数:
        //   d1:
        //     System.DateTime（被减数）。
        //
        //   d2:
        //     System.DateTime（减数）。
        //
        // 返回结果:
        //     System.TimeSpan，它是 d1 和 d2 之间的时间间隔，即 d1 减去 d2。
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static TimeSpan operator -(MyDate d1, MyDate d2)
        {
            return d1.date - d2.date;
        }
        public static TimeSpan operator -(MyDate d1, DateTime d2)
        {
            return d1.date - d2;
        }
        public static TimeSpan operator -(DateTime d1, MyDate d2)
        {
            return d1 - d2.date;
        }
        //
        // 摘要:
        //     从指定的日期和时间减去指定的时间间隔，返回新的日期和时间。
        //
        // 参数:
        //   d:
        //     System.DateTime。
        //
        //   t:
        //     System.TimeSpan。
        //
        // 返回结果:
        //     System.DateTime，它的值为 d 的值减去 t 的值。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     所生成的 System.DateTime 小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public static MyDate operator -(MyDate d, TimeSpan t)
        {
            return d.date - t;
        }
        //
        // 摘要:
        //     确定 System.DateTime 的两个指定实例是否不等。
        //
        // 参数:
        //   d1:
        //     System.DateTime。
        //
        //   d2:
        //     System.DateTime。
        //
        // 返回结果:
        //     如果 d1 和 d2 不表示同一日期和时间，则为 true；否则为 false。
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator !=(MyDate d1, MyDate d2)
        {
            return d1.date != d2.date;
        }
        public static bool operator !=(DateTime d1, MyDate d2)
        {
            return d1 != d2.date;
        }
        public static bool operator !=(MyDate d1, DateTime d2)
        {
            return d1.date != d2;
        }
        //
        // 摘要:
        //     将指定的时间间隔加到指定的日期和时间以生成新的日期和时间。
        //
        // 参数:
        //   d:
        //     System.DateTime。
        //
        //   t:
        //     System.TimeSpan。
        //
        // 返回结果:
        //     System.DateTime，它是 d 和 t 值的和。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     所生成的 System.DateTime 小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public static MyDate operator +(MyDate d, TimeSpan t)
        {
            return d.date + t;
        }
        //
        // 摘要:
        //     确定指定的 System.DateTime 是否小于另一个指定的 System.DateTime。
        //
        // 参数:
        //   t1:
        //     System.DateTime。
        //
        //   t2:
        //     System.DateTime。
        //
        // 返回结果:
        //     如果 t1 小于 t2，则为 true；否则为 false。
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator <(MyDate t1, MyDate t2)
        {
            return t1.date < t2.date;
        }
        public static bool operator <(DateTime t1, MyDate t2)
        {
            return t1 < t2.date;
        }
        public static bool operator <(MyDate t1, DateTime t2)
        {
            return t1.date < t2;
        }
        //
        // 摘要:
        //     确定指定的 System.DateTime 是否小于或等于另一个指定的 System.DateTime。
        //
        // 参数:
        //   t1:
        //     System.DateTime。
        //
        //   t2:
        //     System.DateTime。
        //
        // 返回结果:
        //     如果 t1 小于或等于 t2，则为 true；否则为 false。
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator <=(MyDate t1, MyDate t2)
        {
            return t1.date <= t2.date;
        }

        public static bool operator <=(DateTime t1, MyDate t2)
        {
            return t1 <= t2.date;
        }
        public static bool operator <=(MyDate t1, DateTime t2)
        {
            return t1.date <= t2;
        }
        //
        // 摘要:
        //     确定 System.DateTime 的两个指定的实例是否相等。
        //
        // 参数:
        //   d1:
        //     System.DateTime。
        //
        //   d2:
        //     System.DateTime。
        //
        // 返回结果:
        //     如果 d1 和 d2 表示同一日期和时间，则为 true；否则为 false。
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator ==(MyDate d1, MyDate d2)
        {
            return d1.date == d2.date;
        }

        public static bool operator ==(DateTime d1, MyDate d2)
        {
            return d1 == d2.date;
        }
        public static bool operator ==(MyDate d1, DateTime d2)
        {
            return d1.date == d2;
        }
        //
        // 摘要:
        //     确定指定的 System.DateTime 是否大于另一个指定的 System.DateTime。
        //
        // 参数:
        //   t1:
        //     System.DateTime。
        //
        //   t2:
        //     System.DateTime。
        //
        // 返回结果:
        //     如果 t1 大于 t2，则为 true；否则为 false。
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator >(MyDate t1, MyDate t2)
        {
            return t1.date > t2.date;
        }
        public static bool operator >(DateTime t1, MyDate t2)
        {
            return t1 > t2.date;
        }
        public static bool operator >(MyDate t1, DateTime t2)
        {
            return t1.date > t2;
        }
        // 
        // 摘要:
        //     确定指定的 System.DateTime 是否大于等于另一个指定的 System.DateTime。
        //
        // 参数:
        //   t1:
        //     System.DateTime。
        //
        //   t2:
        //     System.DateTime。
        //
        // 返回结果:
        //     如果 t1 大于等于 t2，则为 true；否则为 false。
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator >=(MyDate t1, MyDate t2)
        {
            return t1.date >= t2.date;
        }
        public static bool operator >=(DateTime t1, MyDate t2)
        {
            return t1 >= t2.date;
        }
        public static bool operator >=(MyDate t1, DateTime t2)
        {
            return t1.date >= t2;
        }

        // 摘要:
        //     获取此实例的日期部分。
        //
        // 返回结果:
        //     新的 System.DateTime，其日期与此实例相同，时间值设置为午夜 12:00:00 (00:00:00)。
        public MyDate Date { get { return this.date.Date; } }
        //
        // 摘要:
        //     获取此实例所表示的日期为该月中的第几天。
        //
        // 返回结果:
        //     日组成部分，表示为 1 和 31 之间的一个值。
        public int Day { get { return date.Day; } }
        //
        // 摘要:
        //     获取此实例所表示的日期是星期几。
        //
        // 返回结果:
        //     一个 System.DayOfWeek 枚举常量，指示此 System.DateTime 值是星期几。
        public DayOfWeek DayOfWeek { get { return date.DayOfWeek; } }
        //
        // 摘要:
        //     获取此实例所表示的日期是该年中的第几天。
        //
        // 返回结果:
        //     该年中的第几天，表示为 1 和 366 之间的一个值。
        public int DayOfYear { get { return date.DayOfYear; } }
        //
        // 摘要:
        //     获取此实例所表示日期的小时部分。
        //
        // 返回结果:
        //     小时组成部分，表示为 0 和 23 之间的一个值。
        public int Hour { get { return date.Hour; } }
        //
        // 摘要:
        //     获取一个值，该值指示由此实例表示的时间是基于本地时间、协调世界时 (UTC)，还是两者皆否。
        //
        // 返回结果:
        //     System.DateTimeKind 值之一。默认值为 System.DateTimeKind.Unspecified。
        public DateTimeKind Kind { get { return date.Kind; } }
        //
        // 摘要:
        //     获取此实例所表示日期的毫秒部分。
        //
        // 返回结果:
        //     毫秒组成部分，表示为 0 和 999 之间的一个值。
        public int Millisecond { get { return date.Millisecond; } }
        //
        // 摘要:
        //     获取此实例所表示日期的分钟部分。
        //
        // 返回结果:
        //     分钟组成部分，表示为 0 和 59 之间的一个值。
        public int Minute { get { return date.Minute; } }
        //
        // 摘要:
        //     获取此实例所表示日期的月份部分。
        //
        // 返回结果:
        //     月组成部分，表示为 1 和 12 之间的一个值。
        public int Month { get { return date.Month; } }
        //
        // 摘要:
        //     获取一个 System.DateTime 对象，该对象设置为此计算机上的当前日期和时间，表示为本地时间。
        //
        // 返回结果:
        //     其值为当前日期和时间的 System.DateTime。
        public static MyDate Now { get { return DateTime.Now; } }
        //
        // 摘要:
        //     获取此实例所表示日期的秒部分。
        //
        // 返回结果:
        //     秒数（介于 0 和 59 之间）。
        public int Second { get { return date.Second; } }
        //
        // 摘要:
        //     获取表示此实例的日期和时间的计时周期数。
        //
        // 返回结果:
        //     表示此实例的日期和时间的计时周期数。该值介于 DateTime.MinValue.Ticks 和 DateTime.MaxValue.Ticks
        //     之间。
        public long Ticks { get { return date.Ticks; } }
        //
        // 摘要:
        //     获取此实例的当天的时间。
        //
        // 返回结果:
        //     System.TimeSpan，它表示当天自午夜以来已经过时间的部分。
        public TimeSpan TimeOfDay { get { return date.TimeOfDay; } }
        //
        // 摘要:
        //     获取当前日期。
        //
        // 返回结果:
        //     设置为当天日期的 System.DateTime，其时间组成部分设置为 00:00:00。
        public static MyDate Today { get { return DateTime.Today; } }
        //
        // 摘要:
        //     获取一个 System.DateTime 对象，该对象设置为此计算机上的当前日期和时间，表示为协调世界时 (UTC)。
        //
        // 返回结果:
        //     其值为当前 UTC 日期和时间的 System.DateTime。
        public static MyDate UtcNow { get { return DateTime.UtcNow; } }
        //
        // 摘要:
        //     获取此实例所表示日期的年份部分。
        //
        // 返回结果:
        //     年份（介于 1 和 9999 之间）。
        public int Year { get { return date.Year; } }

        // 摘要:
        //     返回一个新的 System.DateTime，它将指定 System.TimeSpan 的值加到此实例的值上。
        //
        // 参数:
        //   value:
        //     一个 System.TimeSpan 对象，表示正时间间隔或负时间间隔。
        //
        // 返回结果:
        //     System.DateTime，其值是此实例所表示的日期和时间与 value 所表示的时间间隔之和。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     所生成的 System.DateTime 小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate Add(TimeSpan value) { return date.Add(value); }
        //
        // 摘要:
        //     返回一个新的 System.DateTime，它将指定的天数加到此实例的值上。
        //
        // 参数:
        //   value:
        //     由整数和小数部分组成的天数。value 参数可以是负数也可以是正数。
        //
        // 返回结果:
        //     System.DateTime，其值是此实例所表示的日期和时间与 value 所表示的天数之和。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     所生成的 System.DateTime 小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate AddDays(double value) { return date.AddDays(value); }
        //
        // 摘要:
        //     返回一个新的 System.DateTime，它将指定的小时数加到此实例的值上。
        //
        // 参数:
        //   value:
        //     由整数和小数部分组成的小时数。value 参数可以是负数也可以是正数。
        //
        // 返回结果:
        //     System.DateTime，其值是此实例所表示的日期和时间与 value 所表示的小时数之和。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     所生成的 System.DateTime 小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate AddHours(double value) { return date.AddHours(value); }
        //
        // 摘要:
        //     返回一个新的 System.DateTime，它将指定的毫秒数加到此实例的值上。
        //
        // 参数:
        //   value:
        //     由整数和小数部分组成的毫秒数。value 参数可以是负数也可以是正数。请注意，该值被舍入到最近的整数。
        //
        // 返回结果:
        //     System.DateTime，其值是此实例所表示的日期和时间与 value 所表示的毫秒数之和。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     所生成的 System.DateTime 小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate AddMilliseconds(double value) { return date.AddMilliseconds(value); }
        //
        // 摘要:
        //     返回一个新的 System.DateTime，它将指定的分钟数加到此实例的值上。
        //
        // 参数:
        //   value:
        //     由整数和小数部分组成的分钟数。value 参数可以是负数也可以是正数。
        //
        // 返回结果:
        //     System.DateTime，其值是此实例所表示的日期和时间与 value 所表示的分钟数之和。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     所生成的 System.DateTime 小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate AddMinutes(double value)
        {
            return date.AddMilliseconds(value);
        }
        //
        // 摘要:
        //     返回一个新的 System.DateTime，它将指定的月数加到此实例的值上。
        //
        // 参数:
        //   months:
        //     月份数。months 参数可以是负数也可以是正数。
        //
        // 返回结果:
        //     System.DateTime，其值是此实例所表示的日期和时间与 months 之和。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     所生成的 System.DateTime 小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。-
        //     或 -months 小于 -120,000 或大于 120,000。
        public MyDate AddMonths(int months) { return date.AddMonths(months); }
        //
        // 摘要:
        //     返回一个新的 System.DateTime，它将指定的秒数加到此实例的值上。
        //
        // 参数:
        //   value:
        //     由整数和小数部分组成的秒数。value 参数可以是负数也可以是正数。
        //
        // 返回结果:
        //     System.DateTime，其值是此实例所表示的日期和时间与 value 所表示的秒数之和。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     所生成的 System.DateTime 小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate AddSeconds(double value) { return date.AddSeconds(value); }
        //
        // 摘要:
        //     返回一个新的 System.DateTime，它将指定的计时周期数加到此实例的值上。
        //
        // 参数:
        //   value:
        //     以 100 纳秒为单位的计时周期数。value 参数可以是正数也可以是负数。
        //
        // 返回结果:
        //     System.DateTime，其值是此实例所表示的日期和时间与 value 所表示时间之和。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     所生成的 System.DateTime 小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate AddTicks(long value) { return date.AddTicks(value); }
        //
        // 摘要:
        //     返回一个新的 System.DateTime，它将指定的年份数加到此实例的值上。
        //
        // 参数:
        //   value:
        //     年份数。value 参数可以是负数也可以是正数。
        //
        // 返回结果:
        //     System.DateTime，其值是此实例所表示的日期和时间与 value 所表示的年份数之和。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     value 或得到的 System.DateTime 小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate AddYears(int value) { return date.AddYears(value); }
        //
        // 摘要:
        //     对两个 System.DateTime 的实例进行比较，并返回一个指示第一个实例是早于、等于还是晚于第二个实例的整数。
        //
        // 参数:
        //   t1:
        //     第一个 System.DateTime。
        //
        //   t2:
        //     第二个 System.DateTime。
        //
        // 返回结果:
        //     一个有符号数字，指示 t1 和 t2 的相对值。值类型条件小于零t1 早于 t2。零t1 与 t2 相同。大于零t1 晚于 t2。
        public static int Compare(MyDate t1, MyDate t2) { return DateTime.Compare(t1.date, t2.date); }
        //
        // 摘要:
        //     将此实例的值与指定的 System.DateTime 值相比较，并返回一个整数，该整数指示此实例是早于、等于还是晚于指定的 System.DateTime
        //     值。
        //
        // 参数:
        //   value:
        //     要比较的 System.DateTime 对象。
        //
        // 返回结果:
        //     有符号数字，指示此实例和 value 参数的相对值。值说明小于零此实例早于 value。零此实例与 value 相同。大于零此实例晚于 value。
        public int CompareTo(MyDate value) { return date.CompareTo(value); }
        //
        // 摘要:
        //     将此实例的值与包含指定的 System.DateTime 值的指定对象相比较，并返回一个整数，该整数指示此实例是早于、等于还是晚于指定的 System.DateTime
        //     值。
        //
        // 参数:
        //   value:
        //     要比较的 System.DateTime 装箱对象，或 null。
        //
        // 返回结果:
        //     一个有符号数字，指示此实例和 value 的相对值。值说明小于零此实例早于 value。零此实例与 value 相同。大于零此实例晚于 value
        //     或 value 为 null。
        //
        // 异常:
        //   System.ArgumentException:
        //     value 不是 System.DateTime。
        public int CompareTo(object value) { return date.CompareTo(value.AsDateTime()); }
        //
        // 摘要:
        //     返回指定年和月中的天数。
        //
        // 参数:
        //   year:
        //     年。
        //
        //   month:
        //     月（介于 1 到 12 之间的一个数字）。
        //
        // 返回结果:
        //     指定 year 中 month 的天数。例如，如果 month 等于 2（表示二月），则返回值为 28 或 29，具体取决于 year 是否为闰年。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     month 小于 1 或大于 12。- 或 -year 小于 1 或大于 9999。
        public static int DaysInMonth(int year, int month) { return DateTime.DaysInMonth(year, month); }
        //
        // 摘要:
        //     返回一个值，该值指示此实例是否与指定的 System.DateTime 实例相等。
        //
        // 参数:
        //   value:
        //     要与此实例进行比较的 System.DateTime 实例。
        //
        // 返回结果:
        //     如果 value 参数等于此实例的值，则为 true；否则为 false。
        public bool Equals(MyDate value) { return date.Equals(value.date); }
        //
        // 摘要:
        //     返回一个值，该值指示此实例是否与指定的对象相等。
        //
        // 参数:
        //   value:
        //     要与此实例进行比较的对象。
        //
        // 返回结果:
        //     如果 value 是 System.DateTime 的实例并且等于此实例的值，则为 true；否则为 false。
        public override bool Equals(object value)
        {
            if (value.IsDBNull()) return false;
            var type = value.GetType();
            if (type.IsEnum) return false;

            if (type == typeof(MyDate) || type.IsSubclassOf(typeof(MyDate))) return MyDate.Equals(this, (MyDate)value);
            else if (type == typeof(DateTime) || type.IsSubclassOf(typeof(DateTime))) return DateTime.Equals(this.date, (DateTime)value);

            return DateTime.Equals(this.date, value.AsDateTime());
        }
        //
        // 摘要:
        //     返回一个值，该值指示 System.DateTime 的两个实例是否相等。
        //
        // 参数:
        //   t1:
        //     第一个 System.DateTime 实例。
        //
        //   t2:
        //     第二个 System.DateTime 实例。
        //
        // 返回结果:
        //     如果两个 System.DateTime 值相等，则为 true；否则为 false。
        public static bool Equals(MyDate t1, MyDate t2) { return DateTime.Equals(t1.date, t2.date); }
        //
        // 摘要:
        //     反序列化一个 64 位二进制值，并重新创建序列化的 System.DateTime 初始对象。
        //
        // 参数:
        //   dateData:
        //     64 位有符号整数，它对 2 位字段的 System.DateTime.Kind 属性以及 62 位字段的 System.DateTime.Ticks
        //     属性进行了编码。
        //
        // 返回结果:
        //     一个 System.DateTime 对象，它等效于由 System.DateTime.ToBinary() 方法序列化的 System.DateTime
        //     对象。
        //
        // 异常:
        //   System.ArgumentException:
        //     dateData 小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public static MyDate FromBinary(long dateData) { return DateTime.FromBinary(dateData); }
        //
        // 摘要:
        //     将指定的 Windows 文件时间转换为等效的本地时间。
        //
        // 参数:
        //   fileTime:
        //     以计时周期表示的 Windows 文件时间。
        //
        // 返回结果:
        //     一个 System.DateTime 对象，表示等效于由 fileTime 参数表示的日期和时间的本地时间。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     fileTime 小于零或表示大于 System.DateTime.MaxValue 的时间。
        public static MyDate FromFileTime(long fileTime) { return DateTime.FromFileTime(fileTime); }
        //
        // 摘要:
        //     将指定的 Windows 文件时间转换为等效的 UTC 时间。
        //
        // 参数:
        //   fileTime:
        //     以计时周期表示的 Windows 文件时间。
        //
        // 返回结果:
        //     一个 System.DateTime 对象，表示等效于由 fileTime 参数表示的日期和时间的 UTC 时间。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     fileTime 小于零或表示大于 System.DateTime.MaxValue 的时间。
        public static MyDate FromFileTimeUtc(long fileTime) { return DateTime.FromFileTimeUtc(fileTime); }
        //
        // 摘要:
        //     返回与指定的 OLE 自动化日期等效的 System.DateTime。
        //
        // 参数:
        //   d:
        //     OLE 自动化日期值。
        //
        // 返回结果:
        //     System.DateTime，它表示与 d 相同的日期和时间。
        //
        // 异常:
        //   System.ArgumentException:
        //     该日期不是有效的 OLE 自动化日期值。
        public static MyDate FromOADate(double d) { return DateTime.FromOADate(d); }
        //
        // 摘要:
        //     将此实例的值转换为标准 System.DateTime 格式说明符支持的所有字符串表示形式。
        //
        // 返回结果:
        //     字符串数组，其中的每个元素都表示此实例的值，并且已用标准 System.DateTime 格式设置说明符之一设置格式。
        public string[] GetDateTimeFormats() { return date.GetDateTimeFormats(); }
        //
        // 摘要:
        //     将此实例的值转换为指定的标准 System.DateTime 格式说明符支持的所有字符串表示形式。
        //
        // 参数:
        //   format:
        //     标准日期和时间格式的字符串。（请参见Standard Date and Time Format Strings。）
        //
        // 返回结果:
        //     字符串数组，其中每个元素都是此实例的值的表示形式，并且已用 format 标准 System.DateTime 格式说明符格式化。
        //
        // 异常:
        //   System.FormatException:
        //     format 不是有效的标准日期和时间格式说明符。
        public string[] GetDateTimeFormats(char format) { return date.GetDateTimeFormats(format); }
        //
        // 摘要:
        //     将此实例的值转换为标准 System.DateTime 格式说明符和指定的区域性特定格式信息支持的所有字符串表示形式。
        //
        // 参数:
        //   provider:
        //     一个对象，它提供有关此实例的区域性特定格式设置信息。
        //
        // 返回结果:
        //     字符串数组，其中的每个元素都表示此实例的值，并且已用标准 System.DateTime 格式设置说明符之一设置格式。
        public string[] GetDateTimeFormats(IFormatProvider provider) { return date.GetDateTimeFormats(provider); }
        //
        // 摘要:
        //     将此实例的值转换为指定的标准 System.DateTime 格式说明符和区域性特定格式信息支持的所有字符串表示形式。
        //
        // 参数:
        //   format:
        //     日期和时间格式的字符串。
        //
        //   provider:
        //     一个对象，它提供有关此实例的区域性特定格式设置信息。
        //
        // 返回结果:
        //     字符串数组，其中的每个元素都表示此实例的值，并且已用标准 System.DateTime 格式设置说明符之一设置格式。
        //
        // 异常:
        //   System.FormatException:
        //     format 不是有效的标准日期和时间格式说明符。
        public string[] GetDateTimeFormats(char format, IFormatProvider provider) { return date.GetDateTimeFormats(format, provider); }
        //
        // 摘要:
        //     返回此实例的哈希代码。
        //
        // 返回结果:
        //     32 位有符号整数哈希代码。
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override int GetHashCode() { return date.GetHashCode(); }
        //
        // 摘要:
        //     返回值类型 System.DateTime 的 System.TypeCode。
        //
        // 返回结果:
        //     枚举常数 System.TypeCode.DateTime。
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public TypeCode GetTypeCode() { return date.GetTypeCode(); }
        //
        // 摘要:
        //     指示此 System.DateTime 实例是否在当前时区的夏时制范围内。
        //
        // 返回结果:
        //     如果 System.DateTime.Kind 为 System.DateTimeKind.Local 或 System.DateTimeKind.Unspecified
        //     并且此 System.DateTime 实例的值在当前时区的夏时制范围以内，则为 true。如果 System.DateTime.Kind 为 System.DateTimeKind.Utc，则返回
        //     false。
        public bool IsDaylightSavingTime() { return date.IsDaylightSavingTime(); }
        //
        // 摘要:
        //     返回指定的年份是否为闰年的指示。
        //
        // 参数:
        //   year:
        //     四位数年份。
        //
        // 返回结果:
        //     如果 year 为闰年，则为 true；否则为 false。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     year 小于 1 或大于 9999。
        public static bool IsLeapYear(int year) { return DateTime.IsLeapYear(year); }
        //
        // 摘要:
        //     将日期和时间的指定字符串表示形式转换为其等效的 System.DateTime。
        //
        // 参数:
        //   s:
        //     包含要转换的日期和时间的字符串。
        //
        // 返回结果:
        //     等效于 s 中包含的日期和时间的 System.DateTime。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     s 为 null。
        //
        //   System.FormatException:
        //     s 中不包含日期和时间的有效字符串表示形式。
        [SecuritySafeCritical]
        public static MyDate Parse(string s) { return DateTime.Parse(s); }
        //
        // 摘要:
        //     使用指定的区域性特定格式信息，将日期和时间的指定字符串表示形式转换为其等效的 System.DateTime。
        //
        // 参数:
        //   s:
        //     包含要转换的日期和时间的字符串。
        //
        //   provider:
        //     一个对象，提供有关 s 的区域性特定格式信息。
        //
        // 返回结果:
        //     System.DateTime，等效于由 provider 所指定的 s 中包含的日期和时间。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     s 为 null。
        //
        //   System.FormatException:
        //     s 中不包含日期和时间的有效字符串表示形式。
        [SecuritySafeCritical]
        public static MyDate Parse(string s, IFormatProvider provider) { return DateTime.Parse(s, provider); }
        //
        // 摘要:
        //     使用指定的区域性特定格式信息和格式设置样式将日期和时间的指定字符串表示形式转换为其等效的 System.DateTime。
        //
        // 参数:
        //   s:
        //     包含要转换的日期和时间的字符串。
        //
        //   provider:
        //     一个对象，提供有关 s 的区域性特定格式设置信息。
        //
        //   styles:
        //     枚举值的按位组合，用于指示 s 成功执行分析操作所需的样式元素以及定义如何根据当前时区或当前日期解释已分析日期的样式元素。一个要指定的典型值为 System.Globalization.DateTimeStyles.None。
        //
        // 返回结果:
        //     System.DateTime，等效于由 provider 和 styles 所指定的 s 中包含的日期和时间。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     s 为 null。
        //
        //   System.FormatException:
        //     s 中不包含日期和时间的有效字符串表示形式。
        //
        //   System.ArgumentException:
        //     styles 包含 System.Globalization.DateTimeStyles 值的无效组合。例如，System.Globalization.DateTimeStyles.AssumeLocal
        //     和 System.Globalization.DateTimeStyles.AssumeUniversal。
        [SecuritySafeCritical]
        public static MyDate Parse(string s, IFormatProvider provider, DateTimeStyles styles) { return DateTime.Parse(s, provider, styles); }
        //
        // 摘要:
        //     使用指定的格式和区域性特定格式信息，将日期和时间的指定字符串表示形式转换为其等效的 System.DateTime。字符串表示形式的格式必须与指定的格式完全匹配。
        //
        // 参数:
        //   s:
        //     包含要转换的日期和时间的字符串。
        //
        //   format:
        //     用于定义所需的 s 格式的格式说明符。
        //
        //   provider:
        //     一个对象，提供有关 s 的区域性特定格式信息。
        //
        // 返回结果:
        //     System.DateTime，等效于由 format 和 provider 所指定的 s 中包含的日期和时间。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     s 或 format 为 null。
        //
        //   System.FormatException:
        //     s 或 format 是空字符串。- 或 -s 不包含与 format 中指定的模式相对应的日期和时间。- 或 -s 中的小时组成部分和 AM/PM
        //     指示符不一致。
        public static MyDate ParseExact(string s, string format, IFormatProvider provider)
        { return DateTime.ParseExact(s, format, provider); }
        //
        // 摘要:
        //     使用指定的格式、区域性特定的格式信息和样式将日期和时间的指定字符串表示形式转换为其等效的 System.DateTime。字符串表示形式的格式必须与指定的格式完全匹配，否则会引发异常。
        //
        // 参数:
        //   s:
        //     包含要转换的日期和时间的字符串。
        //
        //   format:
        //     用于定义所需的 s 格式的格式说明符。
        //
        //   provider:
        //     一个对象，提供有关 s 的区域性特定格式设置信息。
        //
        //   style:
        //     枚举值的按位组合，提供有关以下内容的附加信息：s、可能出现在 s 中的样式元素或从 s 到 System.DateTime 值的转换。一个要指定的典型值为
        //     System.Globalization.DateTimeStyles.None。
        //
        // 返回结果:
        //     System.DateTime，等效于由 format、provider 和 style 所指定的 s 中所包含的日期和时间。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     s 或 format 为 null。
        //
        //   System.FormatException:
        //     s 或 format 是空字符串。- 或 -s 不包含与 format 中指定的模式相对应的日期和时间。- 或 -s 中的小时组成部分和 AM/PM
        //     指示符不一致。
        //
        //   System.ArgumentException:
        //     style 包含无效的 System.Globalization.DateTimeStyles 值组合。例如，System.Globalization.DateTimeStyles.AssumeLocal
        //     和 System.Globalization.DateTimeStyles.AssumeUniversal。
        public static DateTime ParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style)
        {
            return DateTime.ParseExact(s, format, provider, style);
        }
        //
        // 摘要:
        //     使用指定的格式数组、区域性特定格式信息和样式，将日期和时间的指定字符串表示形式转换为其 System.DateTime 等效项。字符串表示形式的格式必须至少与指定的格式之一完全匹配，否则会引发异常。
        //
        // 参数:
        //   s:
        //     包含要转换的一个或多个日期和时间的字符串。
        //
        //   formats:
        //     s 的允许格式的数组。
        //
        //   provider:
        //     用于提供有关 s 的区域性特定格式信息的 System.IFormatProvider。
        //
        //   style:
        //     System.Globalization.DateTimeStyles 值的按位组合，指示 s 允许使用的格式。一个要指定的典型值为 System.Globalization.DateTimeStyles.None。
        //
        // 返回结果:
        //     System.DateTime，等效于由 formats、provider 和 style 所指定的 s 中所包含的日期和时间。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     s 或 formats 为 null。
        //
        //   System.FormatException:
        //     s 是空字符串。- 或 -formats 的一个元素是空字符串。- 或 -s 不包含与 formats 中的任何元素对应的日期和时间。- 或 -s
        //     中的小时组成部分和 AM/PM 指示符不一致。
        //
        //   System.ArgumentException:
        //     style 包含无效的 System.Globalization.DateTimeStyles 值组合。例如，System.Globalization.DateTimeStyles.AssumeLocal
        //     和 System.Globalization.DateTimeStyles.AssumeUniversal。
        public static MyDate ParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style)
        {
            return DateTime.ParseExact(s, formats, provider, style);
        }
        //
        // 摘要:
        //     创建新的 System.DateTime 对象，该对象具有与指定的 System.DateTime 相同的刻度数，但是根据指定的 System.DateTimeKind
        //     值的指示，指定为本地时间或协调世界时 (UTC)，或者两者皆否。
        //
        // 参数:
        //   value:
        //     日期和时间。
        //
        //   kind:
        //     枚举值之一，该值指示新对象是表示本地时间、UTC，还是两者皆否。
        //
        // 返回结果:
        //     一个新对象，它具有与 value 参数表示的对象相同的刻度数以及由 kind 参数指定的 System.DateTimeKind 值。
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static MyDate SpecifyKind(MyDate value, DateTimeKind kind)
        {
            return DateTime.SpecifyKind(value.date, kind);
        }
        //
        // 摘要:
        //     从此实例中减去指定的日期和时间。
        //
        // 参数:
        //   value:
        //     System.DateTime 的一个实例。
        //
        // 返回结果:
        //     System.TimeSpan 间隔，它等于此实例所表示的日期和时间减去 value 所表示的日期和时间。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     结果小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public TimeSpan Subtract(MyDate value) { return date.Subtract(value.date); }
        //
        // 摘要:
        //     从此实例中减去指定持续时间。
        //
        // 参数:
        //   value:
        //     System.TimeSpan 的一个实例。
        //
        // 返回结果:
        //     System.DateTime，它等于此实例所表示的日期和时间减去 value 所表示的时间间隔。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     结果小于 System.DateTime.MinValue 或大于 System.DateTime.MaxValue。
        public MyDate Subtract(TimeSpan value) { return date.Subtract(value); }
        //
        // 摘要:
        //     将当前 System.DateTime 对象序列化为一个 64 位二进制值，该值随后可用于重新创建 System.DateTime 对象。
        //
        // 返回结果:
        //     64 位有符号整数，它对 System.DateTime.Kind 和 System.DateTime.Ticks 属性进行了编码。
        public long ToBinary() { return date.ToBinary(); }
        //
        // 摘要:
        //     将当前 System.DateTime 对象的值转换为 Windows 文件时间。
        //
        // 返回结果:
        //     表示为 Windows 文件时间的当前 System.DateTime 对象的值。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     所生成的文件时间将表示协调世界时公元 1601 年 1 月 1 日午夜 12:00 之前的日期和时间。
        public long ToFileTime() { return date.ToFileTime(); }
        //
        // 摘要:
        //     将当前 System.DateTime 对象的值转换为 Windows 文件时间。
        //
        // 返回结果:
        //     表示为 Windows 文件时间的当前 System.DateTime 对象的值。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     所生成的文件时间将表示协调世界时公元 1601 年 1 月 1 日午夜 12:00 之前的日期和时间。
        public long ToFileTimeUtc() { return date.ToFileTimeUtc(); }
        //
        // 摘要:
        //     将当前 System.DateTime 对象的值转换为本地时间。
        //
        // 返回结果:
        //     一个 System.DateTime 对象，其 System.DateTime.Kind 属性为 System.DateTimeKind.Local，并且其值为等效于当前
        //     System.DateTime 对象的值的本地时间；如果经转换的值过大以至于不能由 System.DateTime 对象表示，则为 System.DateTime.MaxValue，或者，如果经转换的值过小以至于不能表示为
        //     System.DateTime 对象，则为 System.DateTime.MinValue。
        public MyDate ToLocalTime() { return date.ToLocalTime(); }
        //
        // 摘要:
        //     将当前 System.DateTime 对象的值转换为其等效的长日期字符串表示形式。
        //
        // 返回结果:
        //     一个字符串，它包含当前 System.DateTime 对象的长日期字符串表示形式。
        [SecuritySafeCritical]
        public string ToLongDateString() { return date.ToLongDateString(); }
        //
        // 摘要:
        //     将当前 System.DateTime 对象的值转换为其等效的长时间字符串表示形式。
        //
        // 返回结果:
        //     一个字符串，它包含当前 System.DateTime 对象的长时间字符串表示形式。
        [SecuritySafeCritical]
        public string ToLongTimeString() { return date.ToLongTimeString(); }
        //
        // 摘要:
        //     将此实例的值转换为等效的 OLE 自动化日期。
        //
        // 返回结果:
        //     一个双精度浮点数，它包含与此实例的值等效的 OLE 自动化日期。
        //
        // 异常:
        //   System.OverflowException:
        //     此实例的值无法表示为 OLE 自动化日期。
        public double ToOADate() { return date.ToOADate(); }
        //
        // 摘要:
        //     将当前 System.DateTime 对象的值转换为其等效的短日期字符串表示形式。
        //
        // 返回结果:
        //     一个字符串，它包含当前 System.DateTime 对象的短日期字符串表示形式。
        [SecuritySafeCritical]
        public string ToShortDateString() { return date.ToShortDateString(); }
        //
        // 摘要:
        //     将当前 System.DateTime 对象的值转换为其等效的短时间字符串表示形式。
        //
        // 返回结果:
        //     一个字符串，它包含当前 System.DateTime 对象的短时间字符串表示形式。
        [SecuritySafeCritical]
        public string ToShortTimeString() { return date.ToShortTimeString(); }

        //
        // 摘要:
        //     使用指定的区域性特定格式信息将当前 System.DateTime 对象的值转换为它的等效字符串表示形式。
        //
        // 参数:
        //   provider:
        //     一个 System.IFormatProvider，它提供区域性特定的格式设置信息。
        //
        // 返回结果:
        //     由 provider 指定的当前 System.DateTime 对象的值的字符串表示形式。
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [SecuritySafeCritical]
        public string ToString(IFormatProvider provider) { return ToString(); }
        //
        // 摘要:
        //     使用指定的格式将当前 System.DateTime 对象的值转换为其等效的字符串表示形式。
        //
        // 参数:
        //   format:
        //     标准或自定义日期和时间格式的字符串。
        //
        // 返回结果:
        //     当前 System.DateTime 对象的值的字符串表示形式，由 format 指定。
        //
        // 异常:
        //   System.FormatException:
        //     format 的长度是 1，并且它不是为 System.Globalization.DateTimeFormatInfo 定义的格式说明符之一。-
        //     或 -format 中不包含有效的自定义格式模式。
        [SecuritySafeCritical]
        public string ToString(string format) { return date.ToString(format); }
        //
        // 摘要:
        //     使用指定的格式和区域性特定格式信息将当前 System.DateTime 对象的值转换为其等效的字符串表示形式。
        //
        // 参数:
        //   format:
        //     标准或自定义日期和时间格式的字符串。
        //
        //   provider:
        //     一个提供区域性特定的格式设置信息的对象。
        //
        // 返回结果:
        //     由 format 和 provider 指定的当前 System.DateTime 对象的值的字符串表示形式。
        //
        // 异常:
        //   System.FormatException:
        //     format 的长度是 1，并且它不是为 System.Globalization.DateTimeFormatInfo 定义的格式说明符之一。-
        //     或 -format 中不包含有效的自定义格式模式。
        /// <summary>
        /// 系统底导调用的ToString函数 。
        /// </summary>
        /// <param name="format"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public string ToString(string format, IFormatProvider provider)
        {
            return date.ToString(format, provider);
        }
        //
        // 摘要:
        //     将当前 System.DateTime 对象的值转换为协调世界时 (UTC)。
        //
        // 返回结果:
        //     一个 System.DateTime 对象，其 System.DateTime.Kind 属性为 System.DateTimeKind.Utc，并且其值为等效于当前
        //     System.DateTime 对象的值的 UTC；如果经转换的值过大以至于不能由 System.DateTime 对象表示，则为 System.DateTime.MaxValue，或者，如果经转换的值过小以至于不能由
        //     System.DateTime 对象表示，则为 System.DateTime.MinValue。
        public MyDate ToUniversalTime() { return date.ToUniversalTime(); }
        //
        // 摘要:
        //     将日期和时间的指定字符串表示形式转换为其 System.DateTime 等效项，并返回一个指示转换是否成功的值。
        //
        // 参数:
        //   s:
        //     包含要转换的日期和时间的字符串。
        //
        //   result:
        //     当此方法返回时，如果转换成功，则包含与 s 中包含的日期和时间等效的 System.DateTime 值；如果转换失败，则为 System.DateTime.MinValue。如果
        //     s 参数为 null，是空字符串 ("") 或者不包含日期和时间的有效字符串表示形式，则转换失败。该参数未经初始化即被传递。
        //
        // 返回结果:
        //     如果 s 参数成功转换，则为 true；否则为 false。
        [SecuritySafeCritical]
        public static bool TryParse(string s, out DateTime result) { return DateTime.TryParse(s, out result); }
        //
        // 摘要:
        //     使用指定的区域性特定格式信息和格式设置样式，将日期和时间的指定字符串表示形式转换为其 System.DateTime 等效项，并返回一个指示转换是否成功的值。
        //
        // 参数:
        //   s:
        //     包含要转换的日期和时间的字符串。
        //
        //   provider:
        //     一个对象，提供有关 s 的区域性特定格式设置信息。
        //
        //   styles:
        //     枚举值的按位组合，该组合定义如何根据当前时区或当前日期解释已分析日期。一个要指定的典型值为 System.Globalization.DateTimeStyles.None。
        //
        //   result:
        //     当此方法返回时，如果转换成功，则包含与 s 中包含的日期和时间等效的 System.DateTime 值；如果转换失败，则为 System.DateTime.MinValue。如果
        //     s 参数为 null，是空字符串 ("") 或者不包含日期和时间的有效字符串表示形式，则转换失败。该参数未经初始化即被传递。
        //
        // 返回结果:
        //     如果 s 参数成功转换，则为 true；否则为 false。
        //
        // 异常:
        //   System.ArgumentException:
        //     styles 不是有效的 System.Globalization.DateTimeStyles 值。- 或 -styles 包含 System.Globalization.DateTimeStyles
        //     值的无效组合（例如，System.Globalization.DateTimeStyles.AssumeLocal 和 System.Globalization.DateTimeStyles.AssumeUniversal）。
        //
        //   System.NotSupportedException:
        //     provider 是一个非特定区域性并且无法在分析操作中使用。
        [SecuritySafeCritical]
        public static bool TryParse(string s, IFormatProvider provider, DateTimeStyles styles, out MyDate result)
        {
            return DateTime.TryParse(s, provider, styles, out result.date);
        }
        //
        // 摘要:
        //     使用指定的格式、区域性特定的格式信息和样式将日期和时间的指定字符串表示形式转换为其等效的 System.DateTime。字符串表示形式的格式必须与指定的格式完全匹配。该方法返回一个指示转换是否成功的值。
        //
        // 参数:
        //   s:
        //     包含要转换的日期和时间的字符串。
        //
        //   format:
        //     所需的 s 格式。
        //
        //   provider:
        //     一个 System.IFormatProvider 对象，提供有关 s 的区域性特定格式设置信息。
        //
        //   style:
        //     一个或多个枚举值的按位组合，指示 s 允许使用的格式。
        //
        //   result:
        //     当此方法返回时，如果转换成功，则包含与 s 中包含的日期和时间等效的 System.DateTime 值；如果转换失败，则为 System.DateTime.MinValue。如果
        //     s 或 format 参数为 null，或者为空字符串，或者未包含对应于 format 中指定的模式的日期和时间，则转换失败。该参数未经初始化即被传递。
        //
        // 返回结果:
        //     如果 s 转换成功，则为 true；否则为 false。
        //
        // 异常:
        //   System.ArgumentException:
        //     styles 不是有效的 System.Globalization.DateTimeStyles 值。- 或 -styles 包含 System.Globalization.DateTimeStyles
        //     值的无效组合（例如，System.Globalization.DateTimeStyles.AssumeLocal 和 System.Globalization.DateTimeStyles.AssumeUniversal）。
        public static bool TryParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style, out MyDate result)
        {
            return DateTime.TryParseExact(s, format, provider, style, out result.date);
        }
        //
        // 摘要:
        //     使用指定的格式数组、区域性特定格式信息和样式，将日期和时间的指定字符串表示形式转换为其等效的 System.DateTime。字符串表示形式的格式必须至少与指定的格式之一完全匹配。该方法返回一个指示转换是否成功的值。
        //
        // 参数:
        //   s:
        //     包含要转换的一个或多个日期和时间的字符串。
        //
        //   formats:
        //     s 的允许格式的数组。
        //
        //   provider:
        //     一个对象，提供有关 s 的区域性特定格式信息。
        //
        //   style:
        //     枚举值的一个按位组合，指示 s 所允许的格式。一个要指定的典型值为 System.Globalization.DateTimeStyles.None。
        //
        //   result:
        //     当此方法返回时，如果转换成功，则包含与 s 中包含的日期和时间等效的 System.DateTime 值；如果转换失败，则为 System.DateTime.MinValue。如果
        //     s 或 formats 为 null，s 或 formats 的一个元素为空字符串，或者 s 的格式与 formats 中的格式模式所指定的格式都不完全匹配，则转换失败。该参数未经初始化即被传递。
        //
        // 返回结果:
        //     如果 s 参数成功转换，则为 true；否则为 false。
        //
        // 异常:
        //   System.ArgumentException:
        //     styles 不是有效的 System.Globalization.DateTimeStyles 值。- 或 -styles 包含 System.Globalization.DateTimeStyles
        //     值的无效组合（例如，System.Globalization.DateTimeStyles.AssumeLocal 和 System.Globalization.DateTimeStyles.AssumeUniversal）。
        public static bool TryParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style, out MyDate result)
        {
            return DateTime.TryParseExact(s, formats, provider, style, out result.date);
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return (date as IConvertible).ToBoolean(provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return (date as IConvertible).ToByte(provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return (date as IConvertible).ToChar(provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return (date as IConvertible).ToDateTime(provider);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return (date as IConvertible).ToDecimal(provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return (date as IConvertible).ToDouble(provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return (date as IConvertible).ToInt16(provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return (date as IConvertible).ToInt32(provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return (date as IConvertible).ToInt64(provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return (date as IConvertible).ToSByte(provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return (date as IConvertible).ToSingle(provider);
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return (date as IConvertible).ToType(conversionType, provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return (date as IConvertible).ToUInt16(provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return (date as IConvertible).ToUInt32(provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return (date as IConvertible).ToUInt64(provider);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            (date as ISerializable).GetObjectData(info, context);
        }
    }
}
