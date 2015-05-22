
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
        /// ���������һ��ֵ��
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
        /// ���ϼ���.
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
        /// <param name="otherFunc">����true ��ʾ��ȣ�Ҫ��ȥ������false ������</param>
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
        /// slice ������startIndexλ�ÿ�ʼһֱ���Ƶ� end ��ָ����Ԫ�أ����ǲ���������λ��Ԫ�ء�
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
        ///������ javascript.slice �� ȡ�����ϵ�һ���� new int[1,2,3].Slice(0,2) ���� [1,2]
        /// </summary>
        /// <remarks>
        /// slice ������startIndexλ�ÿ�ʼһֱ���Ƶ� end ��ָ����Ԫ�أ����ǲ���������λ��Ԫ�ء�
        /// ��� start Ϊ����������Ϊ length + start�����˴� length Ϊ����ĳ��ȡ�
        /// ��� end Ϊ�����ͽ�����Ϊ length + end �����˴� length Ϊ����ĳ��ȡ�
        /// ���ʡ�� end ����ô slice ������һֱ���Ƶ� arrayObj �Ľ�β��
        /// ��� end ������ start ֮ǰ���������κ�Ԫ�ص��������С�
        /// ʾ��:
        /// <code>
        ///  new int[1,2,3].Slice(0,-1) ���� 1,2
        ///  new int[1,2,3].Slice(1,0)  ���� ��.
        ///  new int[1,2,3].Slice(-100,2) ���� 1,2
        ///  new int[1,2,3].Slice(-2,-1) ���� 3
        /// </code>
        /// </remarks>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="startIndex"> ��ʾҪ��ȡ���ַ����Ŀ�ʼ���� . ��� start Ϊ����������Ϊ length + start�����˴� length Ϊ����ĳ��ȡ� </param>
        /// <param name="endIndex">
        /// ��ʾҪ��ȡ���ַ����Ľ�������,��������Ԫ��.
        /// ��� end Ϊ�����ͽ�����Ϊ length + end �����˴� length Ϊ����ĳ��ȡ�
        /// ���ʡ�� end ����ô slice ������һֱ���Ƶ� arrayObj �Ľ�β��
        /// ��� end ������ start ֮ǰ���������κ�Ԫ�ص��������С�
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
        /// ��ƥ�䷽���������ڶ���������ƥ��
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OneTree">��һ������</param>
        /// <param name="TwoTree">�ڶ�����</param>
        /// <param name="SubFunc">���������</param>
        /// <param name="EqualFunc">�еȷ���</param>
        /// <param name="MatchFunc">����ҵ�ͬ������ͬ��ķ���������������һ��ͬ������ƥ����󣬵ڶ���ͬ�����ı������󣬵�һ�����������󡣣�</param>
        /// <param name="TwoTreeNotMatchFunc">��ͬ���Ҳ�����ͬ��ķ���������������һ�����������󣬵ڶ���ͬ������û��ƥ��Ķ��󼯺ϣ�</param>
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
        /// �ѵڶ���������ӵ���һ�����ϡ�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OneTree"></param>
        /// <param name="TwoTree"></param>
        /// <param name="SubFunc"></param>
        /// <param name="EqualFunc"></param>
        public static void TreeAdd<T>(this ��T OneTree,
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
        /// �ӵ�һ��������ȥ�ڶ�������
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
