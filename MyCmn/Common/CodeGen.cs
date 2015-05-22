using System;

namespace MyCmn
{
    /// <summary>
    /// 基于模板的代码生成.是 string.Format 的一个替代方案.
    /// </summary>
    public static partial class CodeGen
    {
        #region TidyPosition enum

        public enum TidyPosition
        {
            None,
            Begin,
            End,
            BeginEnd,
        }

        #endregion

        public const char LeftMark = '【';
        public const char RightMark = '】';

        public const char VarLeftMark = '$';
        public const char VarRightMark = '$';

        /// <summary>
        /// 如果Index 的后面是 空格和回车，则跳到回车后，否则返回Index
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        private static int JumpWithNullLine(this string Content, int Index)
        {
            string str = Content.Substring(Index).TrimStart(" ");
            if (str.StartsWith(Environment.NewLine))
            {
                return Content.IndexOf(Environment.NewLine, Index) + Environment.NewLine.Length;
            }
            return Index;
        }


        private static int GetNext(this string str, int Index, params string[] Find)
        {
            for (int i = Index; i < str.Length; i++)
            {
                if (NextIsIn(str, i, Find))
                {
                    return i;
                }
            }
            return str.Length;
        }

        private static bool NextIsIn(this string str, int Index, params string[] Find)
        {
            foreach (string item in Find)
            {
                bool Hitted = true;
                for (int i = 0; i < item.Length; i++)
                {
                    if (str[Index + i] != item[i])
                    {
                        Hitted = false;
                        break;
                    }
                }
                if (Hitted) return true;
            }
            return false;
        }


        /// <summary>
        /// 是否是 空白* + 回车开始.
        /// </summary>
        /// <param name="retVal"></param>
        /// <returns>返回值是空白*+回车的下一个字符索引. -1 表示非.</returns>
        private static int StartIsBlankEnter(string retVal)
        {
            //去除 空白* + 回车.
            for (int i = 0; i < retVal.Length; i++)
            {
                char c = retVal[i];
                if (c.IsIn('\t', ' ', '\r')) continue;

                if (c == '\n')
                {
                    if (i == retVal.Length - 1)
                    {
                        return -1;
                    }
                    else
                        return i + 1;
                }
                else break;
            }
            return -1;
        }

        /// <summary>
        /// 是否是 回车+ 空白* 结束.
        /// </summary>
        /// <param name="retVal"></param>
        /// <returns>返回值表示最后的回车索引.</returns>
        private static int EndIsEnterBlank(string retVal)
        {
            for (int i = retVal.Length - 1; i >= 0; i--)
            {
                char c = retVal[i];
                if (c.IsIn('\t', ' ')) continue;

                if (c == '\n')
                {
                    if (i > 0 && retVal[i - 1] == '\r')
                    {
                        return i - 1;
                    }
                    else return i;
                }
                else break;
            }

            return -1;
        }

        private static string Tidy(string str, TidyPosition TidyType)
        {
            return str;
        }
    }
}