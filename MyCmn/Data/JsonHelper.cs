using System;
using System.Web.Script.Serialization;
using System.Collections.Specialized;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace MyCmn
{
    public static class JsonHelper
    {
        private static JsonSerializerSettings jSetting;
        static JsonHelper()
        {
            if (jSetting != null) return;

            try
            {
                //http://www.cnblogs.com/q28633999/archive/2011/06/11/2078398.html
                jSetting = new JsonSerializerSettings();
                jSetting.Formatting = Formatting.None;
                jSetting.NullValueHandling = NullValueHandling.Ignore;

                jSetting.Converters.Add(new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
                jSetting.ContractResolver = new DefaultContractResolver
                {
                    DefaultMembersSearchFlags = BindingFlags.Instance | BindingFlags.Public,
                    SerializeCompilerGeneratedMembers = true 
                };
            }
            catch (Exception e)
            {
                Log.To("系统初始化", "JsonHelper初始化发生错误：" + e.Message);
                throw;
            }
            //高级功能非常影响性能。by udi at 2013.11.5
            //jSetting.Converters.Add(new MyDateJsonNetConverter());
            //jSetting.Converters.Add(new MyStringBuilderJsonNetConverter());
            //jSetting.Converters.Add(new MyStringLinerJsonNetConverter());
            //jSetting.Converters.Add(new MyEnmItemJsonNetConverter());
        }

        public static void AddConverter(JsonConverter jsonConverter)
        {
            if (jsonConverter == null) return;

            if (jSetting.Converters.Contains(jsonConverter) == false)
            {
                jSetting.Converters.Add(jsonConverter);
            }
        }

        public static string ToJson<T>(this T source)
        {
            if (source == null) return string.Empty;
            return JsonConvert.SerializeObject(source, jSetting);

            //var nv = source as NameValueCollection;
            //if (nv != null)
            //{
            //    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            //    //执行序列化
            //    return jsonSerializer.Serialize(nv.ToDictionary());
            //}
            //else
            //{
            //    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            //    //执行序列化
            //    return jsonSerializer.Serialize(source);
            //}
        }

        public static T FromJson<T>(this string str, T defaultValue)
        {
            if (str.HasValue() == false)
            {
                return defaultValue;
            }
            else
            {
                string ss = str.Trim().Replace(Environment.NewLine, "");
                if (ss.StartsWith("[") == false && ss.StartsWith("{") == false)
                {
                    str = "{" + str + "}";
                }
            }

            return JsonConvert.DeserializeObject<T>(str, jSetting);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T FromJson<T>(this string str)
            where T : new()
        {
            return FromJson<T>(str, new T());

            //if (str[0] == '[')
            //{
            //    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            //    return jsonSerializer.Deserialize<T>(str);
            //}
            //Type type = typeof(T);
            //var nvType = typeof(NameValueCollection);
            //if (type == nvType ||
            //    type.IsSubclassOf(nvType))
            //{
            //    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            //    var dict = jsonSerializer.Deserialize<Dictionary<string, string>>(str);

            //    NameValueCollection nv = new NameValueCollection();
            //    if (dict == null) return new T();
            //    foreach (var item in dict)
            //    {
            //        nv[item.Key] = item.Value;
            //    }
            //    return (T)(object)nv;
            //}
            //else
            //{
            //    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            //    return jsonSerializer.Deserialize<T>(str);
            //}
        }

        //public static T[] Json2Array<T>(this string str)
        //{
        //    if (str.HasValue() == false)
        //    {
        //        return new T[0];
        //    }
        //    else
        //    {
        //        string ss = str.Trim().Replace(Environment.NewLine, "");
        //        if (ss.StartsWith("[") == false && ss.StartsWith("{") == false)
        //        {
        //            str = "{" + str + "}";
        //        }
        //    }

        //    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
        //    return jsonSerializer.Deserialize<T[]>(str);
        //}
    }
}
