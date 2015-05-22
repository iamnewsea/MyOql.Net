using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using System.Configuration;


namespace MyCmn
{
    /// <summary>
    /// 缓存管理器，用于 缓存 服务器 IIS 端数据。使用的是 HttpRuntime.Cache类. 仅 .Net 4.0 支持.
    /// 如果缓存数据库的数据，请使用 dbo.GetFromCache
    /// </summary>
    /// <example>
    /// CacheHelper.Get 的作用是指当数据项失效后， 调用指定的委托来添加数据项，以保证该项缓存的持续性。
    /// 该示例用来演示最简的 代码风格， 描述 取出在缓存中维护数据。
    /// <code>
    /// var retVal = CacheHelper.Get(CacheKey.OrgUserID, delegate() {
    ///        var htState = new List&lt; string>();
    ///        htState.Add("sys");
    ///        htState.Add("org");
    ///        htState.Add("user");
    ///        htState.Add("none");
    ///        htState.Add("web");
    ///        return htState ;
    ///    });
    /// return retVal ;
    /// </code>
    /// </example>
    public static class CacheHelper
    {
        public static void Add(string key, object Data, int CacheSecond, string[] DependencyCacheKeys)
        {
            if (Data == null)
                return;

            CacheDependency dep = new CacheDependency(null, DependencyCacheKeys);

            HttpRuntime.Cache.Insert(key, Data,
                dep,
                System.Web.Caching.Cache.NoAbsoluteExpiration,
                TimeSpan.FromSeconds(CacheSecond),
                CacheItemPriority.Default,
                null
                );
        }



        /// <summary>
        /// 判断是否存在指定的 Key 的缓存项
        /// </summary>
        /// <param name="key">缓存 Key，可以自已定义， 但是 Key 的 String 在 缓存管理器里不能有重复。</param>
        /// <returns>布尔类型的值 ，存在返回 true ， 不存在返回 false 。</returns>
        public static bool IsExists(string key)
        {
            GodError.Check(key.HasValue() == false, "CacheKey 不能为空");

            return HttpRuntime.Cache[key] != null;

        }


        public static void Remove(string Key)
        {
            HttpRuntime.Cache.Remove(Key);
        }

        public static IEnumerable<string> GetKeys()
        {
            var enm = HttpRuntime.Cache.GetEnumerator();
            while (enm.MoveNext())
            {
                yield return enm.Key.ToString();
            }
        }

        /// <summary>
        /// 用最简单的方式来取一个缓存的值 。
        /// </summary>
        /// <typeparam name="T">缓存值的类型。</typeparam>
        /// <param name="key">缓存的 Key</param>
        /// <returns>得到的缓存值。</returns>
        public static T Get<T>(Enum key)
        {
            return Get<T>(key.ToString());
        }

        /// <summary>
        /// 用最简单的方式来取一个缓存的值 。
        /// </summary>
        /// <typeparam name="T">缓存值的类型。</typeparam>
        /// <param name="key">缓存的 Key</param>
        /// <returns>得到的缓存值。</returns>
        public static T Get<T>(string key)
        {
            return (T)HttpRuntime.Cache.Get(key);
        }


        /// <summary>
        /// 该方法用于取 指定 Key 的缓存数据项.<see cref="CacheHelper" />
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <typeparam name="T">缓存数据项的类型</typeparam>
        /// <param name="CacheKey">要缓存的 Key</param>
        /// <param name="time"></param>
        /// <param name="CachSet">当该数据项失效后， 要调用的用来添加数据项委托</param>
        /// <returns>要取的 缓存 Key 的数据</returns>
        public static T Get<T>(Enum CacheKey, int CacheSecond, string[] PandencyKeys, Func<T> CachSet)
        {
            return Get<T>(CacheKey.ToString(), CacheSecond, PandencyKeys, CachSet);
        }

        /// <summary>
        /// 该方法用于取 指定 Key 的缓存数据项.<see cref="CacheHelper" />
        /// </summary>
        /// <typeparam name="T">缓存数据项的类型</typeparam>
        /// <param name="CacheKey">要缓存的 Key</param>
        /// <param name="time">缓存时间</param>
        /// <param name="CachSet">当该数据项失效后， 要调用的用来添加数据项委托</param>
        /// <returns>要取的 缓存 Key 的数据</returns>
        public static T Get<T>(string CacheKey, int CacheSecond, string[] PandencyKeys, Func<T> CachSet)
        {
            if (CacheSecond <= 0) return CachSet();


            lock (MyHelper.GetLockObject("MyCmn.CacheHelper4.GetKey." + CacheKey))
            {
                if (IsExists(CacheKey) == false)
                {
                    var retVal = CachSet.Invoke();
                    CacheHelper.Add(CacheKey, retVal, CacheSecond, PandencyKeys);
                    return retVal;
                }
                else return Get<T>(CacheKey);
            }
        }
    }
}
