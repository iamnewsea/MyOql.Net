using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Collections;
using System.Reflection;

namespace MyCmn
{
    /// <summary>
    /// 一些类型转换的函数。
    /// </summary>
    /// <remarks>
    /// <pre style='line-height:30px;font-size:14px;font-family: 微软雅黑,宋体'>
    /// 最基本的操作是类型转换.它们大部分是扩展方法,方便使用.
    /// 比如方法调用形式
    ///     A.B().C().D().E(); 的可读性要比:  E(D(C(A.B()))) ;  可读性好.
    ///     估且把第一种称为 链式调用. 把第二种称为 层级调用.
    ///     该类的很多方法是把层级调用转换为链式调用.
    /// 它们是在 .net framework 之上再次封装,在实际业务中,用于替换 .net framework 的方法.
    /// <b style="color:red">应用实例</b>
    /// 1. AsString 是 ToString 的替代方案 
    ///     32.AsString();         //等同于 32.ToString();
    ///     null.AsString();       //返回 null;
    ///     new char[]{'h','e','l','l','o'}.AsString();    //返回 "hello"
    ///     对于枚举, 推荐使用 GetEnumString
    /// 2. 同类方法包括:
    ///     GetInt 是  Convert.ToInt 和 int.Parse 的替代方案
    ///     GetBool
    ///     GetDateTime 
    ///     GetDecimal 
    ///     GetFloat 
    ///     GetGuid 
    ///     GetLong 
    ///     GetUInt 
    /// 3. Split 是 string.Split 的扩展版本.由于没有默认的按连续字符串分隔.
    ///     "hello&amp;nbsp;world".Split("&amp;nbsp;") ;    //返回 ["hello","word"]
    /// 4. TakeOutInt 实现类似 Javascript 的 parseInt 方法
    ///     "width:12px".TakeOutInt();              // 返回 12
    /// 5. HasValue 判断对象是否有值. 是  string.IsEmpty 的替换方案.
    /// 6. GetSub 是 IEnumerable&lt;TSource&gt;.Where((value,index,retval)=> return index &gt;= startIndex &amp;&amp; index &lt;=endIndex) 的替代方案
    /// 7. GetOrDefault 是取出字典值.当字典不存在时,返回默认值.而不是报错.(直接取字典值报错.)
    ///     字典是高效的,易于使用的数据结构,它是 Hashtable 的替代方案. Hashtable 需要装箱,拆箱.
    /// 8. IsIn 判断是否存在于集合中.
    ///     3.IsIn( new int[]{1,2,3,4} ) ;  等效于 new int[]{1,2,3,4}.Contains(3);
    /// 9. TrimWithPair 结队去除.
    ///     "&lt;a&gt;hello&lt;/a&gt;".TrimWithPair("&lt;a&gt;","&lt;/a&gt;") ;     //返回 hello.
    /// 10. SplitSect , SplitLine ,SplitCell 暗文,须要保证文本里都是可见字符.
    ///     当进行转义性替换时,$表示变量时,具有特殊的意义,就要用两个 $$ 表示一个 $ : 
    ///     "you cost $$:$money$"
    ///         .Replace("$$",ValueProc.SplitSect.AsString())
    ///         .FindNextNode(o=&gt;o.Replace("money", "123"))
    ///         .Replace(ValueProc.SplitSect.AsString(),"$$");
    ///         
    ///     //返回 you cost $:123
    ///<hr></hr>
    /// </pre>
    /// </remarks>
    public static partial class ValueProc
    {
        public static bool AssertTrue(this bool TrueCondition, string ErrorMsg)
        {
            if (TrueCondition) return TrueCondition;

            throw new GodError(ErrorMsg);

            return TrueCondition;
        }

        public static R Switch<T, R>(this T obj, params  Func<T, R>[] funcs)
            where R : class
        {
            if (funcs == null) throw new GodError("参数不能为空");

            R ret = null;
            foreach (var item in funcs)
            {
                ret = item(obj);
                if (ret != null)
                {
                    return ret;
                }
            }

            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">传递具有EventName的类</typeparam>
        /// <param name="obj"></param>
        /// <param name="EventName"></param>
        public static void ClearAllEvent(object obj, string EventName)
        {
            var typeObj = obj.GetType();
            var typeT = typeObj;
            FieldInfo field = null;
            while (true)
            {
                if (typeT == typeof(object)) break;

                field = typeT.GetField(EventName,
                      BindingFlags.NonPublic |
                      BindingFlags.Instance |
                      BindingFlags.GetField);

                if (field != null)
                {
                    break;
                }

                typeT = typeT.BaseType;
            }


            if (field == null)
            {
                return;
            }

            var value = field.GetValue(obj);
            if (value == null) return;

            var ary = ((Delegate)value).GetInvocationList();

            foreach (var item in ary)
            {
                typeT.GetEvent("DecrypteEvent").RemoveEventHandler(obj, item);
            }
        }


        public class MySplitWholeExpressionDefine
        {
            public bool RetainEmpty { get; set; }

            public char SplitChar = ',';
            public string SplitEscapeDefine = ",,";

            public char PrefixDefine = '"';
            public string PrefixEscapeDefine = @"""";

            public char SuffixDefine = '"';
            public string SuffixEscapeDefine = @"""";
        }
        /// <summary>
        /// 自定义分隔
        /// </summary>
        /// <param name="Value">要分隔的文本</param>
        /// <param name="Split">分隔符</param>
        /// <param name="WholeExpression">完整表达式，Key表示完整标识符，如：双引号 , Value表示该标识符的转义字符，如 两个 "" </param>
        /// <returns></returns>
        public static string[] MySplit(this string Value, params char[] Splits)
        {
            if (string.IsNullOrEmpty(Value)) return new string[0];
            return Value.Split(Splits, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 自定义分隔
        /// </summary>
        /// <param name="Value">要分隔的文本</param>
        /// <param name="Split">分隔符</param>
        /// <param name="WholeExpression">完整表达式，Key表示完整标识符，如：双引号 , Value表示该标识符的转义字符，如 两个 "" </param>
        /// <returns></returns>
        public static string[] MySplit(this string Value, MySplitWholeExpressionDefine WholeExpression)
        {
            if (WholeExpression == null)
            {
                return MySplit(Value, ',');
            }

            //.Replace(WholeExpression.PrefixEscapeDefine, ValueProc.SplitCell.ToString())
            //.Replace(WholeExpression.SuffixEscapeDefine, ValueProc.SplitLine.ToString());

            var list = new List<string>();
            for (int i = 0; i < Value.Length; i++)
            {
                var item = Value[i];
                string one = string.Empty;

                var index = GetNextIndex(Value, i, WholeExpression);
                if (index < 0)
                {
                    one = Value.Slice(i);

                    if (WholeExpression.RetainEmpty || (one.Length > 0))
                    {
                        list.Add(one);
                    }
                    break;
                }

                one = Value.Slice(i, index);

                if (WholeExpression.RetainEmpty || (one.Length > 0))
                {
                    list.Add(one);
                }

                i = index;

                if (WholeExpression.RetainEmpty && index == Value.Length - 1)
                {
                    list.Add(string.Empty);
                    break;
                }
            }


            return list.ToArray();
        }

        /// <summary>
        /// 查找下一个字符。
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="startIndex"></param>
        /// <param name="Split"></param>
        /// <param name="WholeExpression"></param>
        /// <returns>返回查找到的Index</returns>
        private static int GetNextIndex(string Value, int startIndex, MySplitWholeExpressionDefine WholeExpression)
        {
            for (int i = startIndex; i < Value.Length; i++)
            {
                if (Value.Slice(i, i + WholeExpression.PrefixEscapeDefine.Length) == WholeExpression.PrefixEscapeDefine)
                {
                    i += WholeExpression.PrefixEscapeDefine.Length;
                    continue;
                }

                if (Value.Slice(i, i + WholeExpression.SuffixEscapeDefine.Length) == WholeExpression.SuffixEscapeDefine)
                {
                    i += WholeExpression.PrefixEscapeDefine.Length;
                    continue;
                }

                if (Value.Slice(i, i + WholeExpression.SplitEscapeDefine.Length) == WholeExpression.SplitEscapeDefine)
                {
                    i += WholeExpression.PrefixEscapeDefine.Length;
                    continue;
                }

                if (Value[i] == WholeExpression.SplitChar)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 工作时间的秒数 = 8小时
        /// </summary>
        public const int WorkTimeSecond = 28800;
        public const int HarfDaySecond = 43200;
        public const int DaySecond = 86400;

        /// <summary>
        /// 获取 数值在二进制最大位上的数量
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static int GetBitPositionValue(this int Value)
        {
            var pow = 0;
            while (Value > 0)
            {
                Value = Value >> 1;
                pow++;
            }
            if (pow == 0) return 0;
            return Convert.ToInt32(Math.Pow(2, pow - 1));
        }

        /// <summary>
        /// 按 Byte 方式 进行截断
        /// </summary>
        /// <param name="v"></param>
        /// <param name="byteLen"></param>
        /// <returns></returns>
        private static byte[] GetStringByUtf8(string v, int byteLen)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(v);
            if (byteLen >= data.Length) return data;

            //http://www.cnblogs.com/yayagamer/archive/2010/08/15/1800250.html
            var ret = new List<byte>();
            for (int i = 0; i < byteLen; i++)
            {
                if (PositionHasValue(data[i], 7) == false)
                {
                    ret.Add(data[i]);
                    continue;
                }

                //下面都是第8位为1的。
                //7位是0的，表示连接Byte
                if (PositionHasValue(data[i], 6) == false)
                {
                    throw new Exception("");
                }

                //开头110
                if (PositionHasValue(data[i], 5) == false)
                {
                    if (i + 1 >= byteLen) break;

                    ret.Add(data[i]);
                    ret.Add(data[i + 1]);
                    i += 1;
                    continue;
                }

                //开头1110
                if (PositionHasValue(data[i], 4) == false)
                {
                    if (i + 2 >= byteLen) break;

                    ret.Add(data[i]);
                    ret.Add(data[i + 1]);
                    ret.Add(data[i + 2]);
                    i += 2;
                    continue;
                }
                //开头11110
                if (PositionHasValue(data[i], 3) == false)
                {
                    if (i + 3 >= byteLen) break;

                    ret.Add(data[i]);
                    ret.Add(data[i + 1]);
                    ret.Add(data[i + 2]);
                    ret.Add(data[i + 3]);
                    i += 3;
                    continue;
                }
                //开头111110
                if (PositionHasValue(data[i], 2) == false)
                {
                    if (i + 4 >= byteLen) break;

                    ret.Add(data[i]);
                    ret.Add(data[i + 1]);
                    ret.Add(data[i + 2]);
                    ret.Add(data[i + 3]);
                    ret.Add(data[i + 4]);
                    i += 4;
                    continue;
                }
            }

            return ret.ToArray();
        }

        /// <summary>
        /// 指定位上是否是1
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="LeftPositionIndex"></param>
        /// <returns></returns>
        public static bool PositionHasValue(this int Value, int LeftPositionIndex)
        {
            return (Value & (1 << LeftPositionIndex)) != 0;
        }


        /// <summary>
        /// 修补长度
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Source"></param>
        /// <param name="Length"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static List<T> Patch2Length<T>(this IEnumerable<T> Source, int Length, T DefaultValue = default(T))
        {
            List<T> list = null;
            if (Source == null)
            {
                list = new List<T>();
            }
            else
            {
                list = Source.ToList();
            }

            if (list.Count >= Length) return list;

            for (int i = list.Count; i < Length; i++)
            {
                list.Add(DefaultValue);
            }
            return list;
        }

        public static DateTime Set(this DateTime Dt, DateTimePartEnum Part, int Value)
        {
            switch (Part)
            {
                case DateTimePartEnum.Year:
                    return new DateTime(Value, Dt.Month, Dt.Day, Dt.Hour, Dt.Minute, Dt.Second, Dt.Millisecond);
                case DateTimePartEnum.Month:
                    return new DateTime(Dt.Year, Value, Dt.Day, Dt.Hour, Dt.Minute, Dt.Second, Dt.Millisecond);
                case DateTimePartEnum.Day:
                    return new DateTime(Dt.Year, Dt.Month, Value, Dt.Hour, Dt.Minute, Dt.Second, Dt.Millisecond);
                case DateTimePartEnum.Hour:
                    return new DateTime(Dt.Year, Dt.Month, Dt.Day, Value, Dt.Minute, Dt.Second, Dt.Millisecond);
                case DateTimePartEnum.Minute:
                    return new DateTime(Dt.Year, Dt.Month, Dt.Day, Dt.Hour, Value, Dt.Second, Dt.Millisecond);
                case DateTimePartEnum.Second:
                    return new DateTime(Dt.Year, Dt.Month, Dt.Day, Dt.Hour, Dt.Minute, Value, Dt.Millisecond);
                case DateTimePartEnum.MilliSecond:
                    return new DateTime(Dt.Year, Dt.Month, Dt.Day, Dt.Hour, Dt.Minute, Dt.Second, Value);
                default:
                    break;
            }
            return Dt;
        }


        /// <summary>
        /// 用它和 default(T) 进行比较，如果等于默认值，则返回false, 否则:
        /// 
        /// 1. 时间最小值 ,返回 false
        /// 2. float 非法值 最小值 ,返回 false
        /// 3. dbouble 非法值 最小值,返回false
        /// 4. decimal 最小值, 返回 false
        /// </summary>
        /// <typeparam name="T">检测对象类型</typeparam>
        /// <typeparam name="R">当有值时，回调返回值类型</typeparam>
        /// <param name="Value"></param>
        /// <param name="HasValueFunc">当有值时，执行的回调</param>
        /// <returns></returns>
        public static R HasValue<T, R>(this T Value, Func<T, R> HasValueFunc)
        {
            if (HasValue(Value))
            {
                if (HasValueFunc != null) return HasValueFunc(Value);
            }

            return default(R);
        }

        public static R HasValue<T, R>(this T Value, Func<T, R> HasValueFunc, Func<R> NoValueFunc)
        {
            if (HasValue(Value))
            {
                if (HasValueFunc != null) return HasValueFunc(Value);
            }
            else
            {
                if (NoValueFunc != null) return NoValueFunc();
            }

            return default(R);
        }

        /// <summary>
        /// 判断一个对象是否为 null 或 DBNull
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static bool IsDBNull<T>(this T Value)
        {
            if (object.Equals(Value, null)) return true;
            else
            {
                return object.Equals(Value, DBNull.Value);
            }
        }

        public static bool HasData(this ICollection collection)
        {
            if (object.Equals(collection, null)) return false;
            return collection.Count > 0;
        }

        public static bool HasData(this IListSource source)
        {
            if (source == null) return false;
            var list = source.GetList();
            if (list == null) return false;
            return list.HasData();
        }

        /// <summary>
        /// 包含等于的值。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Between<T>(this T value, T startValue, T endValue) where T : IComparable
        {
            if (Equals(value, null)) return false;
            if (Equals(startValue, null)) return false;
            if (Equals(endValue, null)) return false;

            if (value.CompareTo(startValue) < 0) return false;
            if (value.CompareTo(endValue) > 0) return false;
            return true;
        }

        /// <summary>
        ///	去除开头的字符串。 
        /// </summary>
        /// <param name="value">
        /// </param>
        /// <param name="removeStart">
        /// </param>
        /// <returns>
        /// </returns>
        public static string TrimStart(this string value, string removeStart)
        {
            if (value.StartsWith(removeStart))
            {
                value = value.Substring(removeStart.Length);

                if (value.StartsWith(removeStart))
                {
                    value = value.TrimStart(removeStart);
                }
            }

            return value;
        }

        public static string Remove(this string value, string RemoveValue)
        {
            if (RemoveValue == null) return value;
            if (value == null) return value;
            return value.Replace(RemoveValue, "");
        }

        /// <summary>
        /// 去除结尾的字符串
        /// </summary>
        /// <param name="value">
        /// </param>
        /// <param name="removeEnd">
        /// </param>
        /// <returns>
        /// </returns>
        public static string TrimEnd(this string value, string removeEnd)
        {
            if (value.EndsWith(removeEnd))
            {
                value = value.Remove(value.Length - removeEnd.Length);

                if (value.EndsWith(removeEnd))
                {
                    value = value.TrimEnd(removeEnd);
                }
            }

            return value;
        }

        public static string TrimWithPair(this string value, string removeStart, string removeEnd)
        {
            return TrimWithPair(value, removeStart, removeEnd, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// 结队去除，比如去除 () {} [] , 或 &lt;a>t&lt;/a> 等
        /// </summary>
        /// <param name="value">
        /// </param>
        /// <param name="removeStart">
        /// </param>
        /// <param name="removeEnd">
        /// </param>
        /// <param name="compare"></param>
        /// <returns>
        /// </returns>
        public static string TrimWithPair(this string value, string removeStart, string removeEnd,
                                          StringComparison compare)
        {
            if (value.HasValue() == false || removeStart.HasValue() == false || removeEnd.HasValue() == false)
                return value;

            value = value.Trim();
            removeStart = removeStart.Trim();
            removeEnd = removeEnd.Trim();

            if (value.StartsWith(removeStart, compare) && value.EndsWith(removeEnd, compare))
            {
                value = value.Substring(removeStart.Length);
                value = value.Remove(value.Length - removeEnd.Length);

                if (value.StartsWith(removeStart) && value.EndsWith(removeEnd))
                {
                    value = TrimWithPair(value, removeStart, removeEnd, compare);
                }
            }

            return value.Trim();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T GetValueOrDefault<T>(this T? value) where T : struct
        {
            if (value.HasValue) return value.Value;
            else return default(T);
        }

        /// <summary>
        /// 替换 内容有: 单引号, 双引号, 回车, -- , 大于号, 小于号,反斜线[★]
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string GetSafeValue(this string msg)
        {
            if (string.IsNullOrEmpty(msg))
                return msg;
            return
                msg.Replace("'", "＇").Replace("\"", "＂").Replace(Environment.NewLine, " ").Replace("--", "－－").Replace(
                    "\r", "　").Replace("\n", " ").Replace("<", "＜").Replace(">", "＞").Replace("\\", "＼").Trim();
        }

        //public static T GetDefault<T>()
        //{
        //    int
        //}

        /// <summary>
        /// 如果没能取出Key ， 则返回 default()， 不会报错。  
        /// </summary>
        /// <remarks>
        /// <pre>
        /// GetOrDefault 是取出字典值.当字典不存在时,返回默认值.而不是报错.(直接取字典值报错.)
        ///     字典是高效的,易于使用的数据结构,它是 Hashtable 的替代方案. Hashtable 需要装箱,拆箱.
        /// </pre>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="theDict"></param>
        /// <param name="one"></param>
        /// <returns></returns>
        public static R GetOrDefault<T, R>(this IDictionary<T, R> theDict, T one)
        {
            if (theDict == null) return default(R);
            if (theDict.ContainsKey(one) == false)
                return default(R);
            return theDict[one];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="theDict"></param>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <returns></returns>
        public static R GetOrDefault<T, R>(this IDictionary<T, R> theDict, Func<T, bool> func)
        {
            if (theDict == null) return default(R);
            object retVal = null;
            if (theDict.Any(o =>
                                {
                                    if (func(o.Key))
                                    {
                                        retVal = o.Value;
                                        return true;
                                    }
                                    else return false;
                                }))
            {
                return (R)retVal;
            }
            else
                return default(R);
        }


        /// <summary>
        /// 不区分大小写的比较两个字符.
        /// </summary>
        /// <param name="charValue"> </param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool EqualsNoMatter(this char charValue, char other)
        {
            if (charValue.Equals(other)) return true;
            if (char.IsLetter(charValue) && char.IsLetter(other))
            {
                return char.ToLower(charValue).Equals(char.ToLower(other));
            }
            else return false;
        }

        /// <summary>
        /// 根据种子得到某个索引的值.
        /// </summary>
        /// <remarks>
        /// <code>
        /// 把 十进制 100 , 转为 16 进制.
        /// var num100 = GetSequence("123456789abcdef", 100).PadLeft(8, 'a')
        /// </code>
        /// </remarks>
        /// <param name="seed">种子</param>
        /// <param name="index">索引</param>
        /// <returns></returns>
        public static string GetSequence(string seed, int index)
        {
            GodError.Check(index < 0, "索引值(" + index + ")不能小于0");

            int Length = seed.Length;

            var list = new List<char>();

            int De = index;
            int Yu = 0;
            do
            {
                Yu = De % Length;
                De = De / Length;

                list.Add(seed[Yu]);
            } while (De != 0);

            list.Reverse();
            return new string(list.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetSequenceIndex(string seed, string value)
        {
            GodError.Check(value.HasValue() == false, "Value不能为空.");
            int length = seed.Length;

            var list = new List<int>();
            int retVal = 0;

            int pos = 0;
            value.Reverse().All(o =>
                                    {
                                        int index = seed.IndexOf(o);
                                        GodError.Check(index < 0, "Value的值: " + o + ",必须全部出现在Seed中.");
                                        list.Add(index);

                                        retVal += index * Math.Pow(length, pos).AsInt();
                                        pos++;
                                        return true;
                                    });

            return retVal;
        }

        /// <summary>
        /// 得到 Excel 的列名.
        /// </summary>
        /// <param name="Seed"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public static string GetExcelSequence(string Seed, int Index)
        {
            GodError.Check(Index < 0, "索引值(" + Index + ")不能小于0");
            var chars = new List<char>();
            int Length = Seed.Length;
            do
            {
                if (chars.Count > 0) Index--;
                chars.Insert(0, Seed[Index % Length]);
                Index = (Index - Index % Length) / Length;
            } while (Index > 0);

            return chars.AsString();
        }

        public static int GetExcelSequenceIndex(string Seed, string Value)
        {
            GodError.Check(Value.HasValue() == false, "Value不能为空.");

            int Length = Seed.Length;
            double count = -1;

            for (int i = 0; i < Value.Length; i++)
            {
                count += (Seed.IndexOf(Value[i]) + 1) * Math.Pow(Length, Value.Length - 1 - i);
            }

            return count.AsInt();
        }


        //public static string Format<T>(this string Source, T JsonObject)
        //    where T : class
        //{
        //    if (JsonObject == default(T)) return Source;

        //    Type type = typeof(T);
        //    Dictionary<string, string> dict = new Dictionary<string, string>();
        //    type.GetProperties().All(o =>
        //        {
        //            dict[o.Name] = o.GetValue(JsonObject, null).AsString();
        //            return true;
        //        });
        //    return Format(Source, '{', '}', dict);
        //}


        /// <summary>
        /// Dict 连写方式 : new StringDict { { "k1","v1" },{"k2","v2"} } .
        /// </summary>
        /// <example>
        /// <code>
        /// "内容如下\n {Name1}:{Value1}, {Name2}:{Value2}".Format(new StringDict { { "Name1", "Value1" }, { "Name2", "Value2" } })
        /// </code>
        /// </example>
        /// <param name="Source"></param>
        /// <param name="JsonObject"></param>
        /// <returns></returns>
        public static string FormatEx(this string Source, StringDict JsonObject)
        {
            return FormatEx(Source, "{}", JsonObject);
        }

        public static string FormatEx(this StringLinker Source, StringDict JsonObject)
        {
            return FormatEx(Source.AsString(), JsonObject);
        }


        /// <summary>
        /// 增强型Format.避免用数字索引进行格式化.
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="LeftMark"></param>
        /// <param name="RightMar"></param>
        /// <param name="Dict"></param>
        /// <returns></returns>
        public static string FormatEx(this string Source, string Marks, StringDict Dict)
        {
            //Source = Source.Replace("{{", SplitSect.ToString()).Replace("}}", SplitLine.ToString());
            Marks.HasValue(o => Marks = "{}");
            var LeftMark = Marks[0];
            var RightMark = LeftMark;
            if (Marks.Length > 1) RightMark = Marks[1];

            foreach (string item in Dict.Keys)
            {
                Source = Source.Replace(LeftMark + item + RightMark, Dict[item]);
            }
            return Source;
            //return Source = Source.Replace(SplitSect.ToString(), "{{").Replace(SplitLine.ToString(), "}}");
        }

        ///// <summary>
        ///// string.Format 
        ///// </summary>
        ///// <param name="Source"></param>
        ///// <param name="Paras"></param>
        ///// <returns></returns>
        //public static string Format(this string Source, params string[] Paras)
        //{
        //    return string.Format(Source, Paras);
        //}


        public class SimilarResult
        {
            public int s1Index { get; set; }
            public int s2Index { get; set; }
            public string Value { get; set; }
        }

        public static SimilarResult[] GetMaxCommStrings(string s1, string s2, bool compareWithCase)
        {
            if (String.IsNullOrEmpty(s1) || String.IsNullOrEmpty(s2))
                return new SimilarResult[] { };
            else if (s1 == s2)
                return new SimilarResult[] { new SimilarResult() { s1Index = 0, s2Index = 0, Value = s1 } };

            var d = new int[s1.Length, s2.Length];

            var length = 0;

            for (int i = 0; i < s1.Length; i++)
            {
                for (int j = 0; j < s2.Length; j++)
                {
                    // 左上角值
                    var n = i - 1 >= 0 && j - 1 >= 0 ? d[i - 1, j - 1] : 0;

                    // 当前节点值 = "1 + 左上角值" : "0"
                    d[i, j] = (compareWithCase ? s1[i] == s2[j] : s1[i].EqualsNoMatter(s2[j])) ? 1 + n : 0;

                    // 如果是最大值，则记录该值和行号
                    if (d[i, j] > length)
                    {
                        length = d[i, j];
                    }
                }
            }

            if (length == 0) { return new SimilarResult[] { }; }
            var list = new List<SimilarResult>();
            for (int i = 0; i < s1.Length; i++)
            {
                for (int j = 0; j < s2.Length; j++)
                {
                    // 如果是最大值，则记录该值和行号
                    if (d[i, j] == length)
                    {
                        list.Add(new SimilarResult() { s1Index = i - length + 1, s2Index = j - length + 1, Value = s1.Substring(i - length + 1, length) });
                    }
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// 获取两个字符串的最大公共部分
        /// </summary>
        /// <remarks>
        /// http://hi.baidu.com/tangguoshequ/blog/item/d587dc170878c8946538dbd1.html
        /// </remarks>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="compareWithCase">是否区分大小写</param>
        /// <returns></returns>
        public static SimilarResult GetMaxCommString(string s1, string s2, bool compareWithCase)
        {
            if (String.IsNullOrEmpty(s1) || String.IsNullOrEmpty(s2))
            {
                return null;
            }
            else if (s1 == s2)
            {
                return new SimilarResult() { s1Index = 0, s2Index = 0, Value = s1 };
            }
            else if (compareWithCase == false && string.Equals(s1, s2, StringComparison.CurrentCultureIgnoreCase))
            {
                return new SimilarResult() { s1Index = 0, s2Index = 0, Value = s1 };
            }

            var rangData = new int[s1.Length, s2.Length];

            var maxLen = 0;

            for (int i = 0; i < s1.Length; i++)
            {
                for (int j = 0; j < s2.Length; j++)
                {
                    // 左上角值
                    var n = i - 1 >= 0 && j - 1 >= 0 ? rangData[i - 1, j - 1] : 0;

                    // 当前节点值 = "1 + 左上角值" : "0"
                    rangData[i, j] = (compareWithCase ? s1[i] == s2[j] : s1[i].EqualsNoMatter(s2[j])) ? 1 + n : 0;

                    // 如果是最大值，则记录该值和行号
                    if (rangData[i, j] > maxLen)
                    {
                        maxLen = rangData[i, j];
                    }
                }
            }

            if (maxLen == 0) { return null; }

            for (int i = 0; i < s1.Length; i++)
            {
                for (int j = 0; j < s2.Length; j++)
                {
                    // 如果是最大值，则记录该值和行号
                    if (rangData[i, j] == maxLen)
                    {
                        return new SimilarResult()
                        {
                            s1Index = i - maxLen + 1,
                            s2Index = j - maxLen + 1,
                            Value = s1.Substring(i - maxLen + 1, maxLen)
                        };

                    }
                }
            }
            return null;
        }

        public static SimilarResult[] GetCommStrings(string s1, string s2, bool compareWithCase)
        {
            if (String.IsNullOrEmpty(s1) || String.IsNullOrEmpty(s2))
            {
                return new SimilarResult[0];
            }
            else if (s1 == s2)
            {
                return new SimilarResult[] { new SimilarResult { s1Index = 0, s2Index = 0, Value = s1 } };
            }
            else if (compareWithCase == false && string.Equals(s1, s2, StringComparison.CurrentCultureIgnoreCase))
            {
                return new SimilarResult[] { new SimilarResult { s1Index = 0, s2Index = 0, Value = s1 } };
            }

            var rangData = new int[s1.Length, s2.Length];


            for (int i = 0; i < s1.Length; i++)
            {
                for (int j = 0; j < s2.Length; j++)
                {
                    // 左上角值
                    var n = i - 1 >= 0 && j - 1 >= 0 ? rangData[i - 1, j - 1] : 0;

                    // 当前节点值 = "1 + 左上角值" : "0"
                    rangData[i, j] = (compareWithCase ? s1[i] == s2[j] : s1[i].EqualsNoMatter(s2[j])) ? 1 + n : 0;
                }
            }

            var list = new List<SimilarResult>();
            for (int i = 0; i < s1.Length; i++)
            {
                for (int j = 0; j < s2.Length; j++)
                {
                    if (rangData[i, j] != 1)
                    {
                        continue;
                    }

                    var cstr = new List<char>();
                    cstr.Add(s1[i]);
                    for (var k = 1; k < Math.Min(s1.Length, s2.Length); k++)
                    {
                        if (rangData[i + k, j + k] == 0)
                        {
                            break;
                        }

                        cstr.Add(s1[i + k]);
                    }


                    list.Add(new SimilarResult()
                    {
                        s1Index = i,
                        s2Index = j,
                        Value = new string(cstr.ToArray())
                    });
                }
            }
            return list.ToArray();
        }
        //public enum SimilarEnum
        //{
        //    MaxLength = 0,
        //    First = 1,
        //    Second = 2,
        //}

        /// <summary>
        /// 获取两个字符串相似度。（最大公约数法的扩展）
        /// </summary>
        /// <remarks>
        ///       人 民 共 和 时  代 华 人 国
        ///    中 0, 0, 0, 0, 0, 0, 0, 0, 0
        ///    华 0, 0, 0, 0, 0, 0, 1, 0, 0
        ///    人 1, 0, 0, 0, 0, 0, 0, 2, 0
        ///    民 0, 2, 0, 0, 0, 0, 0, 0, 0
        ///    共 0, 0, 3, 0, 0, 0, 0, 0, 0
        ///    和 0, 0, 0, 4, 0, 0, 0, 0, 0
        ///    国 0, 0, 0, 0, 0, 0, 0, 0, 1
        /// 
        /// 得到 人民共和  和 华人 两个单词
        /// 其中 人民共和 是最大公约数
        /// 按 人民共和 把两个词条分隔。 左左， 右右，再递归求最大公约数
        /// 参照长度 默认为： Max( s1.Length + s2.Length) 
        /// 相似度 =   最大公约数.Length / 最长参照长度   + 
        ///            （左相似度   +  右相似度） /10
        ///            
        /// 1  表示， 整体的相似度 介于 四个连字到五个连字之间。
        /// 
        /// 
        /// "吉林长春市朝阳区"  
        /// "北京市市辖区朝阳区"
        /// 
        /// 北京市朝阳区兴隆庄甲3号
        /// </remarks>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="compareWithCase">是否区分大小写</param>
        /// <param name="CompareLevelCount">比较次数，如果指定，则按指定的次数来。否则递归进行，最大递归次数64</param>
        /// <returns></returns>
        public static double GetSimilar(string s1, string s2, bool compareWithCase = false, int CompareLevelCount = 64)
        {
            if (string.IsNullOrEmpty(s1)) return 0.0;
            if (string.IsNullOrEmpty(s2)) return 0.0;

            if (CompareLevelCount <= 0) return 0.0;

            var allSimilar = new List<double>();

            var commStrResult = GetMaxCommString(s1, s2, compareWithCase);
            if (commStrResult == null) return 0.0;

            var leftSimilar = 0.0;
            if (commStrResult.s1Index > 0 && commStrResult.s2Index > 0)
            {
                leftSimilar = GetSimilar(s1.Substring(0, commStrResult.s1Index), s2.Substring(0, commStrResult.s2Index), compareWithCase, CompareLevelCount - 1);
            }
            var rightSimilar = 0.0;
            if (commStrResult.s1Index + commStrResult.Value.Length < s1.Length &&
                commStrResult.s2Index + commStrResult.Value.Length < s2.Length)
            {
                rightSimilar = GetSimilar(s1.Substring(commStrResult.s1Index + commStrResult.Value.Length), s2.Substring(commStrResult.s2Index + commStrResult.Value.Length), compareWithCase, CompareLevelCount - 1);
            }

            var maxLength = Math.Max(s1.Length, s2.Length) * 1.0;
            var theSimilar = commStrResult.Value.Length / maxLength +
                    (leftSimilar + rightSimilar) / 10;

            return theSimilar;
        }


        /// <summary>
        /// 判断两个时间是否是同一天。
        /// </summary>
        /// <param name="dtValue"></param>
        /// <param name="compareDateTime"></param>
        /// <returns></returns>
        public static bool IsSameDay(this DateTime dtValue, DateTime compareDateTime)
        {
            if (dtValue.Year != compareDateTime.Year) return false;
            if (dtValue.Month != compareDateTime.Month) return false;
            if (dtValue.Day != compareDateTime.Day) return false;
            return true;
        }

        public static bool StartWithEx(this byte[] Source, params byte[] Item)
        {
            if (Item == null) return false;
            if (Source.Length < Item.Length) return false;

            for (int i = 0; i < Item.Length; i++)
            {
                if (Source.ElementAt(i) != Item[i])
                {
                    return false;
                }

            }
            return true;
        }
    }
}