using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MyCmn
{
    public static class HtmlDomHelper
    {
        //public enum FindWordPositionEnum
        //{
        //    /// <summary>
        //    /// 匹配了Word。
        //    /// </summary>
        //    Finded = 1,

        //    /// <summary>
        //    /// 匹配了结束符
        //    /// </summary>
        //    Overed,
        //}

        public static string GetNextWord(string Source, int Start, string StartSkipChars = null, string EndSkipChars = null)
        {
            var list = new List<char>();
            bool progress = false;   // 0 未开始， 1 找到开始， 2 匹配了结束
            for (int i = Start; i < Source.Length; i++)
            {
                if (progress == false)
                {
                    if (IsVariaWord(Source[i], StartSkipChars))
                    {
                        continue;
                    }
                    else
                    {
                        progress = true;
                        list.Add(Source[i]);
                        continue;
                    }
                }
                else if (progress == true)
                {
                    if (IsVariaWord(Source[i], EndSkipChars))
                    {
                        break;
                    }
                    else
                    {
                        list.Add(Source[i]);
                        continue;
                    }
                }
            }

            return new string(list.ToArray());
        }

        /// <summary>
        /// 是否是要跳过处理的 杂物字符
        /// </summary>
        /// <param name="p"></param>
        /// <param name="SkipChars"></param>
        /// <returns></returns>
        private static bool IsVariaWord(char p, string SkipChars)
        {
            if (string.IsNullOrEmpty(SkipChars) == false)
            {
                return SkipChars.Contains(p);
            }
            else
            {
                if (char.IsLetterOrDigit(p)) return false;
                if (char.IsWhiteSpace(p)) return true;
                if (char.IsSymbol(p)) return true;
                if (char.IsPunctuation(p)) return true;
                if (char.IsSeparator(p)) return true;
                return false;
            }
        }

        /// <summary>
        /// 查找不在字符串中的下一个.
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Start"></param>
        /// <param name="Finds"></param>
        /// <returns></returns>
        public static int GetFirstNext(string Source, int Start, params char[] Finds)
        {
            for (int i = Start; i < Source.Length; i++)
            {
                i = IsStringAndJump(Source, i);
                if (i >= Source.Length) break;
                for (int j = 0; j < Finds.Length; j++)
                {
                    if (Source[i] == Finds[j]) return i;
                }
                //if (Finds.Contains(Html[i]))
                //{
                //    return i;
                //}
            }
            return Source.Length;
        }
        /// <summary>
        /// 查找下一个不在字符串中的匹配.
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Start"></param>
        /// <param name="IsJumpString"></param>
        /// <returns></returns>
        public static int GetNextNotNull(string Source, int Start, bool IsJumpString)
        {
            for (int j = Start; j < Source.Length; j++)
            {
                if (IsJumpString)
                {
                    j = IsStringAndJump(Source, j);
                }
                if (Source[j] == ' ') continue;
                if (Source[j] == '\t') continue;
                if (Source[j] == '\n') continue;
                if (Source[j] == '\r' && Source[j + 1] == '\n') continue;
                return j;
            }

            return Source.Length;
        }

        /// <summary>
        /// 如果当前是字符串,就跳过去.
        /// </summary>
        /// <param name="Html"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public static int IsStringAndJump(string Html, int Index)
        {
            char begin = Html[Index];
            if (Html[Index] == '\'' || Html[Index] == '"')
            {
                for (int i = Index + 1; i < Html.Length; i++)
                {
                    if (Html[i] == begin)
                    {
                        if (IsVerity(Html, i)) return i + 1;
                    }
                }
                return Html.Length;
            }
            else return Index;
        }

        public static int GetNext(string Source, char Find, int Start)
        {
            for (int i = Start; i < Source.Length; i++)
            {
                i = IsStringAndJump(Source, i);
                if (Source[i] == Find)
                {
                    if (IsVerity(Source, Start)) return i;
                }
            }
            return Source.Length;
        }

        public static bool IsVerity(string Source, int Index)
        {
            bool IsReal = true;

            for (int j = Index - 1; j >= 0; j++)
            {
                if (Source[j] != '\\') break;
                else IsReal = !IsReal;
            }
            return IsReal;
        }

        /// <summary>
        /// 不区分大小写.
        /// </summary>
        /// <param name="Find"></param>
        /// <param name="Start"></param>
        /// <returns></returns>
        public static int GetNextNoMatter(string Source, string Find, int Start)
        {
            if (Find.Length == 1) return GetNext(Source, Find[0], Start);
            bool SourceIsUpper = true;

            for (int i = Start; i < Source.Length; i++)
            {
                //中文isupper 和 islower 总返回 false.
                SourceIsUpper = Source[i].IsIn("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                bool IsEqual = true;
                for (int j = 0; j < Find.Length; j++)
                {
                    if (SourceIsUpper != Find[j].IsIn("ABCDEFGHIJKLMNOPQRSTUVWXYZ"))
                    {
                        if (char.ToLower(Source[i + j]) != char.ToLower(Find[j]))
                        {
                            IsEqual = false;
                            break;
                        }
                    }
                    else if (Source[i + j] != Find[j])
                    {
                        IsEqual = false;
                        break;
                    }
                }

                if (IsEqual) return i;
            }
            return Source.Length;

        }

    }

}
