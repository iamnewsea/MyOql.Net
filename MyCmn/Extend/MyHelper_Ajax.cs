using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Threading;
namespace MyCmn
{
    public static partial class MyHelper
    {
        ///// <summary>
        ///// 按选择器进行查找。
        ///// </summary>
        ///// <param name="nodes"></param>
        ///// <param name="selector">支持  id(#) (#必须独立存在), class(.) ,tag , 直接子级(>) </param>
        ///// <returns></returns>
        //public static List<CommonTreeItem<HtmlNode>> FindDoms(this CommonTreeItem<HtmlNode> treeNode, string DomSelector)
        //{
        //    var sects = DomSelector.SplitWithSeperator(">", " ").Select(o => o.Trim()).Where(o => o.HasValue()).ToList();
        //    var temp = new List<CommonTreeItem<HtmlNode>>() { treeNode };
        //    for (int i = 0; i < sects.Count; i++)
        //    {
        //        temp = _FindDoms(temp, sects[i]);
        //    }
        //    return temp;
        //}
        //private static List<CommonTreeItem<HtmlNode>> _FindDoms(List<CommonTreeItem<HtmlNode>> treeNodes, string selector)
        //{
        //    var ret = new List<CommonTreeItem<HtmlNode>>();
        //    treeNodes.All(node =>
        //        {
        //            ret.AddRange(_FindDoms(node, selector));
        //            return true;
        //        });
        //    return ret;
        //}
        //private static List<CommonTreeItem<HtmlNode>> _FindDoms(CommonTreeItem<HtmlNode> treeNode, string selector)
        //{
        //    var ret = new List<CommonTreeItem<HtmlNode>>();
        //    new Recursion<CommonTreeItem<HtmlNode>>().Execute(treeNode.Children, o => o.Children, o =>
        //        {
        //            if (o.Entity == null || o.Entity.Type != HtmlNode.NodeType.Tag) return RecursionReturnEnum.Go;
        //            var tag = o.Entity as HtmlTagNode;
        //            if (FilterDom(selector, tag))
        //            {
        //                ret.Add(o);
        //                if (selector.Contains('#'))
        //                {
        //                    return RecursionReturnEnum.StopSub;
        //                }
        //            }
        //            return RecursionReturnEnum.Go;
        //        });
        //    return ret;
        //}
        ///// <summary>
        ///// 匹配单个元素
        ///// </summary>
        ///// <param name="DomSelector"></param>
        ///// <param name="tag"></param>
        ///// <param name="parentIsMatched"></param>
        ///// <returns></returns>
        //public static bool FilterDom(string DomSelector, HtmlTagNode tag)
        //{
        //    if (tag == null) return false;
        //    if (DomSelector.StartsWith(">"))
        //    {
        //        DomSelector = DomSelector.Slice(1);
        //    }
        //    var sects = DomSelector.SplitWithSeperator("#", ".");
        //    foreach (var item in sects)
        //    {
        //        if (item.StartsWith("#"))
        //        {
        //            if (tag.GetAttributeValue("Id") == item.Slice(1))
        //            {
        //                continue;
        //            }
        //            else return false;
        //        }
        //        else if (item.StartsWith("."))
        //        {
        //            var cls = tag.GetAttributeValue("class").AsString();
        //            if ((" " + cls + " ").Contains(" " + item.Slice(1) + " "))
        //            {
        //                continue;
        //            }
        //            else return false;
        //        }
        //        else
        //        {
        //            if (string.Equals(item, tag.TagName, StringComparison.CurrentCultureIgnoreCase))
        //            {
        //                continue;
        //            }
        //            else return false;
        //        }
        //    }
        //    return true;
        //}
        /// <summary>
        /// 通过 Get 方式获取内容，加载到树里。
        /// </summary>
        /// <param name="fullUrl"></param>
        /// <param name="PreFunc"></param>
        /// <param name="HtmlFunc"></param>
        /// <returns></returns>
        public static HtmlTreeTagNode AjaxGet(string fullUrl, int TryTimes = 0, Action<HttpWebRequest> PreFunc = null, Func<string, string> HtmlFunc = null)
        {
            HttpWebResponse resp = null;
            byte[] ary = null;

            if (TryTimes < 0) TryTimes = 0;

            for (; TryTimes >= 0; TryTimes--)
            {
                try
                {
                    ary = GetRemoteArray(fullUrl, TryTimes, PreFunc, out resp);

                    if (ary == null)
                    {
                        continue;
                    }
                    break;
                }
                catch
                {
                    Thread.Sleep(200);
                }
            }


            if (ary == null)
            {
                return null;
            }

            string oriCharSet = null;
            string html = "";

            var bomDefine = ValueProc.GetBomDefine(ary);
            if (bomDefine != null)
            {
                oriCharSet = bomDefine.Type.GetBomEncodingName();
                ary = ary.Slice(bomDefine.BomData.Length).ToArray();
            }


            if (string.IsNullOrEmpty(oriCharSet) == false)
            {
                html = System.Text.Encoding.GetEncoding(oriCharSet).GetString(ary);
            }
            else
            {
                //应该先从 Response 的Header 中取出： Content-Type:text/html; charset=utf-8 编码格式，进行读取。 by udi 2014.04.12
                oriCharSet = resp.CharacterSet;

                if (string.IsNullOrEmpty(oriCharSet))
                {
                    oriCharSet = System.Text.Encoding.Default.BodyName;
                }

                html = System.Text.Encoding.GetEncoding(oriCharSet).GetString(ary);

                string charSet = null;
                if (html.TrimStart().FirstOrDefault() == '<')
                {
                    for (int i = 0; i < html.Length; i++)
                    {
                        var index = HtmlDomHelper.GetNextNoMatter(html, "<meta", i);
                        var end = HtmlDomHelper.GetNextNoMatter(html, "/>", index);
                        var str = html.Slice(index + 5, end);
                        var charSetIndex = str.IndexOf("charset", StringComparison.CurrentCultureIgnoreCase);
                        if (charSetIndex < 0)
                        {
                            i = end + 2;
                            continue;
                        }
                        charSet = HtmlDomHelper.GetNextWord(str, charSetIndex + 7, null, @""" '/>");
                        break;
                    }
                }

                if (charSet.HasValue() && (charSet != oriCharSet))
                {
                    html = System.Text.Encoding.GetEncoding(charSet).GetString(ary);
                }
            }


            //把 \u数字 转换为 字符
            var reg = new Regex(@"\\u.{4}", RegexOptions.Compiled);
            html = reg.Replace(html, new MatchEvaluator(match =>
            {
                if (match.Success == false) return match.Value;

                return char.ConvertFromUtf32(Convert.ToInt32(match.Value.Slice(2), 16));
            }));


            if (HtmlFunc != null)
            {
                html = HtmlFunc(html);
            }
            var nodes = new HtmlCharLoad(html.ToArray()).Load(HtmlNodeProc.ProcType.All);
            return nodes.LoadHtmlNode2Tree();
        }

        public enum UserAgentEnum
        {
            Win7X64Chrome = 1,
            Win7X64IE10,
            Win7X64IE8,
            Win7IE9,
            WinXpIE7,
            WinXpOpera,
        }

        public static string GetUserAgent(UserAgentEnum agentType)
        {
            var dict = new Dictionary<UserAgentEnum, string>();

            dict[UserAgentEnum.Win7X64Chrome] = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36";
            dict[UserAgentEnum.Win7X64IE8] = "mozilla/5.0 (compatible; msie 10.0; windows nt 6.1; wow64; trident/6.0; slcc2; .net clr 2.0.50727; .net clr 3.5.30729; .net clr 3.0.30729; media center pc 6.0; .net4.0c)";
            dict[UserAgentEnum.Win7X64IE8] = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)";

            dict[UserAgentEnum.Win7IE9] = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)";
            dict[UserAgentEnum.WinXpIE7] = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727; .NET4.0E; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET4.0C)";
            dict[UserAgentEnum.WinXpOpera] = "Opera/9.80 (Windows NT 5.1; U; zh-cn) Presto/2.9.168 Version/11.50";


            if (dict.ContainsKey(agentType)) return dict[agentType];
            else return string.Empty;
        }

        public static string GetRandomIP()
        {
            var s1 = new Random((DateTime.Now.Ticks % int.MaxValue).AsInt()).Next(80, 220);
            var s2 = new Random((DateTime.Now.Ticks % int.MaxValue).AsInt() + s1).Next(80, 220);
            var s3 = new Random((DateTime.Now.Ticks % int.MaxValue).AsInt() + s2).Next(80, 220);
            var s4 = new Random((DateTime.Now.Ticks % int.MaxValue).AsInt() + s3).Next(80, 220);

            if (s1 == 127 || s1 == 192)
            {
                s1 = 117;
            }

            return s1 + "." + s2 + "." + s3 + "." + s4;
        }
        private static byte[] GetRemoteArray(string fullUrl, int TryTimes, Action<HttpWebRequest> PreFunc, out HttpWebResponse resp)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullUrl);
            request.Proxy = null;

            request.Timeout = 3000;
            request.ReadWriteTimeout = 3000;

            //随机的 UserAgent
            var radom = new Random((DateTime.Now.Ticks % int.MaxValue).AsInt()).Next(1, 6);

            request.UserAgent = GetUserAgent((UserAgentEnum)radom);

            request.Headers.Add("X_FORWARDED_FOR", GetRandomIP());

            request.Method = "GET";
            if (PreFunc != null)
            {
                PreFunc(request);
            }
            resp = null;

            try
            {
                resp = request.GetResponse() as HttpWebResponse;
            }
            catch
            {
                return null;
            }


            var encoding = resp.ContentEncoding;
            var respStream = resp.GetResponseStream();
            if (encoding.Contains("gzip"))
            {
                respStream = new System.IO.Compression.GZipStream(respStream, System.IO.Compression.CompressionMode.Decompress);
            }
            System.IO.BinaryReader reader = new BinaryReader(respStream);

            var listAry = new List<byte>();

            while (true)
            {
                byte[] tempAry = null;

                try
                {
                    tempAry = reader.ReadBytes(10240);
                }
                catch
                {
                    return null;
                }

                if (tempAry.Length == 0) break;
                listAry.AddRange(tempAry);
            }

            return listAry.ToArray();
        }
        //public static string GetOuterHtml<T>(this CommonTreeItem<T> tree)
        //    where T : HtmlNode
        //{
        //    var sl = new StringLinker();
        //    if (tree.Entity == null) return string.Empty;
        //    if (tree.Entity.Type == HtmlNode.NodeType.Text) return (tree.Entity as HtmlTextNode).Text;
        //    if (tree.Entity.Type != HtmlNode.NodeType.Tag) return string.Empty;
        //    var tag = tree.Entity as HtmlTagNode;
        //    sl += tag.ToString();
        //    foreach (var item in tree.Children)
        //    {
        //        sl += GetOuterHtml(item);
        //    }
        //    sl += new HtmlCloseTagNode(tag.TagName).ToString();
        //    return sl;
        //}
        //public static string GetInnerText<T>(this CommonTreeItem<T> tree)
        //    where T : HtmlNode
        //{
        //    var sl = new StringLinker();
        //    if (tree.Entity != null)
        //    {
        //        if (tree.Entity.Type == HtmlNode.NodeType.Text)
        //        {
        //            return (tree.Entity as HtmlTextNode).Text.Replace("&nbsp;", " ");
        //        }
        //        if (tree.Entity.Type != HtmlNode.NodeType.Tag)
        //        {
        //            return string.Empty;
        //        }
        //    }

        //    foreach (var item in tree.Children)
        //    {
        //        sl += GetInnerText(item);
        //    }
        //    return sl;
        //}


        /// <summary>
        /// 通过  HttpWebRequest 模拟 Post。
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="PostData"></param>
        /// <param name="ResponseAction"></param>
        /// <param name="ExceptionAction"></param>
        public static void AjaxPost(string Url, Func<HttpWebRequest, StringDict> PostData = null, Action<string> ResponseAction = null, Action<string> ExceptionAction = null)
        {
            Action<Exception> ex = null;
            if (ExceptionAction != null)
            {
                ex = (e) => ExceptionAction(e.Message);
            }
            AjaxPost(Url, (req) =>
            {
                if (PostData == null) return null;

                var dict = PostData(req);
                if (dict == null) return null;

                req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                var postValue = dict.Select(o => HttpUtility.UrlEncode(o.Key) + "=" + HttpUtility.UrlEncode(o.Value)).Join("&");
                return postValue;
            },
            (data, res) =>
            {
                var strContent = (res.CharacterSet.HasValue() ? Encoding.GetEncoding(res.CharacterSet) : Encoding.Default)
                    .GetString(data);
                ResponseAction(strContent);
            },
            ex);
        }
        private class RequestState
        {
            // This class stores the State of the request.
            public const int BUFFER_SIZE = 1024;
            public List<byte> ResponseData;
            public byte[] BufferRead;
            public HttpWebRequest request;
            public HttpWebResponse response;
            public Stream streamResponse;
            public RequestState()
            {
                BufferRead = new byte[BUFFER_SIZE];
                ResponseData = new List<byte>();
                request = null;
                streamResponse = null;
            }
        }
        /// <summary>
        /// 通过 HttpWebRequest Post
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="RequestAction"></param>
        /// <param name="ResponseAction"></param>
        /// <param name="ExceptionAction"></param>
        public static void AjaxPost(string Url, Func<HttpWebRequest, string> RequestAction, Action<byte[], HttpWebResponse> ResponseAction, Action<Exception> ExceptionAction)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            //注意：使用 ""application/x-www-form-urlencoded; charset=UTF-8"" 用于Ajax提交Form表单内容。
            request.ContentType = "application/octet-stream";   //用于数据上传。
            RequestState myRequestState = new RequestState();
            myRequestState.request = request;
            using (var requestStream = myRequestState.request.GetRequestStream())
            {
                if (RequestAction != null)
                {
                    var postData = RequestAction(myRequestState.request);
                    if (postData != null)
                    {
                        var writeBytes = System.Text.Encoding.UTF8.GetBytes(postData);
                        requestStream.Write(writeBytes, 0, writeBytes.Length.AsInt());
                    }
                }
                try
                {
                    myRequestState.response = request.GetResponse() as HttpWebResponse; //(new AsyncCallback(UploadCallBack), myRequestState);
                }
                catch (Exception ee)
                {
                    ExceptionAction(ee);
                    return;
                }
                if (ResponseAction != null)
                {
                    while (true)
                    {
                        var readLen = myRequestState.response.GetResponseStream().Read(myRequestState.BufferRead, 0, myRequestState.BufferRead.Length);
                        if (readLen > 0)
                        {
                            myRequestState.ResponseData.AddRange(myRequestState.BufferRead.Slice(0, readLen));
                        }
                        else
                        {
                            ResponseAction(myRequestState.ResponseData.ToArray(), myRequestState.response);
                            break;
                        }
                    }
                }
            }
        }
    }
}
