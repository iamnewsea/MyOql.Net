using System;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace MyCmn
{
    /// <summary>
    /// 重载 List， 重载 + , - , * 操作符。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MyList<T> : List<T>
    {
        public MyList()
            : base()
        {
        }

        public MyList(IEnumerable<T> list)
            : base(list)
        {
        }


        /// <summary>
        /// 集合相加
        /// </summary>
        /// <param name="ListOne"></param>
        /// <param name="ListTwo"></param>
        /// <returns></returns>
        public static MyList<T> operator +(MyList<T> ListOne, List<T> ListTwo)
        {
            var list = new MyList<T>();
            if (ListOne != null)
            {
                list.AddRange(ListOne);
            }

            if (ListTwo != null)
            {
                list.AddRange(ListTwo);
            }
            return list;
        }

        /// <summary>
        /// 集合相减
        /// </summary>
        /// <param name="ListOne"></param>
        /// <param name="ListTwo"></param>
        /// <returns></returns>
        public static MyList<T> operator -(MyList<T> ListOne, List<T> ListTwo)
        {
            if (ListTwo == null)
            {
                return ListOne;
            }
            return new MyList<T>(ListOne.Minus(ListTwo));
        }


        /// <summary>
        /// 去重后，进行笛卡尔积
        /// </summary>
        /// <param name="ListKey"></param>
        /// <param name="ListValue"></param>
        /// <returns></returns>
        public static XmlDictionary<T, T> operator *(MyList<T> ListKey, List<T> ListValue)
        {
            var dict = new XmlDictionary<T, T>();

            if (ListValue == null)
            {
                return dict;
            }

            var list1 = ListKey.Distinct().ToList();
            var list2 = ListValue.Distinct().ToList();


            for (int i = 0; i < list1.Count; i++)
            {
                for (int j = 0; j < list2.Count; j++)
                {
                    dict[list1[i]] = list2[j];
                }
            }

            return dict;
        }
    }
}
