
using System.Collections.Generic;
using System;
using System.Linq;

namespace MyCmn
{
    public static partial class ValueProc
    {
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T, T, bool> Comparer)
        {
            return source.Distinct(new CommonEqualityComparer<T>(Comparer));
        }

        /// <summary>
        /// 给数组添加一个值。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="data"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> AddOne<TSource>(this IEnumerable<TSource> data, TSource Value)
        {
            if (data == null) return new TSource[1] { Value };
            return data.Concat(new TSource[1] { Value });
        }

        /// <summary>
        /// 集合减法.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="data"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> Minus<TSource>(this IEnumerable<TSource> data, IEnumerable<TSource> other)
        {
            if (other != null)
            {
                return data.Where(o => (o.IsIn(other.ToArray()) == false));
            }
            else return data;
        }

        public static IEnumerable<TSource> Minus<TSource>(this IEnumerable<TSource> data, IEnumerable<TSource> other, IEqualityComparer<TSource> compare)
        {
            if (other != null)
            {
                return data.Where(o => (o.IsIn(compare, other.ToArray()) == false));
            }
            else return data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="data"></param>
        /// <param name="otherFunc">返回true 表示相等，要减去。返回false 不减。</param>
        /// <returns></returns>
        public static IEnumerable<TSource> Minus<TSource>(this IEnumerable<TSource> data, Func<TSource, bool> otherFunc)
        {
            if (otherFunc != null)
            {
                return data.Where(o => !otherFunc(o));
            }
            else return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="equalFunc"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static bool Contains<TSource>(this IEnumerable<TSource> data, Func<TSource, bool> equalFunc)
        {
            return !data.All(o => !equalFunc(o));
        }

        /// <summary>
        /// slice 方法从startIndex位置开始一直复制到 end 所指定的元素，但是不包括结束位置元素。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startIndex"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TSource> Slice<TSource>(this IEnumerable<TSource> source, int startIndex)
        {
            return Slice<TSource>(source, startIndex, int.MaxValue);
        }

        /// <summary>
        /// 
        ///类似于 javascript.slice 。 取出集合的一部分 new int[1,2,3].Slice(0,2) 返回 [1,2]
        /// </summary>
        /// <remarks>
        /// slice 方法从startIndex位置开始一直复制到 end 所指定的元素，但是不包括结束位置元素。
        /// 如果 start 为负，将它作为 length + start处理，此处 length 为数组的长度。
        /// 如果 end 为负，就将它作为 length + end 处理，此处 length 为数组的长度。
        /// 如果省略 end ，那么 slice 方法将一直复制到 arrayObj 的结尾。
        /// 如果 end 出现在 start 之前，不复制任何元素到新数组中。
        /// 示例:
        /// <code>
        ///  new int[1,2,3].Slice(0,-1) 返回 1,2
        ///  new int[1,2,3].Slice(1,0)  返回 空.
        ///  new int[1,2,3].Slice(-100,2) 返回 1,2
        ///  new int[1,2,3].Slice(-2,-1) 返回 3
        /// </code>
        /// </remarks>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="startIndex"> 表示要截取的字符串的开始索引 . 如果 start 为负，将它作为 length + start处理，此处 length 为数组的长度。 </param>
        /// <param name="endIndex">
        /// 表示要截取的字符串的结束索引,不包括该元素.
        /// 如果 end 为负，就将它作为 length + end 处理，此处 length 为数组的长度。
        /// 如果省略 end ，那么 slice 方法将一直复制到 arrayObj 的结尾。
        /// 如果 end 出现在 start 之前，不复制任何元素到新数组中。
        /// </param>
        /// <returns></returns>
        public static IEnumerable<TSource> Slice<TSource>(this IEnumerable<TSource> source, int startIndex, int endIndex)
        {
            if (source == null) return new List<TSource>();
            if (startIndex < 0) return Slice<TSource>(source, source.Count() + startIndex, endIndex);
            if (endIndex < 0) return Slice<TSource>(source, startIndex, source.Count() + endIndex);

            return source.Where((value, index) =>
            {
                return index >= startIndex && index < endIndex;
            });
        }

        /// <summary>
        /// 树匹配方法，遍历第二个树进行匹配
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OneTree">第一个树。</param>
        /// <param name="TwoTree">第二个树</param>
        /// <param name="SubFunc">查找子项方法</param>
        /// <param name="EqualFunc">判等方法</param>
        /// <param name="MatchFunc">如果找到同级的相同项的方法。参数：（第一个同级树的匹配对象，第二个同级树的遍历对象，第一个父级树对象。）</param>
        /// <param name="TwoTreeNotMatchFunc">在同级找不到相同项的方法。参数：（第一个父级树对象，第二个同级树中没有匹配的对象集合）</param>
        public static void TreeMatch<T>(this T OneTree,
            T TwoTree,
            Func<T, List<T>> SubFunc,
            Func<T, T, bool> EqualFunc,
            Action<T, T, T> MatchFunc,
            Action<T, List<T>> TwoTreeNotMatchFunc)
            where T : class
        {
            if (EqualFunc(OneTree, TwoTree) == false)
            {
                TwoTreeNotMatchFunc(null, new List<T>() { TwoTree });
                return;
            }

            var list = new List<T>();
            var OneChildren = SubFunc(OneTree);

            SubFunc(TwoTree).All(t =>
                {
                    var findOne = OneChildren.FirstOrDefault(o => EqualFunc(o, t));
                    if (findOne != null)
                    {
                        MatchFunc(findOne, t, OneTree);

                        TreeMatch(findOne, t, SubFunc, EqualFunc, MatchFunc, TwoTreeNotMatchFunc);
                    }
                    else
                    {
                        list.Add(t);
                    }
                    return true;
                });

            if (list.Count > 0)
            {
                TwoTreeNotMatchFunc(OneTree, list);
            }
        }


        /// <summary>
        /// 把第二个树，添加到第一个树上。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OneTree"></param>
        /// <param name="TwoTree"></param>
        /// <param name="SubFunc"></param>
        /// <param name="EqualFunc"></param>
        public static void TreeAdd<T>(this 　T OneTree,
             T TwoTree,
            Func<T, List<T>> SubFunc,
            Func<T, T, bool> EqualFunc)
            where T : class
        {
            TreeMatch<T>(OneTree, TwoTree, SubFunc, EqualFunc,
                    (a, b, l) =>
                    {
                    }, (a, b) =>
                    {
                        SubFunc(a).AddRange(b);
                    });
        }


        /// <summary>
        /// 从第一个树，减去第二个树。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OneTree"></param>
        /// <param name="TwoTree"></param>
        /// <param name="SubFunc"></param>
        /// <param name="EqualFunc"></param>
        public static void TreeMinus<T>(this T OneTree,
             T TwoTree,
            Func<T, List<T>> SubFunc,
            Func<T, T, bool> EqualFunc)
              where T : class
        {
            TreeMatch<T>(OneTree, TwoTree, SubFunc, EqualFunc,
                    (a, b, l) =>
                    {
                        SubFunc(l).Remove(a);
                    }, (a, b) =>
                    {
                    });
        }

        public static bool ForEach<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            if (source == null) return false;
            if (predicate == null) return false;

            var index = -1;
            return source.All(o =>
            {
                index++;

                return predicate(o, index);
            });
        }

        public static string GetNextCnWord(this string Value, int StartIndex = 0)
        {
            if (string.IsNullOrEmpty(Value)) return string.Empty;
            if (StartIndex < 0) StartIndex = 0;
            StartIndex = GetNextCnCharIndex(Value, StartIndex);
            if (StartIndex < 0) return string.Empty;

            var lastIndex = GetNextNotCnCharIndex(Value, StartIndex + 1);
            if (lastIndex < 0)
            {
                return Value.Slice(StartIndex);
            }

            return Value.Slice(StartIndex, lastIndex);
        }

        public static int GetNextNotCnCharIndex(this string Value, int StartIndex = 0)
        {
            if (string.IsNullOrEmpty(Value)) return -1;

            for (var lastIndex = StartIndex; lastIndex < Value.Length; lastIndex++)
            {
                var chr = Value[lastIndex];
                if (IsCnChar(chr) == false && char.IsLetterOrDigit(chr) == false)
                {
                    return lastIndex;
                }
            }
            return -1;
        }

        public static int GetNextCnCharIndex(this string Value, int StartIndex = 0)
        {
            if (string.IsNullOrEmpty(Value)) return -1;

            for (var lastIndex = StartIndex; lastIndex < Value.Length; lastIndex++)
            {
                var chr = Value[lastIndex];
                if (IsCnChar(chr) || char.IsLetterOrDigit(chr)) return lastIndex;
            }
            return -1;
        }

        public static bool IsCnChar(this char chr)
        {
            if (Convert.ToInt32(chr).Between(19968, 40959))
            {
                return true;
            }

            return false;
        }
    }
}
