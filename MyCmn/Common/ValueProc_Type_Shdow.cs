using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MyCmn
{
    public static partial class ValueProc
    {
        public static T AsType<T>(this object value)
        {
            return As<T>(value);
        }

        public static T As<T>(object value)
        {
            var type = typeof(T);
            var retVal = AsType(type, value);
            if (retVal == null && type.IsEnum) return (T)(object)EnumHelper.DEFAULT_VALUE;//.GetDefault(type);
            return (T)retVal;
        }

        /// <summary>
        /// 类型转换并装箱,不处理错误.
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static T As<T>(object value, T defaultValue)
        {
            var retVal = AsType(typeof(T), value);
            if (retVal == null) return defaultValue;
            return (T)retVal;
        }

        /// <summary>
        /// 得到 对象 的 布尔类型的值。
        /// </summary>
        /// <param name="Value">要转换的值</param>
        /// <returns>如果对象的值可正确返回， 返回对象转换的值 ，否则， 返回默认值 。</returns>
        public static bool AsBool(this object Value)
        {
            return AsBool(Value, false);
        }


        /// <summary>
        /// 得到对象的 Int 类型的值 。
        /// </summary>
        /// <param name="Value">要转换的值</param>
        /// <returns>如果对象的值可正确返回， 返回对象转换的值 ，否则， 返回默认值 。</returns>
        public static int AsInt(this object Value)
        {
            return AsInt(Value, 0);
        }

        public static uint AsUInt(this object Value)
        {
            return AsUInt(Value, 0, false);
        }

        public static uint AsUInt(this object Value, uint defaultValue)
        {
            return AsUInt(Value, defaultValue, false);
        }

        /// <summary>
        /// 得到对象的 String 类型的值
        /// </summary>
        /// <param name="Value">要转换的值</param>
        /// <returns>如果对象的值可正确返回， 返回对象转换的值 ，否则， 返回默认值 。</returns>
        public static string AsString(this object Value)
        {
            return AsString(Value, string.Empty);
        }

        /// <summary>
        /// 得到对象的 Decimal 类型的值
        /// </summary>
        /// <param name="Value">要转换的值</param>
        /// <returns>如果对象的值可正确返回， 返回对象转换的值 ，否则， 返回默认值 。</returns>
        public static decimal AsDecimal(this object Value)
        {
            return AsDecimal(Value, 0);
        }

        /// <summary>
        /// 得到对象的 Float 类型的值
        /// </summary>
        /// <param name="Value">要转换的值</param>
        /// <returns>如果对象的值可正确返回， 返回对象转换的值 ，否则， 返回默认值 。</returns>
        public static float AsFloat(this object Value)
        {
            return AsFloat(Value, 0f);
        }

        public static double AsDouble(this object Value)
        {
            return AsDouble(Value, 0d);
        }

        /// <summary>
        /// 得到对象的 Long 类型的值
        /// </summary>
        /// <param name="Value">要转换的值</param>
        /// <returns>如果对象的值可正确返回， 返回对象转换的值 ，否则， 返回默认值 。</returns>
        public static long AsLong(this object Value)
        {
            return AsLong(Value, 0, false);
        }

        public static long AsLong(this object Value, long defaultValue)
        {
            return AsLong(Value, defaultValue, false);
        }

        /// <summary>
        /// 得到对象的 Int 类型的值,如果是 Float 和 Double , 则四舍五入返回.
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int AsInt(this object Value, int defaultValue)
        {
            return AsInt(Value, defaultValue, false);
        }

        /// <summary>
        /// 得到对象的 DateTime 类型的值
        /// </summary>
        /// <param name="Value">要转换的值</param>
        /// <returns>如果对象的值可正确返回， 返回对象转换的值 ，否则， 返回默认值 。</returns>
        public static DateTime AsDateTime(this object Value)
        {
            return AsDateTime(Value, DateTime.MinValue);
        }

        public static MyDate AsMyDate(this object Value)
        {
            return new MyDate(AsDateTime(Value, MyDate.MinValue));
        }
        public static MyDate AsMyDate(this object Value, MyDate defaultValue)
        {
            return new MyDate(AsDateTime(Value, defaultValue));
        }
        public static short AsShortInt(this object Value)
        {
            return AsShortInt(Value, 0, false);
        }

        public static short AsShortInt(this object Value, short defaultValue)
        {
            return AsShortInt(Value, defaultValue, false);
        }
    }
}