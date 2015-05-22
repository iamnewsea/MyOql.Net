using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Linq;

namespace MyCmn
{
    public static class MyHelper
    {
        public static T GetOrDefault<T>(this Dictionary<string, T> dict, string key)
        {
            if (dict.ContainsKey(key)) return dict[key];
            else return default(T);
        }

        public static string AsString(this object obj)
        {
            if (object.Equals(obj, null)) return string.Empty;
            else return obj.ToString();
        }

        public static bool AsBool(this string value)
        {
            var result = false;
            bool.TryParse(value, out result);
            return result;
        }

        public static bool HasValue(this string Value)
        {
            return !string.IsNullOrEmpty(Value);
        }

        public static uint AsUInt(this int Vaule)
        {
            return Convert.ToUInt32(Vaule);
        }
        public static int AsInt(this double Vaule)
        {
            return Convert.ToInt32(Vaule);
        }

        public static int AsInt(this long Vaule)
        {
            return Convert.ToInt32(Vaule);
        }

        public static bool IsDBNull(this object Value)
        {
            if (object.Equals(Value, null)) return true;
            if (object.Equals(Value, DBNull.Value)) return true;
            return false;
        }

        /// <summary>
        /// 不区分大小写的比较两个字符.
        /// </summary>
        /// <param name="charValue"> </param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool EqualsNoMatter(this char charValue, char other)
        {
            if (charValue.Equals(other)) return true;
            if (char.IsLetter(charValue) && char.IsLetter(other))
            {
                return char.ToLower(charValue).Equals(char.ToLower(other));
            }
            else return false;
        }
        public static SimilarResult GetMaxCommString(string s1, string s2, bool compareWithCase)
        {
            if (String.IsNullOrEmpty(s1) || String.IsNullOrEmpty(s2))
            {
                return null;
            }
            else if (s1 == s2)
            {
                return new SimilarResult() { s1Index = 0, s2Index = 0, Value = s1 };
            }
            else if (compareWithCase == false && string.Equals(s1, s2, StringComparison.CurrentCultureIgnoreCase))
            {
                return new SimilarResult() { s1Index = 0, s2Index = 0, Value = s1 };
            }

            var rangData = new int[s1.Length, s2.Length];

            var maxLen = 0;

            for (int i = 0; i < s1.Length; i++)
            {
                for (int j = 0; j < s2.Length; j++)
                {
                    // 左上角值
                    var n = i - 1 >= 0 && j - 1 >= 0 ? rangData[i - 1, j - 1] : 0;

                    // 当前节点值 = "1 + 左上角值" : "0"
                    rangData[i, j] = (compareWithCase ? s1[i] == s2[j] : s1[i].EqualsNoMatter(s2[j])) ? 1 + n : 0;

                    // 如果是最大值，则记录该值和行号
                    if (rangData[i, j] > maxLen)
                    {
                        maxLen = rangData[i, j];
                    }
                }
            }

            if (maxLen == 0) { return null; }

            for (int i = 0; i < s1.Length; i++)
            {
                for (int j = 0; j < s2.Length; j++)
                {
                    // 如果是最大值，则记录该值和行号
                    if (rangData[i, j] == maxLen)
                    {
                        return new SimilarResult()
                        {
                            s1Index = i - maxLen + 1,
                            s2Index = j - maxLen + 1,
                            Value = s1.Substring(i - maxLen + 1, maxLen)
                        };

                    }
                }
            }
            return null;
        }


        /// <summary>
        /// 获取两个字符串相似度。（最大公约数法的扩展）
        /// </summary>
        /// <remarks>
        ///       人 民 共 和 时  代 华 人 国
        ///    中 0, 0, 0, 0, 0, 0, 0, 0, 0
        ///    华 0, 0, 0, 0, 0, 0, 1, 0, 0
        ///    人 1, 0, 0, 0, 0, 0, 0, 2, 0
        ///    民 0, 2, 0, 0, 0, 0, 0, 0, 0
        ///    共 0, 0, 3, 0, 0, 0, 0, 0, 0
        ///    和 0, 0, 0, 4, 0, 0, 0, 0, 0
        ///    国 0, 0, 0, 0, 0, 0, 0, 0, 1
        /// 
        /// 得到 人民共和  和 华人 两个单词
        /// 其中 人民共和 是最大公约数
        /// 按 人民共和 把两个词条分隔。 左左， 右右，再递归求最大公约数
        /// 参照长度 默认为： Max( s1.Length + s2.Length) 
        /// 相似度 =   最大公约数.Length / 最长参照长度   + 
        ///            （左相似度   +  右相似度） /100
        ///            
        /// 1  表示， 整体的相似度 介于 四个连字到五个连字之间。
        /// </remarks>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="compareWithCase">是否区分大小写</param>
        /// <param name="CompareLevelCount">比较次数，如果指定，则按指定的次数来。否则递归进行，最大递归次数64</param>
        /// <returns></returns>
        public static double GetSimilar(string s1, string s2, bool compareWithCase = false, int CompareLevelCount = 64)
        {
            if (string.IsNullOrEmpty(s1)) return 0.0;
            if (string.IsNullOrEmpty(s2)) return 0.0;

            if (CompareLevelCount <= 0) return 0.0;

            var allSimilar = new List<double>();

            var commStrResult = GetMaxCommString(s1, s2, compareWithCase);
            if (commStrResult == null) return 0.0;

            var leftSimilar = 0.0;
            if (commStrResult.s1Index > 0 && commStrResult.s2Index > 0)
            {
                leftSimilar = GetSimilar(s1.Substring(0, commStrResult.s1Index), s2.Substring(0, commStrResult.s2Index), compareWithCase, CompareLevelCount - 1);
            }
            var rightSimilar = 0.0;
            if (commStrResult.s1Index + commStrResult.Value.Length < s1.Length &&
                commStrResult.s2Index + commStrResult.Value.Length < s2.Length)
            {
                rightSimilar = GetSimilar(s1.Substring(commStrResult.s1Index + commStrResult.Value.Length), s2.Substring(commStrResult.s2Index + commStrResult.Value.Length), compareWithCase, CompareLevelCount - 1);
            }

            var maxLength = Math.Max(s1.Length, s2.Length) * 1.0;
            var theSimilar = commStrResult.Value.Length / maxLength +
                    (leftSimilar + rightSimilar) / 10.0;

            return theSimilar;
        }


    }
}
