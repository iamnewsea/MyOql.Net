using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;

namespace MyCmn
{
    /// <summary>
    /// 区分大小写的字符串字典, ( StringDictionary,NameValueCollection  是不区分大小写的字符串字典)
    /// </summary>
    [Serializable]
    public class StringDict : XmlDictionary<string, string>
    {
        public StringDict() { }

        /// <summary>
        /// 自定义构造函数。采用 Key:Value,Key:Value 格式。
        /// </summary>
        /// <remarks>
        /// 去除前后的 {}。
        /// 采用 Key:Value,Key:Value 格式。
        /// :用 \: 进行转义。
        /// ,用 \, 进行转义。
        /// </remarks>
        /// <param name="Json"></param>
        public StringDict(string Json)
        {
            var strVal = Json.TrimWithPair("{", "}");
            strVal.Replace(@"\,", ValueProc.SplitSect.ToString());
            strVal.Replace(@"\:", ValueProc.SplitLine.ToString());

            strVal.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).All(o =>
                {
                    var kv = o.Split(":".ToCharArray());
                    if (kv.Length != 2) return true;
                    var key = kv[0].Replace(ValueProc.SplitSect.ToString(), ",");
                    var val = kv[1].Replace(ValueProc.SplitLine.ToString(), ":");
                    this[key] = val;
                    return true;
                });
        }


        public StringDict(IEnumerable<KeyValuePair<string, string>> dict)
        {
            if (dict == null) return;
            dict.All(o =>
            {
                this[o.Key] = o.Value;
                return true;
            });
        }

        public static StringDict LoadFrom<TKey, TValue>(IDictionary<TKey, TValue> obj)
        {
            var ret = new StringDict();

            if (obj == null) return ret;

            foreach (var item in obj.Keys)
            {
                ret.Add(item.AsString(), obj[item].AsString());
            }

            return ret;
        }

        /// <summary>
        /// 返回 Key:Value,Key:Value 格式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.Count == 0) return string.Empty;

            return string.Join(",",
                    this.Select(
                        o => o.Key.Replace(",", @"\,").Replace(":", @"\:") + ":" + o.Value.Replace(",", @"\,").Replace(":", @"\:")
                    ).ToArray());
        }
    }
}
