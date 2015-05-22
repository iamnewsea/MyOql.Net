//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;

//namespace MyCmn
//{
//    /// <summary>
//    /// Release 下，万次　仅LoadToSect 方法 19776 毫秒．相比 CharLoad 模式,巨慢,Load方法 29776毫秒.
//    /// </summary>
//    public class HtmlParse
//    {
//        private List<string> Sects = null;
//        public List<HtmlNode> Load(string Html)
//        {
//            List<HtmlNode> retVal = new List<HtmlNode>();

//            LoadToSect(Html);
//            int curLevel = 0;

//            int start = 0;
//            ////判断是否是被截断的输出流
//            //int cutedClosedIndex = GetCutedClosedTagIndex(start);

//            //if (cutedClosedIndex > 0)
//            //{
//            //    HtmlTextNode txt = new HtmlTextNode();

//            //    txt.Level = 1;
//            //    txt.Text = string.Join("", Sects.GetSub(start, cutedClosedIndex));
//            //    retVal.Add(txt);

//            //    start = cutedClosedIndex - 1;
//            //}

//            for (int i = start; i < Sects.Count; i++)
//            {
//                int beginTagIndex = GetNextMarkIndex(i);

//                if (retVal.Count > 0 && retVal[retVal.Count - 1] is HtmlTagNode)
//                {
//                    string tagName = (retVal[retVal.Count - 1] as HtmlTagNode).TagName;
//                    if (tagName.IsIn(StringComparer.CurrentCultureIgnoreCase,
//                    "script", "style", "pre", "textarea"))
//                    {
//                        int endTagIndex = GetCloseTag(beginTagIndex, tagName);

//                        HtmlTextNode txt = new HtmlTextNode();

//                        txt.Level = curLevel + 1;

//                        txt.Text = string.Join("", Sects.Slice(i, beginTagIndex).ToArray());

//                        txt.Text += string.Join("", Sects.Slice(beginTagIndex - 1, endTagIndex + 1).ToArray());

//                        retVal.Add(txt);
//                        i = endTagIndex - 1;
//                        continue;
//                    }
//                }


//                if (i != beginTagIndex)
//                {
//                    HtmlTextNode txt = new HtmlTextNode();

//                    txt.Level = curLevel + 1;

//                    txt.Text = string.Join("", Sects.Slice(i, beginTagIndex).ToArray());

//                    retVal.Add(txt);
//                    i = beginTagIndex - 1;
//                    continue;
//                }

//                if (Sects[beginTagIndex] == "<")
//                {
//                    HtmlTagNode tag = new HtmlTagNode();

//                    tag.Level = curLevel + 1;
//                    tag.TagName = Sects[beginTagIndex + 1];

//                    int endTagIndex = GetNextMarkIndex(beginTagIndex + 2);
//                    if (Sects[endTagIndex] == @"/>")
//                    {
//                        tag.IsSole = true;
//                        curLevel--;
//                    }

//                    HtmlAttrNode an = null;
//                    for (int j = beginTagIndex + 2; j < endTagIndex; j++)
//                    {
//                        if (Sects[j].Trim().HasValue() == false)
//                        {
//                            continue;
//                        }
//                        if (an == null)
//                        {
//                            an = new HtmlAttrNode();
//                            an.IsSole = true;
//                            an.Name = Sects[j];
//                            tag.Attrs.Add(an);
//                            continue;
//                        }
//                        else if (Sects[j] == "=")
//                        {
//                            an.IsSole = false;
//                            continue;
//                        }
//                        else
//                        {
//                            if (an.IsSole == true)
//                            {
//                                an = new HtmlAttrNode();
//                                an.IsSole = true;
//                                an.Name = Sects[j];
//                                tag.Attrs.Add(an);
//                                continue;
//                            }
//                            else
//                            {
//                                an.Value = Sects[j];
//                                an = null;
//                            }
//                        }
//                    }

//                    retVal.Add(tag);
//                    i = endTagIndex;

//                    curLevel++;
//                }
//                else if (Sects[beginTagIndex] == "</" && IsColseTag(beginTagIndex, ""))
//                {
//                    HtmlCloseTagNode cl = new HtmlCloseTagNode();
//                    cl.TagName = Sects[beginTagIndex + 1];

//                    cl.Level = curLevel;

//                    retVal.Add(cl);

//                    i = beginTagIndex + 2;
//                    curLevel--;
//                }
//                else
//                {
//                    HtmlTextNode txt = new HtmlTextNode();

//                    txt.Level = curLevel + 1;

//                    txt.Text = string.Join("", Sects.Slice(i, beginTagIndex).ToArray());
//                    retVal.Add(txt);
//                }

//            }


//            for (int i = 0; i < retVal.Count; i++)
//            {
//                if (retVal[i] is HtmlTextNode)
//                {
//                    bool IsNormalText = true;
//                    HtmlTextNode txt = retVal[i] as HtmlTextNode;

//                    if (i > 0)
//                    {
//                        if (retVal[i - 1] is HtmlTagNode)
//                        {
//                            if ((retVal[i - 1] as HtmlTagNode).TagName.IsIn(StringComparer.CurrentCultureIgnoreCase,
//                                "pre", "textarea"))
//                            {
//                                IsNormalText = false;
//                            }
//                        }
//                    }

//                    if (IsNormalText) txt.Text = txt.Text.Trim();

//                    if (txt.Text.HasValue() == false)
//                    {
//                        retVal.RemoveAt(i);
//                        i--;
//                    }
//                }
//            }
//            return retVal;
//        }

//        private static int GetNextClose(List<string> Sects, string CloseString, int Start)
//        {
//            for (int i = Start; i < Sects.Count; i++)
//            {
//                if (Sects[i] == CloseString)
//                {
//                    if (i > 0 && Sects[i - 1] != @"\")
//                    {
//                        return i;
//                    }
//                }
//            }
//            return Sects.Count - 1;
//        }

//        private static int GetNextClose(string Content, char CloseTag, int Start)
//        {
//            for (int i = Start; i < Content.Length; i++)
//            {
//                if (Content[i] == CloseTag)
//                {
//                    if (i > 0 && Content[i - 1] != '\\')
//                    {
//                        return i;
//                    }
//                }
//            }
//            return Content.Length - 1;
//        }

//        private static bool InString(Dictionary<int, int> DictSored, int TestIndex)
//        {
//            foreach (var item in DictSored)
//            {
//                if (item.Value < TestIndex)
//                {
//                    return false;
//                }
//                else if (item.Key > TestIndex && item.Value < TestIndex)
//                {
//                    return true;
//                }
//            }
//            return false;
//        }
//        private static int FindWithoutRef(string Content, string Find, Dictionary<int, int> RefDict, int Start)
//        {
//            int pos = Start;
//            while (true)
//            {
//                if (pos > Content.Length - 1) break;
//                pos = Content.IndexOf(Find, pos);
//                if (pos < 0) return -1;
//                if (InString(RefDict, pos) == false) return pos;
//                else pos++;
//            }
//            return -1;
//        }

//        //public static string FormatJs(string JsContent)
//        //{
//        //    Dictionary<int, int> dict = new Dictionary<int, int>();
//        //    int pos = 0;
//        //    int next = 0;
//        //    while (true)
//        //    {
//        //        //没有处理 / / 表示的正则表达式。
//        //        pos = JsContent.IndexOfAny(@"""'".ToCharArray(), pos);

//        //        if (pos > -1)
//        //        {
//        //            next = GetNextClose(JsContent, JsContent[pos], pos + 1);

//        //            if (next > -1)
//        //            {
//        //                dict[pos] = next;
//        //                pos = next + 1;
//        //            }
//        //            else break;
//        //        }
//        //        else break;
//        //    }

//        //    // 处理 /**/
//        //    pos = 0;
//        //    next = 0;
//        //    while (true)
//        //    {
//        //        pos = FindWithoutRef(JsContent, "/*", dict, pos);
//        //        if (pos > -1)
//        //        {
//        //            next = FindWithoutRef(JsContent, "*/", dict, pos + 1);
//        //            if (next > -1)
//        //            {
//        //                JsContent = JsContent.Remove(pos, next - pos + 2);
//        //                pos = next + 1;
//        //            }
//        //            else break;
//        //        }
//        //        else break;
//        //    }

//        //    // 处理  //
//        //    pos = 0;
//        //    next = 0;
//        //    while (true)
//        //    {
//        //        pos = FindWithoutRef(JsContent, "//", dict, pos);
//        //        if (pos > -1)
//        //        {
//        //            next = FindWithoutRef(JsContent, Environment.NewLine, dict, pos + 1);
//        //            if (next > -1)
//        //            {
//        //                JsContent = JsContent.Remove(pos, next - pos);
//        //                pos = next + 1;
//        //            }
//        //            else break;
//        //        }
//        //        else break;
//        //    }


//        //    string SplitString = @"~!%^&*()-+=[]{}|:;<>,.?/";

//        //    var ms = Regex.Matches(JsContent, @"\s+", RegexOptions.Singleline | RegexOptions.Compiled);
//        //    List<string> list = new List<string>();
//        //    pos = 0;
//        //    foreach (Match item in ms)
//        //    {
//        //        if (item.Index == 0 || item.Index == JsContent.Length - 1) { pos = item.Index + item.Length; continue; }

//        //        if (
//        //            item.Index > 0 && item.Index < JsContent.Length - 1 &&
//        //            InString(dict, item.Index) == false &&
//        //            (SplitString.Contains(JsContent[item.Index - 1]) || SplitString.Contains(JsContent[item.Index + 1]))
//        //            )
//        //        {
//        //            list.Add(JsContent.Substring(pos, item.Index - pos));
//        //            pos = item.Index + item.Length;
//        //            continue;
//        //        }

//        //        list.Add(JsContent.Substring(pos, item.Index - pos));
//        //        list.Add(" ");
//        //        pos = item.Index + item.Length;
//        //    }
//        //    if (pos < JsContent.Length) list.Add(JsContent.Substring(pos));
//        //    return string.Join("", list.ToArray());


//        //    //List<char> list = JsContent.ToCharArray().ToList();


//        //    //for (int i = 0; i < list.Count; i++)
//        //    //{
//        //    //    if (list[i] == ' ' || list[i] == '\t')
//        //    //    {
//        //    //        if (i == 0 || i == list.Count - 1)
//        //    //        {
//        //    //            list.RemoveAt(i);
//        //    //            i--;
//        //    //        }
//        //    //        else if (InString(dict, i) == false)
//        //    //        {
//        //    //            if (SplitString.Contains(list[i - 1]) || SplitString.Contains(list[i + 1]))
//        //    //            {
//        //    //                list.RemoveAt(i);
//        //    //                i--;
//        //    //            }
//        //    //        }
//        //    //    }
//        //    //}

//        //    //return new string(list.ToArray());

//        //    //JsContent = string.Join(Environment.NewLine, JsContent.Split(Environment.NewLine).Where(o => o.Trim().StartsWith("//") == false).ToArray());

//        //    ////思路，把字符串分词，'|"|\b|\\。然后，固定 "",'',/**/ , //整行
//        //    //List<string> Sects = new List<string>();

//        //    ////1.分词。
//        //    //MatchCollection mc = Regex.Matches(JsContent, @"""|'|\b|\\", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

//        //    //int pos = 0;
//        //    //for (int i = 0; i < mc.Count; i++)
//        //    //{
//        //    //    Match m = mc[i];
//        //    //    //if (i == 0)
//        //    //    //{
//        //    //    //    Sects.Add(JsContent.Substring(pos, m.Index));
//        //    //    //    if (m.Value.HasValue()) Sects.Add(m.Value);
//        //    //    //}
//        //    //    //else
//        //    //    {
//        //    //        Sects.Add(JsContent.Substring(pos, m.Index - pos));
//        //    //        if (m.Value.HasValue()) Sects.Add(m.Value);

//        //    //        if (i == mc.Count - 1)
//        //    //        {
//        //    //            Sects.Add(JsContent.Substring(m.Index + m.Length));
//        //    //        }
//        //    //    }

//        //    //    pos = m.Index + m.Length;
//        //    //}

//        //    ////2.去空。
//        //    //Sects = Sects.Where(o => o.HasValue()).ToList();

//        //    //for (int i = 0; i < Sects.Count; i++)
//        //    //{
//        //    //    if (Sects[i].IsIn("'", @""""))
//        //    //    {
//        //    //        if (i == 0 || (i > 0 && Sects[i - 1] != @"\"))
//        //    //        {
//        //    //            i = HtmlParse.GetNextClose(Sects, Sects[i], i + 1);
//        //    //        }
//        //    //    }
//        //    //    else { Sects[i] = Regex.Replace(Sects[i].Replace(Environment.NewLine, " "), @"\s+", " ", RegexOptions.Compiled | RegexOptions.Singleline); }
//        //    //}
//        //    //return string.Join("", Sects.ToArray());
//        //}
//        //public static string FormatCss(string CssContent)
//        //{
//        //    return CssContent;
//        //}

//        private int GetCloseTag(int start, string tagName)
//        {
//            int endTagIndex = start;
//            while (true)
//            {
//                endTagIndex = GetNextMarkIndex(endTagIndex);
//                if (IsColseTag(endTagIndex, tagName))
//                {
//                    return endTagIndex;
//                }
//                else endTagIndex++;

//                if (endTagIndex >= Sects.Count) return Sects.Count;
//            }
//        }

//        /// <summary>
//        /// 是否是 关闭标签。
//        /// </summary>
//        /// <param name="endTagIndex"></param>
//        /// <param name="TagName"></param>
//        /// <returns></returns>
//        private bool IsColseTag(int endTagIndex, string TagName)
//        {
//            if (endTagIndex < Sects.Count && Sects[endTagIndex] == "</")
//            {
//                if (endTagIndex + 1 < Sects.Count && Sects[endTagIndex + 1].Trim().HasValue() == false) Sects.RemoveAt(endTagIndex + 1);

//                if (endTagIndex + 2 < Sects.Count && Sects[endTagIndex + 2].Trim().HasValue() == false) Sects.RemoveAt(endTagIndex + 2);

//                if (endTagIndex + 2 < Sects.Count && Sects[endTagIndex + 2] == ">")
//                {
//                    if (TagName.HasValue() == false) return true;

//                    if (Sects[endTagIndex + 1].Equals(TagName, StringComparison.CurrentCultureIgnoreCase))
//                    {
//                        return true;
//                    }
//                }
//            }
//            //if (endTagIndex > 0 && Sects[endTagIndex] == "/>") return true;

//            return false;
//        }

//        private int GetNextMarkIndex(int start)
//        {
//            for (int i = start; i < Sects.Count; i++)
//            {
//                if (Sects[i].IsIn("<", "/>", "</", ">")) return i;
//            }
//            return Sects.Count;
//        }

//        /// <summary>
//        /// 1.分词，2.合并文本，3.把 /&gt; 以及 &gt;/  合并。 
//        /// 所以 &lt; 一定是一个开始标记。
//        /// </summary>
//        /// <param name="Html"></param>
//        private void LoadToSect(string Html)
//        {
//            Sects = new List<string>();

//            //1.分词。
//            MatchCollection mc = Regex.Matches(Html, @"""|'|<|>|\b|/", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

//            int pos = 0;
//            for (int i = 0; i < mc.Count; i++)
//            {
//                Match m = mc[i];
//                //if (i == 0)
//                //{
//                //    Sects.Add(Html.Substring(pos, m.Index));
//                //    if (m.Value.HasValue()) Sects.Add(m.Value);
//                //}
//                //else
//                {
//                    Sects.Add(Html.Substring(pos, m.Index - pos));
//                    if (m.Value.HasValue()) Sects.Add(m.Value);

//                    if (i == mc.Count - 1)
//                    {
//                        Sects.Add(Html.Substring(m.Index + m.Length));
//                    }
//                }

//                pos = m.Index + m.Length;
//            }

//            //2.去空。
//            Sects = Sects.Where(o => o.HasValue()).ToList();

//            //3. 合并文本。
//            string beginChar = "";

//            int start = 0;
//            for (int i = 0; i < Sects.Count; i++)
//            {
//                if (Sects[i] == "<" && Sects[i + 1] == "!--")
//                {
//                    Sects[i] += Sects[i + 1];
//                    Sects.RemoveAt(i + 1);


//                    //上面之后 Sect[i] 变为：<!-- 
//                    while (true)
//                    {
//                        if ((i + 2 < Sects.Count) == false) break;

//                        if (Sects[i + 2] == ">" && Sects[i + 1].EndsWith("--"))
//                        {
//                            Sects[i] += Sects[i + 1];
//                            Sects.RemoveAt(i + 1);

//                            Sects[i] += Sects[i + 1];
//                            Sects.RemoveAt(i + 1);
//                            break;
//                        }
//                        else
//                        {
//                            Sects[i] += Sects[i + 1];
//                            Sects.RemoveAt(i + 1);
//                        }
//                    }

//                    continue;
//                }
//                else if (Sects[i] == "'" || Sects[i] == @"""")
//                {
//                    if (i > 0 && Sects[i - 1][Sects[i - 1].Length - 1] == '\\') continue;

//                    if (beginChar == Sects[i])
//                    {
//                        beginChar = "";

//                        for (int j = start; j < i; j++)
//                        {
//                            Sects[start] += Sects[start + 1];
//                            Sects.RemoveAt(start + 1);
//                        }
//                        i = start + 1;
//                    }
//                    else if (beginChar == "")
//                    {
//                        start = i;
//                        beginChar = Sects[i];
//                    }
//                    continue;
//                }
//            }

//            //合并结束标记 /> </ . 保证 <是开始标记。 >可能是是 打开和关闭的 结束标记。
//            for (int i = 0; i < Sects.Count; i++)
//            {
//                //<![CDATA[     ]]>
//                if (Sects[i] == "<" && Sects[i + 1] == "![" && Sects[i + 3][0] == '[' &&
//                    Sects[i + 2].Equals("CDATA", StringComparison.CurrentCultureIgnoreCase))
//                {
//                    Sects[i] += Sects[i + 1];
//                    Sects.RemoveAt(i + 1);

//                    Sects[i] += Sects[i + 1];
//                    Sects.RemoveAt(i + 1);

//                    Sects[i] += Sects[i + 1];
//                    Sects.RemoveAt(i + 1);

//                    while (true)
//                    {
//                        if ((i + 2 < Sects.Count) == false) break;

//                        if (Sects[i + 2] == ">" && Sects[i + 1].EndsWith("]]"))
//                        {
//                            Sects[i] += Sects[i + 1];
//                            Sects.RemoveAt(i + 1);

//                            Sects[i] += Sects[i + 1];
//                            Sects.RemoveAt(i + 1);
//                            break;
//                        }
//                        else
//                        {
//                            Sects[i] += Sects[i + 1];
//                            Sects.RemoveAt(i + 1);
//                        }
//                    }
//                }
//                // <!doctype>
//                else if (Sects[i] == "<" && Sects[i + 1] == "!" && Sects[i + 2] != "--")
//                {
//                    Sects[i] += Sects[i + 1];
//                    Sects.RemoveAt(i + 1);
//                    while (true)
//                    {
//                        if ((i + 1 < Sects.Count) == false) break;

//                        if (Sects[i + 1] == ">")
//                        {
//                            Sects[i] += Sects[i + 1];
//                            Sects.RemoveAt(i + 1);
//                            break;
//                        }
//                        else
//                        {
//                            Sects[i] += Sects[i + 1];
//                            Sects.RemoveAt(i + 1);
//                        }
//                    }
//                    continue;
//                }
//                else if (Sects[i] == "/" && Sects[i + 1] == ">")
//                {
//                    Sects[i] += Sects[i + 1];
//                    Sects.RemoveAt(i + 1);
//                    continue;
//                }
//                else if (Sects[i] == "<" && Sects[i + 1] == "/")
//                {
//                    Sects[i] += Sects[i + 1];
//                    Sects.RemoveAt(i + 1);
//                    continue;
//                }
//                else if (Sects[i] == "<" && char.IsLetter(Sects[i + 1][0]) == false)
//                {
//                    Sects[i] += Sects[i + 1];
//                    Sects.RemoveAt(i + 1);
//                    continue;
//                }
//            }
//        }
//    }
//}
