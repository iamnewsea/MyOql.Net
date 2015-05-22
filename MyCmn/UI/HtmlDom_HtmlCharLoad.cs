using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCmn
{
    public class HtmlCharLoad
    {
        /// <summary>
        /// Release下万次　780 毫秒．超强.
        /// </summary>
        /// <param name="Html"></param>
        public HtmlCharLoad(char[] Html)
        {
            this.Html = Html;
            Nodes = new List<HtmlNode>();
        }

        #region 主处理函数.

        public List<HtmlNode> Load(HtmlNodeProc.ProcType PType)
        {
            int i = GetNextNotNull(0);
            if (IsValue(i, "<"))
            {
                if (i > 0)
                {
                    var txt = new HtmlTextNode();
                    txt.Text = new string(Html, 0, i);
                    Nodes.Add(txt);
                }

                for (; i < Html.Length; i++)
                {
                    if (i < Html.Length - 1 && Html[i] == '<' && !char.IsWhiteSpace(Html[i + 1]))
                    {
                        i = ProcTag(i);
                        continue;
                    }
                    else
                    {
                        i = ProcText(i);
                        continue;
                    }
                }
                var np = new HtmlNodeProc(Nodes);
                np.Proc(PType);
                return np.Nodes;
            }
            //如果是Ajax 返回的格式, 不做处理.
            else
            {
                var txt = new HtmlTextNode();
                txt.Text = new string(Html);
                Nodes.Add(txt);
                return Nodes;
            }
        }


        public List<CmdArgNode> LoadCmdArgs(char CmdSign)
        {
            Func<int, int, CmdArgNode> GetCmdArg = (beginIndex, endIndex) =>
                                                       {
                                                           int cmdStart = GetFirstNext(beginIndex, CmdSign);
                                                           int cmdEnd = GetFirstNext(cmdStart, ' ', '\r', '\n', '\t') -
                                                                        1;

                                                           var node = new CmdArgNode
                                                                          {
                                                                              Cmd =
                                                                                  Html.Slice(cmdStart + 1, cmdEnd + 1).
                                                                                  AsString().Trim()
                                                                          };

                                                           var args = new List<string>();

                                                           cmdEnd += 2;

                                                           for (int k = cmdEnd; k < endIndex; k++)
                                                           {
                                                               int opNext = GetFirstNext(k, ' ', '\r', '\n', '\t') - 1;

                                                               args.Add(
                                                                   Html.Slice(k, opNext + 1).AsString()
                                                                   .Replace(@"\""", @""""));

                                                               k = opNext + 1;
                                                           }

                                                           node.Args = args.Select(o => o.TrimWithPair(@"""", @"""")).ToArray();
                                                           return node;
                                                       };


            var list = new List<CmdArgNode>();

            int i = 0, j = 0;
            i = GetNextNotNull(i);
            j = GetFirstNext(i, CmdSign, ' ', '\n', '\r', '\t');

            if (j < 0)
            {
                j = Html.Length;
            }

            var cmdPrefix = Html.Slice(i, j).AsString().Trim().Replace(@"\""", @"""");

            if (cmdPrefix.HasValue())
            {
                list.Add(new CmdArgNode
                             {
                                 Cmd = cmdPrefix,
                                 Args = new string[0]
                             });

                i = j + 1;
            }


            for (; i < Html.Length; i++)
            {
                i = GetNextNotNull(i);
                i = GetFirstNext(i, CmdSign);
                j = GetFirstNext(i + 1, CmdSign) - 1;


                list.Add(GetCmdArg(i, j));

                i = j;
            }

            return list;
        }

        private int ProcTag(int Start)
        {
            int i = Start + 1;

            i = GetNextNotNull(i);
            int pos = 0;
            if (Html[i] == '/')
            {
                i = GetNext('>', i + 1);
                var close = new HtmlCloseTagNode();
                close.TagName = new string(Html, Start + 2, i - (Start + 2));
                Nodes.Add(close);
                return i;
            }
            else if (IsValue(i, "!--")) //Html[i] == '!' && Html[i + 1] == '-' && Html[i + 2] == '-')
            {
                i = GetNext("-->", i);


                Nodes.Add(new HtmlNoteNode { Text = new string(Html, Start, i + 3 - Start) });
                i = i + 2;
                return i;
            }
            else if (IsValue(i, "!"))
            {
                i = GetNext(">", i);


                Nodes.Add(new HtmlTextNode { Text = new string(Html, Start, i + 1 - Start) });
                return i;
            }
            else
            {
                i = GetNext('>', Start + 2);
                pos = GetFirstNext(Start, ' ', '\t', '\r', '\n', '>');
                var tag = new HtmlTagNode();
                tag.TagName = new string(Html, Start + 1, pos - Start - 1);
                Nodes.Add(tag);
                if (Html[i - 1] == '/') tag.IsSole = true;

                tag.Attrs.AddRange(GetAttr(pos, tag.IsSole ? i - 1 : i));

                if (tag.TagName.IsIn(StringComparer.CurrentCultureIgnoreCase, "script", "style", "textarea", "pre"))
                {
                    if (tag.IsSole == false)
                    {
                        pos = GetNextNoMatter("</" + tag.TagName + ">", i);
                        if (pos - (i + 1) > 0)
                        {
                            Nodes.Add(new HtmlTextNode { Text = new string(Html, i + 1, pos - (i + 1)) });
                        }

                        Nodes.Add(new HtmlCloseTagNode { TagName = tag.TagName });

                        i = pos + 2 + tag.TagName.Length;
                    }
                }
                return i;
            }
        }

        private List<HtmlAttrNode> GetAttr(int start, int end)
        {
            int pos = 0;
            var list = new List<HtmlAttrNode>();
            for (int i = start; i < end; i++)
            {
                i = GetNextNotNull(i);

                pos = GetFirstNext(i, '=', ' ', '\t', '\r', '\n', '/', '>');
                if (pos <= i) continue;
                var atr = new HtmlAttrNode();

                atr.Name = new string(Html, i, pos - i);

                i = GetNextNotNull(pos);
                if (Html[i] == '=')
                {
                    //找属性值。   有多种情况。
                    //1.带有引号。 后面有空格。        绝对标准。
                    //2.带有引号。 后面是 >
                    //3.带有引号。 后面是 />           ------
                    //4.不带引号。 后面有空格。
                    //5.不带引号。 后面是>结束符。
                    //6.不带引号。 后面是/>结束符。 

                    pos = GetFirstNext(i, ' ', '\t', '\r', '\n', '>', '/');

                    if (Html[pos] == '/' && !IsValue(pos, "/>"))
                    {
                        pos = GetFirstNext(i, ' ', '\t', '\r', '\n', '>');
                    }

                    var strVal = new string(Html, i + 1, pos - i - 1);

                    if (strVal.Length >= 2)
                    {
                        if (strVal[0] == '"' && strVal[strVal.Length - 1] == '"')
                        {
                            atr.ValueQuote = '"';
                            atr.Value = strVal.Substring(1, strVal.Length - 2);
                        }
                        else if (strVal[0] == '\'' && strVal[strVal.Length - 1] == '\'')
                        {
                            atr.ValueQuote = '\'';
                            atr.Value = strVal.Substring(1, strVal.Length - 2);
                        }
                        else
                        {
                            atr.Value = strVal;
                        }
                    }
                    else { atr.Value = strVal; }

                    i = pos;
                }
                else
                {
                    atr.Value = null;
                    i--;
                }

                list.Add(atr);
            }
            return list;
        }


        private int ProcText(int Start)
        {
            // Text 里的内容, 是不分 是否在 string 里的.
            int pos = 0;
            //简单处理文本，当仅出现一个   < 号时，则再继续查找。
            while (true)
            {
                for (int i = Start + 1; i < Html.Length - 1; i++)
                {
                    if (Html[i] == '<' && !char.IsWhiteSpace(Html[i + 1]))
                    {
                        pos = i;
                        break;
                    }
                }

                if (pos == 0 || pos >= Html.Length - 1)
                {
                    pos = Html.Length;
                    break;
                }

                if (!char.IsWhiteSpace(Html[pos + 1])) break;
            }

            //while (true)
            //{
            //    pos++;
            //    for (; pos < Html.Length; pos++)
            //    {
            //        if (Html[pos] == '<')
            //        {
            //            if (IsVerity(pos))
            //            {
            //                break;
            //            }
            //        }
            //    }

            //    if (pos >= Html.Length - 1) break;
            //    if (Html[pos + 1] == '/') break;
            //    if (Html[pos + 1] == '!' && Html[pos + 2] == '-' && Html[pos + 3] == '-') break;

            //    if (char.IsLetter(Html[pos + 1])) break;
            //}
            //for (pos = Start; pos < Html.Length; pos++)
            //{
            //    pos = GetNextNotNull(pos);

            //    if (Html[pos] == '<')
            //    {
            //        pos = pos - 1;
            //        break;
            //    }
            //}
            //进行到下一个字符.
            var strTxt = new string(Html, Start, pos - Start);
            Nodes.Add(new HtmlTextNode { Text = strTxt });

            //if (pos < Html.Length - 1)
            //{
            //}
            //else
            //{
            //    Nodes.Add(new HtmlTextNode() { Text = new string(Html, Start, Html.Length - 1 - Start) });
            //}
            return pos - 1;
            //if (pos == Html.Length - 1) 
            //else return pos - 1;
        }

        /// <summary>
        /// 
        /// </summary>
        public class CmdArgNode
        {
            /// <summary>
            /// 
            /// </summary>
            public string Cmd;
            /// <summary>
            /// 
            /// </summary>
            public string[] Args;
        }

        #endregion

        #region HtmlCharLoad API

        //private bool IsValue(int Start, string Value, StringComparison Compare)
        //{
        //    return new string(Html, Start, Value.Length).Equals(Value, Compare);
        //}

        private bool IsValueNoMatter(int Start, string Find)
        {
            if (Start + Find.Length >= Html.Length) return false;

            for (int i = 0; i < Find.Length; i++)
            {
                if (Html[Start + i].EqualsNoMatter(Find[i]) == false) return false;
            }
            return true;
        }

        /// <summary>
        /// 判断当前位置开始的值是否是某值。
        /// </summary>
        /// <param name="start"></param>
        /// <param name="find"></param>
        /// <returns></returns>
        private bool IsValue(int start, string find)
        {
            if (start + find.Length >= Html.Length) return false;

            for (int i = 0; i < find.Length; i++)
            {
                if (Html[start + i] != find[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// 如果当前是字符串,就跳过去.
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        private int IsStringAndJump(int Index)
        {
            if (Index >= Html.Length) return Html.Length;

            char begin = Html[Index];
            if (Html[Index] == '\'' || Html[Index] == '"')
            {
                for (int i = Index + 1; i < Html.Length; i++)
                {
                    if (Html[i] == begin)
                    {
                        if (IsVerity(i)) return i + 1;
                    }
                }
                return Html.Length;
            }
            else return Index;
        }

        /// <summary>
        /// 是否是真实的.而非转义的.仅判断前面临近的字符是否是转义字符。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool IsVerity(int index)
        {
            bool isReal = true;

            //之前是 j++ , 改为 j-- . udi . 2011-8-9
            for (int j = index - 1; j >= 0; j--)
            {
                if (Html[j] != '\\') break;
                else isReal = !isReal;
            }
            return isReal;
        }

        /// <summary>
        /// 查找在字符串中的下一个.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="finds"></param>
        /// <returns></returns>
        private int GetFirstNext(int start, params char[] finds)
        {
            for (int i = start; i < Html.Length; i++)
            {
                i = IsStringAndJump(i);
                if (i >= Html.Length) break;
                for (int j = 0; j < finds.Length; j++)
                {
                    if (Html[i] == finds[j]) return i;
                }
            }
            return Html.Length;
        }

        /// <summary>
        /// 区分大小写.
        /// </summary>
        /// <param name="find"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        private int GetNext(string find, int start)
        {
            if (find.Length == 1) return GetNext(find[0], start);

            for (int i = start; i < Html.Length; i++)
            {
                bool isEqual = true;
                for (int j = 0; j < find.Length; j++)
                {
                    if (Html[i + j] != find[j])
                    {
                        isEqual = false;
                        break;
                    }
                }

                if (isEqual) return i;
            }
            return Html.Length;
        }

        /// <summary>
        /// 不区分大小写.不考虑字符串内.
        /// </summary>
        /// <param name="find"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        private int GetNextNoMatter(string find, int start)
        {
            if (find.Length == 1) return GetNext(find[0], start);

            for (int i = start; i < Html.Length; i++)
            {
                //中文isupper 和 islower 总返回 false.
                bool sourceIsLetter = char.IsLetter(Html[i]);
                if (sourceIsLetter)
                {
                }

                bool isEqual = true;
                for (int j = 0; j < find.Length; j++)
                {
                    if (char.IsLetter(find[j]) == sourceIsLetter && char.ToLower(find[j]) != char.ToLower(Html[i + j]))
                    {
                        {
                            isEqual = false;
                            break;
                        }
                    }
                    else if (find[j] != Html[i + j])
                    {
                        isEqual = false;
                        break;
                    }
                }

                if (isEqual) return i;
            }
            return Html.Length;
        }


        /// <summary>
        /// 查找下一个不在字符串中的匹配..
        /// </summary>
        /// <param name="find"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        private int GetNext(char find, int start)
        {
            for (int i = start; i < Html.Length; i++)
            {
                i = IsStringAndJump(i);

                if (i >= Html.Length)
                {
                    throw new GodError("Html解析出错，可能出现未闭合的字符串，请检查：" + Html.Slice((start < 5 ? 0 : start - 5), start + 32).AsString());
                }

                if (Html[i] == find)
                {
                    if (IsVerity(i)) return i;
                }
            }
            return Html.Length;
        }

        /// <summary>
        /// 查找下一个不在字符串中的匹配.
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        private int GetNextNotNull(int start)
        {
            for (int j = start; j < Html.Length; j++)
            {
                j = IsStringAndJump(j);

                if (j >= Html.Length) return Html.Length;

                if (Html[j] == ' ') continue;
                if (Html[j] == '\t') continue;
                if (Html[j] == '\n') continue;
                if (Html[j] == '\r' && Html[j + 1] == '\n') continue;
                return j;
            }

            return Html.Length;
        }

        #endregion

        private char[] Html;
        private List<HtmlNode> Nodes;
    }
}