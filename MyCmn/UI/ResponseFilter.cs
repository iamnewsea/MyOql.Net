using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Web.Mvc;

namespace MyCmn
{
    /// <summary>
    /// 占用URL 的 _OriFilter_ 参数 , 如果为 true , 则不过滤.
    /// </summary>
    public partial class ResponseFilter : Stream
    {
        private Stream m_sink;
        private Action<List<HtmlNode>> m_PreProc;
        private Action<List<HtmlNode>> m_EndProc;

        public ResponseFilter(Stream sink, Action<List<HtmlNode>> PreProc, Action<List<HtmlNode>> EndProc)
        {
            m_sink = sink;
            this.m_PreProc = PreProc;
            this.m_EndProc = EndProc;
            listBuffer = new List<byte>();
        }

        // The following members of Stream must be overriden.
        public override bool CanRead
        { get { return m_sink.CanRead; } }

        public override bool CanSeek
        { get { return m_sink.CanSeek; } }

        public override bool CanWrite
        { get { return m_sink.CanWrite; } }

        public override long Length
        { get { return m_sink.Length; } }

        public override long Position
        {
            get { return m_sink.Position; }
            set { m_sink.Position = value; ; }
        }

        public override long Seek(long offset, SeekOrigin direction)
        {
            return m_sink.Seek(offset, direction);
        }

        public override void SetLength(long length)
        {
            m_sink.SetLength(length);
        }

        public override void Close()
        {
            ProcHtml();
            m_sink.Close();
        }

        public override void Flush()
        {
            m_sink.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return m_sink.Read(buffer, offset, count);
        }


        private List<byte> listBuffer;

        private void ProcHtml()
        {
            //string text = string.Join("", listBuffer.ToArray());
            byte[] byto = null;

            listBuffer = ValueProc.RemoveBom(listBuffer.ToArray()).ToList();

            if (HttpContext.Current != null && HttpContext.Current.Request.QueryString["_OriRender_"].AsBool())
            {
                m_sink.Write(listBuffer.ToArray(), 0, listBuffer.Count);
                return;
            }

            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //for (int kk = 0; kk < 10000; kk++)
            //{

            //List<HtmlNode> htmlDom = new HtmlParse().Load(text);


            bool IsDebug = HttpContext.Current.IsDebuggingEnabled;

            List<HtmlNode> htmlDom =
                new HtmlCharLoad(HttpContext.Current.Response.ContentEncoding.GetString(listBuffer.ToArray()).ToCharArray())
                .Load(IsDebug ? HtmlNodeProc.ProcType.None : HtmlNodeProc.ProcType.All);

            if (m_PreProc != null)
            {
                m_PreProc(htmlDom);
            }

            List<string> useFiles = new List<string>();

            ////整理 script,将所有script 提前,放入 head.
            //for (int i = 0; i < htmlDom.Count; i++)
            //{
            //    if (htmlDom[i] is HtmlCloseTagNode)
            //    {
            //        if (string.Equals((htmlDom[i] as HtmlCloseTagNode).TagName, "head", StringComparison.CurrentCultureIgnoreCase))
            //            continue;
            //    }
            //}

            //解析 ~/
            for (int i = 0; i < htmlDom.Count; i++)
            {
                if (htmlDom[i].Type == HtmlNode.NodeType.Tag)
                {
                    HtmlTagNode tn = htmlDom[i] as HtmlTagNode;

                    for (int j = 0; j < tn.Attrs.Count; j++)
                    {
                        HtmlAttrNode an = tn.Attrs[j];
                        if (an.IsSole == true) continue;
                        if (an.Name == "value" &&
                            tn.TagName.IsIn(StringComparer.CurrentCultureIgnoreCase, "input"))
                        {
                            continue;
                        }

                        an.Value = an.Value
                            .Replace("~/", MyUrl.GetUrlHeader())
                            ;
                    }
                }
                else if (htmlDom[i] is HtmlTextNode)
                {
                    HtmlTextNode tn = htmlDom[i] as HtmlTextNode;
                    if (i > 0 && htmlDom[i - 1] is HtmlTagNode && (htmlDom[i - 1] as HtmlTagNode).TagName.IsIn(
                        StringComparer.CurrentCultureIgnoreCase, "script", "style"))
                    {
                        tn.Text = tn.Text
                            .Replace("~/", MyUrl.GetUrlHeader())
                             ;
                    }
                }
            }

            //在 head 里引用文件去重
            for (int i = 0; i < htmlDom.Count; i++)
            {
                if (htmlDom[i].Type == HtmlNode.NodeType.CloseTag)
                {
                    if (string.Equals((htmlDom[i] as HtmlCloseTagNode).TagName, "head", StringComparison.CurrentCultureIgnoreCase))
                        break;
                }

                if (htmlDom[i].Type == HtmlNode.NodeType.Tag)
                {
                    HtmlTagNode tn = htmlDom[i] as HtmlTagNode;

                    if (string.Equals(tn.TagName, "link", StringComparison.CurrentCultureIgnoreCase))
                    {
                        HtmlAttrNode atr = tn.Attrs.Where(o => string.Equals(o.Name, "href")).FirstOrDefault();
                        if (atr != null)
                        {
                            string url = atr.Value.ResolveUrl();
                            atr.Value = url;
                            if (useFiles.Contains(url, StringComparer.CurrentCultureIgnoreCase) == false)
                            {
                                useFiles.Add(url);
                            }
                            else
                            {
                                if (tn.IsSole)
                                {
                                    htmlDom.RemoveAt(i);
                                    i--;
                                }
                                else
                                {
                                    htmlDom.RemoveAt(i);
                                    htmlDom.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                    }
                    else if (string.Equals(tn.TagName, "script", StringComparison.CurrentCultureIgnoreCase))
                    {
                        HtmlAttrNode atr = tn.Attrs.Where(o => string.Equals(o.Name, "src")).FirstOrDefault();
                        if (atr != null)
                        {
                            string url = atr.Value.ResolveUrl();
                            atr.Value = url;
                            if (useFiles.Contains(url, StringComparer.CurrentCultureIgnoreCase) == false)
                            {
                                useFiles.Add(url);
                            }
                            else
                            {
                                if (tn.IsSole)
                                {
                                    htmlDom.RemoveAt(i);
                                    i--;
                                }
                                else
                                {
                                    htmlDom.RemoveAt(i);
                                    htmlDom.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                    }
                }
            }





            if (m_EndProc != null)
            {
                m_EndProc(htmlDom);
            }
            byto = HttpContext.Current.Response.ContentEncoding.GetBytes(string.Join("", htmlDom.Select(o => o.ToString()).ToArray()));

            m_sink.Write(byto, 0, byto.Length);
            return;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            listBuffer.AddRange(buffer.Slice(offset, count - offset));
        }
    }
}
