using System;
using System.Collections.Generic;

namespace System
{
    [Flags]
    public enum DateTimePartEnum
    {
        Year = 1,
        Month = 3,
        Day = 7,
        Hour = 15,
        Minute = 31,
        Second = 63,
        MilliSecond = 127,
    }
}
