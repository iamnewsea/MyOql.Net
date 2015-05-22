using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MyCmn
{
    /// <summary>
    /// 递归返回类型.
    /// </summary>
    public enum RecursionReturnEnum
    {
        /// <summary>
        /// 继续
        /// </summary>
        Go,

        /// <summary>
        /// 停止向下执行递归.
        /// </summary>
        StopSub,


        /// <summary>
        /// 终止递归.
        /// </summary>
        Abord,
    }

    public class Recursion<T> where T : class
    {
        /// <summary>
        /// 递归执行
        /// </summary>
        /// <param name="Container"></param>
        /// <param name="Subs"></param>
        /// <param name="Exec"></param>
        /// <returns></returns>
        //public RecursionReturnEnum Execute<S>(T Container, Func<T, IEnumerable<S>> Subs, Func<S, RecursionReturnEnum> SubExec)
        //{
        //    RecursionReturnEnum ret = RecursionReturnEnum.Go;
        //    var em = Subs(Container).GetEnumerator();
        //    while (em.MoveNext())
        //    {
        //        var current = em.Current  ;

        //        ret = Execute<S>(current, Subs, SubExec);
        //        if (ret == RecursionReturnEnum.StopSub)
        //        {
        //            continue;
        //        }
        //        else if (ret == RecursionReturnEnum.Abord)
        //        {
        //            break;
        //        }
        //    }

        //    return ret;
        //}

        /// <summary>
        /// 递归执行
        /// </summary>
        /// <param name="Container"></param>
        /// <param name="Subs"></param>
        /// <param name="Exec">传入的是子项</param>
        /// <returns></returns>
        public bool Execute(IEnumerable<T> Container, Func<T, IEnumerable<T>> Subs, Func<T, RecursionReturnEnum> Exec)
        {
            if (Container == null) return true;

            foreach (T item in Container)
            {
                var ret = Exec(item);

                if (ret == RecursionReturnEnum.StopSub) continue;
                else if (ret == RecursionReturnEnum.Abord) return false;

                if (Execute(Subs(item), Subs, Exec) == false)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 带容器的方法。
        /// </summary>
        /// <param name="Container"></param>
        /// <param name="Subs"></param>
        /// <param name="Exec"></param>
        /// <param name="Index">传入的是容器</param>
        /// <returns></returns>
        public RecursionReturnEnum Execute(T Container, Func<T, IEnumerable<T>> Subs, Func<T, int, RecursionReturnEnum> Exec, bool Root = true)
        {
            if (Container == null) return RecursionReturnEnum.StopSub;

            RecursionReturnEnum ret = new RecursionReturnEnum();
            if (Root)
            {
                Exec(Container, 0);
                if (ret == RecursionReturnEnum.StopSub) return RecursionReturnEnum.StopSub;
                else if (ret == RecursionReturnEnum.Abord) return RecursionReturnEnum.Abord;
            }

            var subs1 = Subs(Container);
            if (subs1 == null) return RecursionReturnEnum.StopSub;

            var subs2 = subs1.ToArray();
            for (var i = 0; i < subs2.Length; i++)
            {
                T item = subs2[i];
                ret = Exec(item, i);
                if (ret == RecursionReturnEnum.StopSub) continue;
                else if (ret == RecursionReturnEnum.Abord) return RecursionReturnEnum.Abord;
                ret = Execute(item, Subs, Exec, false);
                if (ret == RecursionReturnEnum.StopSub) continue;
                else if (ret == RecursionReturnEnum.Abord) return RecursionReturnEnum.Abord;
            }

            return RecursionReturnEnum.Go;
        }
        /// <summary>
        /// 递归执行
        /// </summary>
        /// <param name="Container"></param>
        /// <param name="Subs"></param>
        /// <param name="Exec"></param>
        /// <param name="InitLevel">初始Level</param>
        /// <returns></returns>
        public bool Execute(IEnumerable<T> Container, Func<T, IEnumerable<T>> Subs, Func<T, int, int,RecursionReturnEnum> Exec, int InitLevel = 0)
        {
            if (Container == null) return true;

            for (int i = 0, len = Container.Count(); i < len; i++)
            {
                var item = Container.ElementAt(i);
                var ret = Exec(item, InitLevel, i);

                if (ret == RecursionReturnEnum.StopSub) continue;
                else if (ret == RecursionReturnEnum.Abord) return false;

                if (Execute(Subs(item), Subs, Exec, InitLevel + 1) == false)
                    return false;
            }
            return true;
        }

        public R Get<R>(IEnumerable<T> Container, Func<T, IEnumerable<T>> Subs, Func<T, R> Exec)
        {
            foreach (T item in Container)
            {
                R retVal = Exec(item);
                if (retVal.HasValue()) return retVal;
                retVal = Get(Subs(item), Subs, Exec);
                if (retVal.HasValue())
                    return retVal;

            }
            return default(R);
        }


        /// <summary>
        /// 保留命中查询条件的所有子项，并保留命中条件的父项
        /// </summary>
        /// <param name="Container"></param>
        /// <param name="Subs"></param>
        /// <param name="Exec"></param>
        /// <returns>返回值仅在内部递归调用时有意义。表示。是否有一个子项命中。。</returns>
        public bool TreeFilter(List<T> Container, Func<T, List<T>> Subs, Func<T, bool> Exec)
        {
            var exeRet = false;
            var subRet = false;
            var ret = new List<T>();
            for (var i = 0; i < Container.Count; i++)
            {
                var item = Container[i];
                exeRet = Exec(item);

                subRet |= exeRet;
                if (exeRet)
                {
                    continue;
                }
                else
                {
                    exeRet = TreeFilter(Subs(item), Subs, Exec);
                    subRet |= exeRet;

                    if (exeRet == false)
                    {
                        //移除该子项。
                        Container.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
            }

            return subRet;
        }
    }

}
