using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCmn
{
    /// <summary>
    /// 基于模板的代码生成.是 string.Format 的一个替代方案.
    /// </summary>
    public static partial class CodeGen
    {
        #region IfLinker enum

        public enum IfLinker
        {
            None,
            And,
            Or,
        }

        #endregion

        #region IfOp enum

        public enum IfOp
        {
            None,
            Equal,
            NotEqual,
            MoreThen,
            LessThen,
            MareEqualThen,
            LessEqualThen,
        }

        #endregion

        #region TmpIfEnum enum

        public enum TmpIfEnum
        {
            none,
            @if,
            @eif,
            @else,
            @fi,
        }

        #endregion

        //public class TmpIfConditionSect
        //{
        //    public string Value { get; set; }
        //    public bool IsVar { get; set; }
        //}

        /// <summary>
        /// 一个语句里， 只能出现一个 TmpIf 。而且，要先进行 TmpFor ，然后进行 TmpIf ， 最后是 Replace
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static TmpIfSect TmpIf(this string str)
        {
            //简化规则,只删除前面的空行.
            //str = Regex.Replace(str, string.Format(@"\r?\n[\t\x20]*({0}if:.*?{1})", LeftMark, RightMark), @"$1", RegexOptions.Compiled);
            //str = Regex.Replace(str, string.Format(@"\r?\n[\t\x20]*({0}eif:.*?{1})", LeftMark, RightMark), @"$1", RegexOptions.Compiled);
            //str = Regex.Replace(str, string.Format(@"\r?\n[\t\x20]*({0}else{1})", LeftMark, RightMark), @"$1", RegexOptions.Compiled);
            //str = Regex.Replace(str, string.Format(@"\r?\n[\t\x20]*({0}fi{1})", LeftMark, RightMark), @"$1", RegexOptions.Compiled);


            var retVal = new TmpIfSect();
            string ifSect = LeftMark.ToString() + TmpIfEnum.@if.ToString() + ":";

            //算法同For,也分为三段,开始,If判断,结尾
            int begin = str.IndexOf(ifSect);

            //结尾部分
            if (begin == -1)
            {
                retVal.Text = str;
                return retVal;
            }
            //开始部分.
            else if (begin > 0)
            {
                retVal.Text = str.Substring(0, begin);
                retVal.Next = TmpIf(str.Substring(begin));
                return retVal;
            }

            //If判断部分.
            string elseIfSect = LeftMark.ToString() + TmpIfEnum.@eif.ToString() + ":";
            string elseSect = LeftMark.ToString() + TmpIfEnum.@else.ToString() + RightMark.ToString();
            string endIfSect = LeftMark.ToString() + TmpIfEnum.fi.ToString() + RightMark.ToString();

            int level = 0;
            int curPos = 0;
            int expStartIndex = curPos;
            TmpIfEnum preIf = TmpIfEnum.none;
            while (true)
            {
                int beginIndex = GetNext(str, curPos,
                                         LeftMark.ToString() + TmpIfEnum.@if.ToString() + ":",
                                         LeftMark.ToString() + TmpIfEnum.eif.ToString() + ":",
                                         LeftMark.ToString() + TmpIfEnum.@else.ToString() + RightMark.ToString(),
                                         LeftMark.ToString() + TmpIfEnum.fi.ToString() + RightMark.ToString()
                    );

                int endIndex = str.IndexOf(RightMark, beginIndex);
                GodError.Check(endIndex < 0,  "找不到匹配标签,请检查If结构是否正常,(" + str.Slice(0, 20) + ")");
                string exp = str.Substring(expStartIndex, beginIndex - expStartIndex);

                if (str.IndexOf(ifSect, beginIndex) == beginIndex)
                {
                    level++;
                    if (level == 1)
                    {
                        //if (str.StartsWith(ifSect + RightMark))
                        //{
                        //    //retVal.Ifs.Add(GetIfCondition("", null));
                        //}
                        //else
                        {
                            string con = str.Substring(beginIndex + ifSect.Length,
                                                       endIndex - (beginIndex + ifSect.Length));
                            retVal.Ifs.Add(GetIfCondition(con, null));
                        }
                        expStartIndex = endIndex + 1;
                        preIf = TmpIfEnum.@if;
                    }
                }
                else if (str.IndexOf(endIfSect, beginIndex) == beginIndex)
                {
                    level--;
                    if (level == 0)
                    {
                        if (retVal.Ifs.Count > 0)
                        {
                            switch (preIf)
                            {
                                case TmpIfEnum.@if:
                                    retVal.Ifs.Last().Expression = TmpIf(exp);
                                    break;
                                case TmpIfEnum.eif:
                                    retVal.Ifs.Last().Expression = TmpIf(exp);
                                    break;
                                case TmpIfEnum.@else:
                                    retVal.Else = TmpIf(exp);
                                    break;
                                case TmpIfEnum.fi:
                                    break;
                                default:
                                    break;
                            }
                        }
                        expStartIndex = endIndex + 1;
                        preIf = TmpIfEnum.@fi;
                        retVal.GetLast().Next = TmpIf(str.Substring(endIndex + 1));
                        break;
                    }
                }
                else if (str.IndexOf(elseIfSect, beginIndex) == beginIndex)
                {
                    if (level == 1)
                    {
                        switch (preIf)
                        {
                            case TmpIfEnum.@if:
                                retVal.Ifs.Last().Expression = TmpIf(exp);
                                break;
                            case TmpIfEnum.eif:
                                retVal.Ifs.Last().Expression = TmpIf(exp);
                                break;
                            case TmpIfEnum.@else:
                                break;
                            case TmpIfEnum.fi:
                                break;
                            default:
                                break;
                        }

                        string con = str.Substring(beginIndex + elseIfSect.Length,
                                                   endIndex - (beginIndex + elseIfSect.Length));
                        retVal.Ifs.Add(GetIfCondition(con, null));

                        expStartIndex = endIndex + 1;
                        preIf = TmpIfEnum.@eif;
                    }
                }
                else if (str.IndexOf(elseSect, beginIndex) == beginIndex)
                {
                    if (level == 1)
                    {
                        switch (preIf)
                        {
                            case TmpIfEnum.@if:
                                retVal.Ifs.Last().Expression = TmpIf(exp);
                                break;
                            case TmpIfEnum.eif:
                                retVal.Ifs.Last().Expression = TmpIf(exp);
                                break;
                            case TmpIfEnum.@else:
                                break;
                            case TmpIfEnum.fi:
                                break;
                            default:
                                break;
                        }

                        expStartIndex = endIndex + 1;
                        preIf = TmpIfEnum.@else;
                    }
                }

                curPos = endIndex + 1;
            }

            return retVal;
        }

        private static TmpIfCondition GetIfCondition(string ConditionText, TmpIfSect Expression)
        {
            var retVal = new TmpIfCondition();
            retVal.Expression = Expression;
            retVal.Condition = ConditionText;
            return retVal;
        }

        public static TmpIfSect Var(this TmpIfSect sect, string ifName, Func<bool> func)
        {
            sect.Vars[ifName] = func().AsString();
            sect.SetDownVar(ifName);
            return sect;
        }

        public static string EndIf(this TmpIfSect sect)
        {
            return sect.ToString();
        }

        #region Nested type: TmpIfCondition

        /// <summary>
        /// If 条件表达式.
        /// </summary>
        public class TmpIfCondition
        {
            public TmpIfCondition()
            {
                //this.Var = new TmpIfConditionSect();
                //this.Value = new TmpIfConditionSect();
                Expression = new TmpIfSect();
            }

            public bool? IsTrue ;
            public string Condition { get; set; }
            //public IfOp Op { get; set; }
            //public TmpIfConditionSect Value { get; set; }

            //public TmpIfCondition SubCondition { get; set; }

            //public IfLinker Linker { get; set; }
            //public TmpIfCondition Next { get; set; }

            //public bool IsTrue { get; set; }

            public TmpIfSect Expression { get; set; }
        }

        #endregion

        #region Nested type: TmpIfSect

        /// <summary>
        /// 包含所有的If节点.
        /// </summary>
        /// <remarks>
        /// If节点的条件是一个表达式.
        /// @强制表示是一个变量. 像Sqlserver 的变量标识符.
        /// true , false , 1 , 2.5 , "Hello" 表示数值.
        /// &amp;  表示 逻辑与
        /// |  表示 逻辑并
        /// </remarks>
        /// <example><code>
        ///【if:base】Base【eif:ww=='Dict' &amp; i&gt;0】erer【else】!!【fi】
        ///【if:base】Base【eif:@2&gt;oo】er
        /// ------------------------------------------
        ///【if:base】Base【eif:@2&gt;oo】erer【else】!!【fi】
        /// ------------------------------------------
        /// er【else】!!【fi】
        /// </code></example>
        [Serializable]
        public class TmpIfSect
        {
            public TmpIfSect()
            {
                Ifs = new List<TmpIfCondition>();
                Vars = new Dictionary<string, string>();
            }

            /// <summary>
            /// 如果存在Text 说明是文本域.
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// 条件为真的文本内容。
            /// </summary>
            public List<TmpIfCondition> Ifs ;

            //public int TrueIndex { get; set; }
            /// <summary>
            /// 条件为假的文本内容。
            /// </summary>
            public TmpIfSect Else { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public TmpIfSect Next { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public Dictionary<string, string> Vars ;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="varName"></param>
            public void SetDownVar(string varName)
            {
                Ifs.All(o =>
                            {
                                if (o.Expression.Vars.ContainsKey(varName) == false)
                                {
                                    o.Expression.Vars[varName] = Vars[varName];
                                }
                                if (o.IsTrue.HasValue == false && (o.Condition == varName))
                                {
                                    o.IsTrue = Decide(o.Condition, o.Expression.Vars);
                                }
                                o.Expression.SetDownVar(varName);
                                return true;
                            });
                if (Else != null)
                {
                    if (Else.Vars.ContainsKey(varName) == false)
                    {
                        Else.Vars[varName] = Vars[varName];

                        Else.SetDownVar(varName);
                    }
                }

                TmpIfSect curIf = Next;

                if (curIf != null)
                {
                    if (curIf.Vars.ContainsKey(varName) == false)
                    {
                        curIf.Vars[varName] = Vars[varName];
                    }

                    curIf.SetDownVar(varName);
                }
            }

            private bool? Decide(string p, Dictionary<string, string> Vars)
            {
                if (Vars.ContainsKey(p))
                    return Vars[p].AsBool();

                return null;
                //throw new GodError("没有定义变量:" + p);
            }

            public override string ToString()
            {
                // Var 变量推到 Next 和 Else 下一级。

                var sl = new StringLinker();
                if (Text.HasValue())
                {
                    string strText = Text;

                    sl += strText;
                }
                else if (Ifs.Count == 0)
                {
                    return string.Empty;
                }
                else
                {
                    bool had = false;
                    bool Hited = false;

                    Ifs.All(o =>
                                {
                                    if (o.IsTrue.HasValue)
                                    {
                                        Hited = true;
                                        return false;
                                    }
                                    return true;
                                });

                    if (Hited == false)
                    {
                        sl += Text;
                        bool isFirstIf = true;
                        foreach (TmpIfCondition item in Ifs)
                        {
                            sl += LeftMark + (isFirstIf ? "" : "e") + "if:" + item.Condition + RightMark;
                            sl += item.Expression;
                            if (isFirstIf)
                            {
                                isFirstIf = false;
                            }
                        }

                        if (Else != null)
                        {
                            sl += LeftMark + "else" + RightMark;
                            sl += Else.AsString();
                        }

                        sl += LeftMark + "fi" + RightMark;
                    }
                    else
                    {
                        foreach (TmpIfCondition item in Ifs)
                        {
                            if (item.IsTrue.AsBool())
                            {
                                sl += item.Expression.ToString();
                                had = true;
                                break;
                            }
                        }

                        if (had == false)
                        {
                            sl += Else.AsString();
                        }
                    }
                }
                if (Next != null) sl += Next.ToString();

                return sl;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public TmpIfSect GetLast()
            {
                if (Next == null) return this;
                return Next.GetLast();
            }
        }

        #endregion
    }
}