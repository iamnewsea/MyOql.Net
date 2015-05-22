using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MyCmn
{
    public class HtmlNodeProc
    {
        public enum ProcType
        {
            None = 0,
            Html = 1,
            Js = 2,
            Css = 4,
            Txt = 8,
            Level = 16,   //生成级别
            All = 63,
        }

        public List<HtmlNode> Nodes;
        public ProcType Type;
        public HtmlNodeProc(List<HtmlNode> Nodes)
        {
            this.Nodes = Nodes;


            for (var i = 0; i < this.Nodes.Count; i++)
            {
                if (Nodes[i].Type == HtmlNode.NodeType.Text)
                {
                    var txt = Nodes[i] as HtmlTextNode;

                    if (txt.Text.Trim().HasValue()) continue;


                    if (i == 0)
                    {
                        Nodes.RemoveAt(i);
                        i--;
                        continue;
                    }

                    if (Nodes[i - 1].Type == HtmlNode.NodeType.CloseTag)
                    {
                        Nodes.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (Nodes[i - 1].Type == HtmlNode.NodeType.Tag)
                    {
                        var tag = Nodes[i - 1] as HtmlTagNode;
                        var style = tag.Attrs.FirstOrDefault(o => o.IsSole == false && o.Name.Equals("style", StringComparison.CurrentCultureIgnoreCase));

                        if (style != null)
                        {
                            style.Value = Regex.Replace(style.Value, @"\r\n\s*", " ", RegexOptions.Compiled);
                        }

                        if (tag.TagName.IsIn(StringComparer.CurrentCultureIgnoreCase, "pre", "style", "script", "textarea"))
                        {
                            continue;
                        }

                        Nodes.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
            }
        }

        public void Proc(ProcType PType)
        {
            this.Type = PType;
            if (Type.Contains(ProcType.Html))
            {
                ProcHtml();
            }
            if (Type.Contains(ProcType.Js))
            {
                ProcJs();
            }
            if (Type.Contains(ProcType.Css))
            {
                ProcCss();
            }
            if (Type.Contains(ProcType.Txt))
            {
                ProcTxt();
            }

            if (Type.Contains(ProcType.Level))
            {
                var level = 0;
                for (int i = 0; i < Nodes.Count; i++)
                {
                    var node = Nodes[i];
                    node.Level = level;
                    if (node.Type == HtmlNode.NodeType.Tag)
                    {
                        if ((node as HtmlTagNode).IsSole == false)
                        {
                            level++;
                        }
                    }
                    else if (node.Type == HtmlNode.NodeType.CloseTag)
                    {
                        level--;

                        continue;
                    }
                }
            }
        }

        internal enum JsLineTypeEnum
        {
            /// <summary>
            /// 上一行结尾在字符串中。
            /// </summary>
            PrevEndIsInString,

            /// <summary>
            /// 上一行结尾表示结束。
            /// </summary>
            PrevEndIsEnd,


        }


        public class JsProc
        {
            protected JsProc() { }
            public string Source;
            public List<char> ProcedHtml;
            //Dictionary<string,string> NewVar ;

            public JsProc(string JsSource)
            {
                this.Source = JsSource;
                Dictionary<int, int> BeRemoved = new Dictionary<int, int>();
                //处理 // /**/
                int pos = 0;
                for (int i = 0; i < Source.Length; i++)
                {
                    i = HtmlDomHelper.GetNext(Source, '/', i);
                    if (i >= Source.Length - 1)
                    {
                        break;
                    }

                    if (Source[i + 1] == '/')
                    {
                        pos = GetNextDirect('\n', i);
                        BeRemoved[i] = pos - 1;
                        i = pos;

                        if (pos >= Source.Length - 1)
                        {
                            break;
                        }
                    }
                    else if (Source[i + 1] == '*')
                    {
                        pos = HtmlDomHelper.GetNextNoMatter(Source, "*/", i) + 1;
                        BeRemoved[i] = pos;
                        i = pos;
                        //Source.RemoveRange(i, pos - i);
                        continue;
                    }

                }

                pos = 0;
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<int, int> item in BeRemoved)
                {
                    sb.Append(Source.Substring(pos, item.Key - pos));
                    pos = item.Value + 1;
                }

                if (BeRemoved.Count > 0)
                {
                    sb.Append(Source.Substring(pos, Source.Length - pos));
                    Source = sb.ToString();
                }

                //由于后续代码出错,需要重写. 重写规范: 只去除空白. 不去除回车. 因为程序需要回车做为结束语句的标志.
                //后续的添加对 如果结尾是 ;等结尾
                //或者
                //不以 \ 结尾并且以 {}(),.*!%^&|-+=[]/<>?  开头的处理，则合并行。
                //行头有三种处理新结果：1.NewLine + row  （原封不动）  。2.TrimStart(row)  （去白，去回车。）3.NewLine + TrimStart(row)  （仅去除白）
                //行尾有两种处理结果： 1. TrimEnd(row)    （去白）       2. row     （原封不动）
                /*
                 有一种情况是 jv.page().Edit_OK = function(){}; jv.page().Edit_Cancel= function(){} ; 这种赋值，必须要有分号。没有分号会出错。 
                 */
                this.ProcedHtml = new List<char>();

                pos = 0;

                Action OneLineProc = () =>
                    {
                        //是否断行. js 中 以 \ 结尾 表示断行.
                        bool NeedNewLine = false;
                        Func<string, char, string> GetNotInString = (row, lastOne) =>
                            {
                                if (lastOne == ';' ||
                                    lastOne == '{' ||
                                    lastOne == '}' ||
                                    lastOne == '(' ||
                                    lastOne == ')' ||
                                    lastOne == ',' ||
                                    lastOne == '.' ||
                                    lastOne == '*' ||
                                    lastOne == '!' ||
                                    lastOne == '%' ||
                                    lastOne == '^' ||
                                    lastOne == '&' ||
                                    lastOne == '|' ||
                                    lastOne == '-' ||
                                    lastOne == '+' ||
                                    lastOne == '=' ||
                                    lastOne == '[' ||
                                    lastOne == ']' ||
                                    lastOne == '/' ||
                                    lastOne == '<' ||
                                    lastOne == '>' ||
                                    lastOne == '?')
                                {
                                    NeedNewLine = false;
                                    return row.Trim(' ', '\t');
                                }
                                else
                                {
                                    //其它情况，如边字符，未结尾的脚本等。
                                    NeedNewLine = true;

                                    return row.TrimStart(' ', '\t');
                                }
                            };

                        foreach (var row in Source.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None))
                        {
                            /*Js 中，形如： var a = " \
                             *                          
                             * " ;
                             * 这种情况是不存在的。空行的出现肯定是可以忽略的。
                            */
                            var row_Trim = row.TrimEnd();
                            if (row_Trim.Length == 0) continue;
                            var lastOne = row_Trim.LastOrDefault();
                            var firstOne = row_Trim.FirstOrDefault();

                            if (NeedNewLine == false)
                            {
                                ProcedHtml.AddRange(GetNotInString(row, lastOne));
                            }
                            else
                            {
                                if (lastOne == ';' ||
                                    lastOne == '{' ||
                                    lastOne == '}' ||
                                    lastOne == '(' ||
                                    lastOne == ')' ||
                                    lastOne == ',' ||
                                    lastOne == '.' ||
                                    lastOne == '*' ||
                                    lastOne == '!' ||
                                    lastOne == '%' ||
                                    lastOne == '^' ||
                                    lastOne == '&' ||
                                    lastOne == '|' ||
                                    lastOne == '-' ||
                                    lastOne == '+' ||
                                    lastOne == '=' ||
                                    lastOne == '[' ||
                                    lastOne == ']' ||
                                    lastOne == '/' ||
                                    lastOne == '<' ||
                                    lastOne == '>' ||
                                    lastOne == '?')
                                {
                                    NeedNewLine = false;
                                    ProcedHtml.AddRange(GetNotInString(row, lastOne));
                                }
                                else
                                {
                                    ProcedHtml.AddRange(Environment.NewLine);


                                    if (lastOne == ';' ||
                                        lastOne == '{' ||
                                        lastOne == '}' ||
                                        lastOne == '(' ||
                                        lastOne == ')' ||
                                        lastOne == ',' ||
                                        lastOne == '.' ||
                                        lastOne == '*' ||
                                        lastOne == '!' ||
                                        lastOne == '%' ||
                                        lastOne == '^' ||
                                        lastOne == '&' ||
                                        lastOne == '|' ||
                                        lastOne == '-' ||
                                        lastOne == '+' ||
                                        lastOne == '=' ||
                                        lastOne == '[' ||
                                        lastOne == ']' ||
                                        lastOne == '/' ||
                                        lastOne == '<' ||
                                        lastOne == '>' ||
                                        lastOne == '?')
                                    {
                                        NeedNewLine = false;
                                        ProcedHtml.AddRange(row.TrimEnd(' ', '\t'));
                                    }
                                    else
                                    {  //其它情况，如边字符，未结尾的脚本等。
                                        NeedNewLine = true;
                                        ProcedHtml.AddRange(row);
                                    }
                                }
                            }
                        }
                    };

                if (ValueProc.IsConfigJsCssCompressed())
                {
                    OneLineProc();
                }
                else
                {
                    ProcedHtml.AddRange(Source);
                }
                //bool isCutedLine = false;
                //for (int i = 0; i < Source.Length; )
                //{
                //    if (isCutedLine == false) i = GetNextNotNull(i, false);
                //    pos = GetFirstNext(i, '\r', '\n');
                //    if (pos >= Source.Length - 1)
                //    {
                //        ProcedHtml.AddRange(Source.Substring(i, Source.Length - i).TrimEnd());
                //        break;
                //    }

                //    ProcedHtml.AddRange(Source.Substring(i, pos - i + 2));

                //    if (pos > 0 && Source[pos - 1] == '\\') isCutedLine = true;
                //    i = pos + 2;
                //}
            }

            #region HtmlCharLoad API

            //protected bool IsValue(int Start, string Find)
            //{
            //    for (int i = 0; i < Find.Length; i++)
            //    {
            //        if (Source[Start + i] != Find[i]) return false;
            //    }
            //    return true;
            //}

            //protected bool IsValue(int Start, string Value, StringComparison Compare)
            //{
            //    return Source.Substring(Start, Value.Length).Equals(Value, Compare);
            //}





            /// <summary>
            /// 区分大小写.
            /// </summary>
            /// <param name="Find"></param>
            /// <param name="Start"></param>
            /// <returns></returns>
            protected int GetNext(string Find, int Start)
            {
                if (Find.Length == 1) return HtmlDomHelper.GetNext(Source, Find[0], Start);

                for (int i = Start; i < Source.Length; i++)
                {
                    bool IsEqual = true;
                    for (int j = 0; j < Find.Length; j++)
                    {
                        if (Source[i + j] != Find[j])
                        {
                            IsEqual = false;
                            break;
                        }
                    }

                    if (IsEqual) return i;
                }
                return Source.Length;

            }



            /// <summary>
            /// 查找下一个不在字符串中的匹配.不进行任何判断
            /// </summary>
            /// <param name="Find"></param>
            /// <param name="Start"></param>
            /// <returns></returns>
            protected int GetNextDirect(char Find, int Start)
            {
                for (int i = Start; i < Source.Length; i++)
                {
                    if (Source[i] == Find)
                    {
                        return i;
                    }
                }
                return Source.Length;
            }

            /// <summary>
            /// 查找下一个不在字符串中的匹配.
            /// </summary>
            /// <param name="Start"></param>
            /// <param name="IsJumpString"></param>
            /// <returns></returns>
            protected int GetNextNotNull(int Start, bool IsJumpString)
            {
                for (int j = Start; j < Source.Length; j++)
                {
                    if (IsJumpString)
                    {
                        j = HtmlDomHelper.IsStringAndJump(Source, j);
                    }
                    if (Source[j] == ' ') continue;
                    if (Source[j] == '\t') continue;
                    if (Source[j] == '\n') continue;
                    if (Source[j] == '\r' && Source[j + 1] == '\n') continue;
                    return j;
                }

                return Source.Length;
            }
            #endregion
        }


        public class CssProc : JsProc
        {
            public CssProc(string JsSource)
                : base()
            {
                this.Source = JsSource;
                Dictionary<int, int> BeRemoved = new Dictionary<int, int>();
                //处理 /* background: color;*/
                int pos = 0;
                for (int i = 0; i < Source.Length; i++)
                {
                    i = HtmlDomHelper.GetNext(Source, '/', i);
                    if (i >= Source.Length - 1)
                    {
                        break;
                    }
                    if (Source[i + 1] == '*')
                    {
                        pos = HtmlDomHelper.GetNextNoMatter(Source, "*/", i) + 1;
                        BeRemoved[i] = pos;
                        i = pos;
                        //Source.RemoveRange(i, pos - i);
                        continue;
                    }

                }

                pos = 0;
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<int, int> item in BeRemoved)
                {
                    sb.Append(Source.Substring(pos, item.Key - pos));
                    pos = item.Value + 1;
                }

                if (BeRemoved.Count > 0)
                {
                    sb.Append(Source.Substring(pos, Source.Length - pos));
                    Source = sb.ToString();
                }

                //由于后续代码出错,需要重写. 重写规范: 只去除空白. 不去除回车. 因为程序需要回车做为结束语句的标志.

                this.ProcedHtml = new List<char>();

                pos = 0;

                Action OneLineProc = () =>
                    {
                        //是否断行. js 中 以 \ 结尾 表示断行.
                        //CSS 当行尾以 {}; 结尾时，去除前后空白。
                        bool IsInString = false;
                        Func<string, char, string> GetNotInString = (row, lastOne) =>
                        {
                            if (lastOne == ';' ||
                                lastOne == '{' ||
                                lastOne == '}')
                            {
                                IsInString = false;
                                return row.Trim(' ', '\t');
                            }
                            else
                            {
                                //其它情况，如边字符，未结尾的脚本等。
                                IsInString = true;

                                return row.TrimStart(' ', '\t');
                            }
                        };

                        foreach (var row in Source.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None))
                        {
                            /*Js 中，形如： var a = " \
                             *                          
                             * " ;
                             * 这种情况是不存在的。空行的出现肯定是可以忽略的。
                            */
                            var row_Trim = row.TrimEnd();
                            if (row_Trim.Length == 0) continue;
                            var lastOne = row_Trim.LastOrDefault();
                            var firstOne = row_Trim.FirstOrDefault();

                            if (IsInString == false)
                            {
                                ProcedHtml.AddRange(GetNotInString(row, lastOne));
                            }
                            else
                            {
                                if (lastOne == ';' ||
                                    lastOne == '{' ||
                                    lastOne == '}')
                                {
                                    IsInString = false;
                                    ProcedHtml.AddRange(GetNotInString(row, lastOne));
                                }
                                else
                                {
                                    ProcedHtml.AddRange(Environment.NewLine);


                                    if (lastOne == ';' ||
                                        lastOne == '{' ||
                                        lastOne == '}')
                                    {
                                        IsInString = false;
                                        ProcedHtml.AddRange(row.TrimEnd(' ', '\t'));
                                    }
                                    else
                                    {  //其它情况，如边字符，未结尾的脚本等。
                                        IsInString = true;
                                        ProcedHtml.AddRange(row);
                                    }
                                }
                            }
                        }
                    };

                if (ValueProc.IsConfigJsCssCompressed())
                {
                    OneLineProc();
                }
                else
                {
                    ProcedHtml.AddRange(Source);
                }
            }
        }


        #region Compress
        public void ProcTxt()
        {
        }

        public void ProcCss()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Type == HtmlNode.NodeType.Text)
                {
                    HtmlTextNode txt = Nodes[i] as HtmlTextNode;
                    if (i > 0 && Nodes[i - 1].Type == HtmlNode.NodeType.Tag)
                    {
                        HtmlTagNode tag = Nodes[i - 1] as HtmlTagNode;
                        if (string.Equals(tag.TagName, "style", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (txt.Text.HasValue())
                            {
                                txt.Text = new CssProc(txt.Text).ProcedHtml.AsString();
                            }
                        }
                    }
                }
            }
        }

        public void ProcJs()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Type == HtmlNode.NodeType.Text)
                {
                    HtmlTextNode txt = Nodes[i] as HtmlTextNode;
                    if (i > 0 && Nodes[i - 1].Type == HtmlNode.NodeType.Tag)
                    {
                        HtmlTagNode tag = Nodes[i - 1] as HtmlTagNode;
                        if (string.Equals(tag.TagName, "script", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (txt.Text.HasValue())
                            {
                                txt.Text = new JsProc(txt.Text).ProcedHtml.AsString();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 去空白,去注释.
        /// </summary>
        public void ProcHtml()
        {
            //int pos = 0;
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Type == HtmlNode.NodeType.Note)
                {
                    Nodes.RemoveAt(i);
                    i--;
                    continue;
                }
                //else if (i > 0)
                //{
                //    if (Nodes[i - 1].Type == HtmlNode.NodeType.Tag)
                //    {
                //        var tag = Nodes[i - 1] as HtmlTagNode;
                //        if (tag.TagName.IsIn(StringComparer.CurrentCultureIgnoreCase, "pre", "style", "script", "textarea"))
                //        {
                //            continue;
                //        }
                //    }
                //    pos = 0;
                //    StringBuilder sb = new StringBuilder();
                //    for (int j = HtmlDomHelper.GetNextNotNull(txt.Text, 0, false); j < txt.Text.Length; j++)
                //    {
                //        pos = HtmlDomHelper.GetFirstNext(txt.Text, j, ' ', '\t', '\r', '\n');

                //        sb.Append(txt.Text.Substring(j, pos - j));
                //        j = HtmlDomHelper.GetNextNotNull(txt.Text, pos + 1, false) - 1;

                //        if (j >= txt.Text.Length - 1) break;
                //        else { sb.Append(" "); }
                //    }

                //    if (pos > 0) { txt.Text = sb.ToString(); }
                //}
                //}
            }

            return;
            //for (int i = 0; i < Nodes.Count; i++)
            //{
            //    if (Nodes[i].Type == HtmlNode.NodeType.Tag)
            //    {
            //        HtmlTagNode tag = Nodes[i] as HtmlTagNode;
            //        if (tag.Attrs.Count > 0)
            //        {
            //            for (int j = 0; j < tag.Attrs.Count; j++)
            //            {
            //                if (tag.Attrs[j].IsSole == true) continue;
            //                string val = tag.Attrs[j].Value;
            //                if (val.HasValue() == false) continue;
            //                int indN = val.IndexOf('\n');
            //                if (indN < 0) continue;

            //                if (val[indN - 1] == '\r' && val[indN - 2] == '\\')
            //                {
            //                    if (IsVerity(val, indN - 1) == false)
            //                    {
            //                        val = val.Remove(indN - 2, 3);
            //                    }
            //                    else val = val.Remove(indN - 1, 2);

            //                    tag.Attrs[j].Value = val;
            //                    continue;
            //                }
            //                if (val[indN - 1] == '\r')
            //                {
            //                    val = val.Remove(indN - 1, 2);
            //                    tag.Attrs[j].Value = val;
            //                    continue;
            //                }
            //                else
            //                {
            //                    val = val.Remove(indN, 1);
            //                    tag.Attrs[j].Value = val;
            //                    continue;
            //                }
            //            }
            //        }
            //    }
            //}
        }
        #endregion


    }

}
