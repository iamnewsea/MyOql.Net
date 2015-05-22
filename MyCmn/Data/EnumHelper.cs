using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;


namespace MyCmn
{
    /// <summary>
    /// 枚举的辅助类， 在 数据库 列定义， UI 传值 规范， UI 列表列定义规范中经常会使用。
    /// </summary>
    /// <remarks>
    /// 如果要得到 Enum 类型的返回值，应该使用： Enum.Parse(Type,"value") ;其中 value 可以是 数字，也可以是定义字符串。
    /// </remarks>
    public static partial class EnumHelper
    {
        public static Type WhichDefine(string EnumValue, params Type[] FindEnumType)
        {
            if (FindEnumType == null) return null;

            foreach (Type item in FindEnumType)
            {
                if (Enum.IsDefined(item, EnumValue) == true)
                    return item;
            }

            return null;
        }

        public static T[] ToEnumList<T>() where T : IComparable, IFormattable, IConvertible
        {
            List<T> list = new List<T>();
            var ary = Enum.GetValues(typeof(T));
            foreach (var item in ary)
            {
                list.Add((T)item);
            }
            return list.ToArray();
        }

        /// <summary>
        /// 判断枚举是否是多个枚举组合的。（二进制判断是否存在多个1）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsMultiDefine<T>(this T value) where T : IComparable, IFormattable, IConvertible
        {
            var EnumValue = value.AsInt();

            bool? Count1 = null;
            while (EnumValue > 0)
            {
                int yu = EnumValue % 2;
                if (yu > 0)
                {
                    if (Count1.HasValue == false)
                    {
                        Count1 = false;
                        EnumValue = EnumValue >> 1;
                        continue;
                    }

                    if (Count1.HasValue && Count1 == false)
                    {
                        Count1 = true;
                        break;
                    }
                }
                EnumValue = EnumValue >> 1;
            }

            return Count1.HasValue && Count1.Value;
        }

        /// <summary>
        /// 如果仅有一个枚举值，返回该枚举的字符串定义，如果是组合枚举，则返回 数字格式。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string ToShortString<T>(this T value) where T : IComparable, IFormattable, IConvertible
        {
            if (IsMultiDefine<T>(value))
            {
                return value.AsInt().ToString();
            }
            return value.ToString();
        }


        /// <summary>
        /// 枚举减法。 
        /// </summary>
        /// <remarks>枚举的加法使用 |= </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="minus"></param>
        /// <returns></returns>
        public static T MinusEnum<T>(this T value, T minus) where T : IComparable, IFormattable, IConvertible
        {
            var val = value.ToInt32(null);
            return (T)(object)(val ^ (val & minus.ToInt32(null)));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="value">传入null，返回全部，传入 "", 不返回任何对象</param>
        /// <returns></returns>
        public static List<int> ToEnumList(Type enumType, string value = null)
        {
            var ret = new List<int>();

            if (string.IsNullOrEmpty(value) || value == "0")
            {
                ret.Add(DEFAULT_VALUE);// GetDefault(enumType));
                return ret;
            }

            if (value.IsInt())
            {
                return ToEnumList(enumType, value.AsInt());
            }

            GodError.Check(enumType.IsEnum == false, "不明确的枚举类型!");
            var fds = Enum.GetValues(enumType);
            var sects = value.MySplit(',').Select(o => o.Trim()).ToArray();

            foreach (var item in fds)
            {
                var strVal = item.ToString();
                var intVal = Convert.ToInt32(item);

                if (value != null)
                {
                    if (sects.Contains(strVal, StringComparer.CurrentCultureIgnoreCase))
                    {
                        ret.Add(intVal);
                    }
                }
                else
                {
                    ret.Add(intVal);
                }
            }

            return ret;
        }

        /// <summary>
        /// 如果枚举里定义了该Int值，则只返回该枚举，如果  value ==0 , 则返回全部枚举
        /// </summary>
        /// <remarks>
        /// 如果枚举定义：  1,3,4 . value = 7 , 则返回：  3,4 
        /// 如果枚举定义：  1,3,4 . value = 3 , 则返回：  3 
        /// </remarks>
        /// <param name="enumType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<int> ToEnumList(Type enumType, int value)
        {
            GodError.Check(enumType.IsEnum == false, "不能转换的枚举类型:" + enumType.FullName);

            var ret = new List<int>();
            if (value == 0)
            {
                ret.Add(DEFAULT_VALUE);// GetDefault(enumType));// 0;
                return ret;
            }


            var fds = Enum.GetValues(enumType);

            foreach (var item in fds)
            {
                var val = Convert.ToInt32(item);
                if (value != 0)
                {
                    if (val == value)
                    {
                        return new List<int> { val };
                    }
                    else
                    {
                        if (value.Contains(val))
                        {
                            ret.Add(val);
                        }
                    }
                }
            }


            //清洗 返回的数据。 使 枚举间没有包含数据。
            var clone = new int[ret.Count];
            ret.CopyTo(clone);
            var removeItem = new List<int>();

            for (int i = 0; i < ret.Count; i++)
            {
                var val = ret[i];
                foreach (var item in clone)
                {
                    if (val <= item) continue;

                    if (val.Contains(item))
                    {
                        removeItem.Add(item);
                    }
                }
            }

            ret = ret.Minus(removeItem).ToList();
            return ret;
        }

        /// <summary>
        /// 得到枚举的可能单个枚举值列表.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TheUnionEnum"></param>
        /// <returns></returns>
        public static T[] GetEnumList<T>(this T TheUnionEnum) where T : IComparable, IFormattable, IConvertible
        {
            var type = typeof(T);
            return ToEnumList(type, TheUnionEnum.AsInt()).Select(o => (T)(object)o).ToArray();
        }

        /// <summary>
        /// 按位域得到各个Enum的值。
        /// </summary>
        /// <remarks>
        /// 关于位域，请参考：http://127.0.0.1:47873/help/1-5452/ms.help?method=page&amp;id=M%3aSYSTEM.FLAGSATTRIBUTE.%23CTOR&amp;topicversion=100&amp;topiclocale=ZH-CN&amp;SQM=1&amp;product=VS&amp;productVersion=100&amp;locale=ZH-CN
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="EnumValue"></param>
        /// <returns></returns>
        public static string GetEnumString(this Enum EnumValue)
        {
            return string.Join(",", ToEnumList(EnumValue.GetType(), EnumValue.AsInt()).Select(o => o.ToString()).ToArray());
        }

        /// <summary>
        /// 得到枚举的可能单个枚举值的Int值列表. 算法和 枚举没有关系.
        /// </summary>
        /// <param name="EnumValue"></param>
        /// <returns></returns>
        public static int[] GetEachDefine(int EnumValue)
        {
            var list = new List<int>();
            EnumValue = EnumValue & int.MaxValue;
            int step = 0;
            while (EnumValue > 0)
            {
                int yu = EnumValue % 2;
                if (yu > 0)
                {
                    list.Add(1 << step);
                }
                EnumValue = EnumValue >> 1;
                step++;
            }
            return list.ToArray();
        }

        /// <summary>
        /// 判断是否包含某值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TheContainer"></param>
        /// <param name="TheOne"></param>
        /// <returns>不判断是否标记Flag,单纯的按二进制进行匹配.</returns>
        public static bool Contains<T>(this T TheContainer, T TheOne) where T : IComparable, IFormattable, IConvertible
        {
            int con = ValueProc.AsInt(TheContainer);
            int one = ValueProc.AsInt(TheOne);
            if (one == 0) return false;
            if (con == one) return true;
            if ((con & one) == one) return true;
            return false;
        }

        /// <summary>
        /// 把一个类型的枚举值转换为另一个类型的枚举值.多个返回值用 逻辑或 表示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="EnumValue"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this int EnumValue)
            where T : IComparable, IFormattable, IConvertible
        {
            return (T)(object)(EnumValue);
        }


        public static T ToEnum<T>(this string EnumValue, T defaultValue) where T : IComparable, IFormattable, IConvertible, new()
        {
            var enumType = typeof(T);
            GodError.Check(enumType.IsEnum == false, "不明确的枚举类型!");
            var fds = ToEnumList(enumType, EnumValue).ToList();// ,  enumType);

            if (fds.Count == 0) return defaultValue;

            var ret = 0;
            foreach (var item in fds)
            {
                ret |= item.AsInt();
            }
            return (T)(object)ret;
        }

        /// <summary>
        /// 把指定的字符串形式的枚举值转换为枚举.如果字符串表示多个枚举,用","分隔, 多个返回值用 逻辑或 表示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="EnumString"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string EnumString) where T : IComparable, IFormattable, IConvertible
        {
            if (string.IsNullOrEmpty(EnumString) || EnumString == "0") return (T)(object)DEFAULT_VALUE;// GetDefault(typeof(T));


            if (EnumString.IsInt()) { return ToEnum<T>(EnumString.AsInt()); }

            return (T)(object)ToEnum(EnumString, typeof(T));
        }

        /// <summary>
        /// 把指定的字符串形式的枚举值转换为枚举.如果字符串表示多个枚举,用","分隔, 多个返回值用 逻辑或 表示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="EnumString"></param>
        /// <returns></returns>
        public static int ToEnum(this string EnumString, Type type)
        {
            if (string.IsNullOrEmpty(EnumString) || EnumString == "0") return DEFAULT_VALUE;// GetDefault(type).AsInt();

            if (EnumString.IsInt())
            {
                return EnumString.AsInt();
            }

            var ret = 0;
            ToEnumList(type, EnumString).All(o =>
            {
                ret |= Convert.ToInt32(o);
                return true;
            });

            return ret;
        }

        /// <summary>
        /// 枚举的默认值，为0
        /// </summary>
        public const int DEFAULT_VALUE = 0;

        ///// <summary>
        ///// 取默认值，当是枚举的时候，取空值。优先选择： 0,None,Default,-1,-2147483648
        ///// </summary>
        ///// <param name="enumType"></param>
        ///// <returns></returns>
        //public static int GetDefault(Type enumType)
        //{
        //    return 0;
        //    //if (enumType.IsNullableType())
        //    //{
        //    //    enumType = Nullable.GetUnderlyingType(enumType);
        //    //}

        //    //if (enumType.IsEnum == false)
        //    //{
        //    //    return ValueProc.GetDefaultValue(enumType).AsInt();
        //    //}

        //    //if (Enum.IsDefined(enumType, 0) == false)
        //    //{
        //    //    return 0;
        //    //}
        //    //else
        //    //{
        //    //    //查找 None,Default
        //    //    if (Enum.IsDefined(enumType, "None")) { return Convert.ToInt32(Enum.Parse(enumType, "None")); }
        //    //    else if (Enum.IsDefined(enumType, "Default")) { return Convert.ToInt32(Enum.Parse(enumType, "Default")); }
        //    //    else if (Enum.IsDefined(enumType, -1) == false) { return -1; }
        //    //    else if (Enum.IsDefined(enumType, int.MinValue) == false) { return int.MinValue; }
        //    //}
        //    //throw new GodError("找不到枚举：" + enumType.FullName + " 的默认值，已搜索：[0,None,Default,-1,-2147483648] 值。");
        //}



        public static string GetDescription<T>(this T myEnum) where T : IComparable, IFormattable, IConvertible
        {
            var type = typeof(T);
            if (type.IsEnum == false)
            {
                type = myEnum.GetType();
            }
            return GetDescription(type, myEnum.AsInt());
        }

        /// <summary>
        /// 通过反射 ，查找 System.ComponentModel.Description 属性找出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="AnyDescr"></param>
        /// <returns></returns>
        public static T GetEnumFromDescription<T>(string AnyDescr) where T : IComparable, IFormattable, IConvertible
        {
            //  MyDescAttribute retVal = new MyDescAttribute();
            FieldInfo[] fis = typeof(T).GetFields();
            if (fis.Length == 0)
            {
                return default(T);
            }
            foreach (FieldInfo item in fis)
            {
                if (item.GetType().FullName == "System.Reflection.RtFieldInfo") continue;
                var arrs = item.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (arrs.Length == 0) continue;
                DescriptionAttribute desc = arrs[0] as DescriptionAttribute;
                if (string.Equals(desc.Description.AsString(), AnyDescr, StringComparison.CurrentCultureIgnoreCase)) return item.Name.ToEnum<T>();
            }
            return default(T);
        }

        /// <summary>
        /// 按Short 来.
        /// </summary>
        /// <param name="EnumDefineType"></param>
        /// <param name="myEnumValue"></param>
        /// <returns></returns>
        public static string GetDescription(Type EnumDefineType, int myEnumValue)
        {
            List<string> retVal = new List<string>();
            List<int> val = EnumHelper.ToEnumList(EnumDefineType, myEnumValue).Select(o => Convert.ToInt32(o)).ToList();
            Array oriData = Enum.GetValues(EnumDefineType);

            if (val.Count > 1)
            {
                val.Remove(0);
            }

            foreach (int eachVal in val)
            {
                foreach (var item in oriData)
                {
                    if (item.AsInt() == eachVal)
                    {
                        Object[] objList = EnumDefineType.GetField(item.AsString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (objList.Length == 0)
                        {
                            retVal.Add(item.AsString());
                        }
                        else
                        {
                            retVal.Add(((DescriptionAttribute)objList[0]).Description);
                        }
                        break;
                    }
                }
            }

            return string.Join(",", retVal.ToArray());
        }

    }
}
