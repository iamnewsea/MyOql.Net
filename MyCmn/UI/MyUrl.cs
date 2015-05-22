using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace MyCmn
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// 1. +  URL 中+号表示空格 %2B
    /// 2. 空格 URL中的空格可以用+号或者编码 %20
    /// 3. /  分隔目录和子目录 %2F 
    /// 4. ?  分隔实际的 URL 和参数 %3F 
    /// 5. % 指定特殊字符 %25 
    /// 6. # 表示书签 %23 
    /// 7. &amp; URL 中指定的参数间的分隔符 %26 
    /// 8. = URL 中指定参数的值 %3D 
    /// </remarks>
    public class MyUrl
    {
        /// <summary>
        /// http://localhost
        /// </summary>
        public string Prefix;

        public bool IsUrl { get; set; }

        /// <summary>
        /// URL 的 路径表示 ， 如： 
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// 查询字符串。
        /// </summary>
        public string Query { get; private set; }

        /// <summary>
        /// URL中 的表示 路径的各个部分。
        /// </summary>
        public string[] PathSegment;

        /// <summary>
        /// 查询字符串的对象表示。
        /// </summary>
        public Dictionary<string, string> QuerySegment;

        /// <summary>
        /// 后缀名。 含有 “.”
        /// </summary>
        public string Extension;

        /// <summary>
        /// 锚点后面的东西。
        /// </summary>
        public string Annex;

        public MyUrl()
        {
            this.IsUrl = false;
            this.Prefix = string.Empty;
            this.Path = string.Empty;
            this.Query = string.Empty;
            this.Annex = string.Empty;
            this.Extension = string.Empty;

            this.PathSegment = new string[0];
            this.QuerySegment = new Dictionary<string, string>();
        }

        public MyUrl(string Url)
            : this()
        {
            if (Url.HasValue() == false) return;

            if (Url.StartsWith("javascript:", StringComparison.CurrentCultureIgnoreCase) ||
                Url.StartsWith("#"))
            {
                this.Prefix = Url;
                return;
            }


            this.IsUrl = true;
            if (Url.StartsWith("/", StringComparison.CurrentCultureIgnoreCase))
            {
            }
            else if (Url.StartsWith("~/", StringComparison.CurrentCultureIgnoreCase) ||
                Url.StartsWith("./", StringComparison.CurrentCultureIgnoreCase) ||
                Url.StartsWith("../", StringComparison.CurrentCultureIgnoreCase))
            {
                // 五种情况： http:// , https:// ， /  , ~/ , ../ 
                Url = Url.Trim().ResolveUrl();
            }
            else
            {
                var match = new Regex(@"^\b(\w+)\://", RegexOptions.Compiled).Match(Url);
                if (match.Success == false)
                {
                    this.Prefix = Url;
                    return;
                }

                if (match.Index == 0)
                {
                    this.Prefix = Url.Substring(0, match.Length);
                    Url = Url.Substring(match.Length);
                }
                else
                {
                    this.Prefix = Url;
                    return;
                }

            }

            //if (Url.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) ||
            //    Url.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase) ||
            //    Url.StartsWith("/", StringComparison.CurrentCultureIgnoreCase) ||
            //    Url.StartsWith("~/", StringComparison.CurrentCultureIgnoreCase) ||
            //    Url.StartsWith("../", StringComparison.CurrentCultureIgnoreCase))
            //{
            //}
            //else
            //{
            //    this.Prefix = Url;
            //    return;
            //}



            //var myUrl = string.Empty;
            //if (Url.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase))
            //{
            //    int firstSplitIndex = Url.IndexOf('/', 8);
            //    if (firstSplitIndex > 0)
            //    {
            //        this.Prefix = Url.Substring(0, firstSplitIndex);
            //        Url = Url.Substring(firstSplitIndex);
            //    }
            //    else
            //    {
            //        this.Prefix = Url;
            //        return;
            //    }
            //}
            //else if (Url.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
            //{
            //    int firstSplitIndex = Url.IndexOf('/', 9);
            //    if (firstSplitIndex > 0)
            //    {
            //        this.Prefix = Url.Substring(0, firstSplitIndex);
            //        Url = Url.Substring(firstSplitIndex);
            //    }
            //    else
            //    {
            //        this.Prefix = Url;
            //        return;
            //    }
            //}
            //else if (HttpContext.Current != null)
            //{
            //    if (Url.StartsWith("/"))
            //    {
            //        myUrl = Url ;
            //    }
            //    else
            //    {
            //        var lp = HttpContext.Current.Request.Url.LocalPath;
            //        lp = lp.Substring(0, lp.LastIndexOf('/'));
            //        myUrl = lp + "/" + Url.TrimStart("./");
            //    }
            //}

            var pathQueryAndAnnex = Url.Split('#');
            if (pathQueryAndAnnex.Length > 1)
            {
                this.Annex = pathQueryAndAnnex[1];
            }

            var pathAndQuery = pathQueryAndAnnex[0].Split('?');

            var extIndex = pathAndQuery[0].IndexOf('.', pathAndQuery[0].LastIndexOf('/'));
            if (extIndex > 0)
            {
                this.Path = pathAndQuery[0].Substring(0, extIndex);
                this.Extension = pathAndQuery[0].Substring(extIndex);
            }
            else
            {
                this.Path = pathAndQuery[0];
            }

            if (pathAndQuery.Length > 1)
            {
                this.Query = pathAndQuery[1].AsString();
            }
            this.PathSegment = this.Path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            this.Query.Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).All(o =>
                {
                    var eachQuery = o.Split('=').ToList();
                    if (eachQuery.Count == 1) eachQuery.Add("");
                    this.QuerySegment[eachQuery[0]] = eachQuery[1];
                    return true;
                });
        }

        /// <summary>
        /// 把 MyUrl 对象表述为 Url 字符串对象。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var retVal = this.Prefix.AsString("/");
            if (PathSegment.Length > 0)
            {
                retVal += string.Join("/", this.PathSegment) + this.Extension.AsString();
            }
            if (this.QuerySegment.Count > 0)
            {
                retVal += "?" + string.Join("&", this.QuerySegment.Select(o => o.Key + "=" + o.Value).ToArray());
            }
            return retVal;
        }



        ///// <summary>
        ///// 通过当前请求的URL，得到  GetUrlHeader 前面的部分。 对于站点得到： http://localhost/ , 对于虚拟目录得到的是： http://localhost/Cne/
        ///// </summary>
        ///// <returns></returns>
        //public static string GetUrlPrefix()
        //{
        //    var index = HttpContext.Current.Request.Url.AbsoluteUri.IndexOf(HttpContext.Current.Request.Url.AbsolutePath);
        //    return HttpContext.Current.Request.Url.AbsoluteUri.Substring(0, index);
        //}


        /// <summary>
        /// 得到  ~/ 的 Url 根形式  。 对于站点，得到的是 / , 对虚拟目录得到 /Cne/
        /// </summary>
        /// <returns></returns>
        public static string GetUrlHeader()
        {
            var ret = HttpRuntime.AppDomainAppVirtualPath;  //它返回的是  /Cne
            if (ret.Last() == '/') return ret;
            return ret + "/";
        }
    }
}
