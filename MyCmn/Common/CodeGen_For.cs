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
        #region TmpForEnum enum

        public enum TmpForEnum
        {
            @for,
            endfor
        }

        #endregion

        private static TmpForSect<T> Tmp_For<T>(this string str, string forName, IEnumerable<T> source)
        {
            var retVal = new TmpForSect<T>();
            string forBegin = LeftMark.ToString() + "for:" + forName + RightMark.ToString();
            string forEnd = LeftMark.ToString() + "endfor" + RightMark.ToString();

            //找开始索引,这很重要,把结果分成三部分,开始部分, 循环部分,结尾部分.
            int begin = str.IndexOf(forBegin, 0);

            //结尾部分
            if (begin == -1)
            {
                retVal.Text = str.Substring(0);
                return retVal;
            }
            //开头部分
            else if (begin > 0)
            {
                retVal.Text = Tidy(str.Substring(0, begin), TidyPosition.End);
                retVal.Next = Tmp_For(str.Substring(begin), forName, source);
                return retVal;
            }
            //剩下的是循环部分了.
            //Level 表示嵌套级别
            int Level = 0;
            int curPos = 0;
            while (true)
            {
                string forSect = LeftMark.ToString() + "for:";
                int nextIndex = GetNext(str, curPos, forSect, forEnd);

                if (nextIndex < 0 || nextIndex >= str.Length)
                {
                    break;
                }

                //如果下一个是新的 for
                if (str.IndexOf(forBegin, nextIndex) == nextIndex)
                {
                    if (Level == 0)
                    {
                        retVal.forName = forName;
                        retVal.Source = source;
                        begin = forBegin.Length;
                    }
                    Level++;
                    curPos = nextIndex + forSect.Length;
                }
                else if (str.IndexOf(forSect, nextIndex) == nextIndex)
                {
                    Level++;
                    curPos = nextIndex + forSect.Length;
                }
                else if (str.IndexOf(forEnd, nextIndex) == nextIndex)
                {
                    Level--;
                    curPos = nextIndex + forEnd.Length;
                }

                //当遇到第一个闭合endfor时.
                if (Level == 0)
                {
                    TmpForSect<T> subSect = Tmp_For(str.Substring(begin, nextIndex - begin), forName, source);
                    retVal.Source.All(o =>
                                          {
                                              var cloneOne = subSect.Clone() as TmpForSect<T>;
                                              cloneOne.Each = o;
                                              retVal.Templates = retVal.Templates.AddOne(cloneOne);
                                              return true;
                                          });

                    retVal.GetLast().Next = Tmp_For(str.Substring(curPos), forName, source);
                    break;
                }
            }

            GodError.Check(Level > 0, "发现没有闭合的for循环,循环变量: " + forName + "!");
            GodError.Check(Level < 0, "发现没有多余的for循环,循环变量: " + forName + "!");
            return retVal;
        }

        /// <summary>
        /// 开始循环。轻量级模板语言，不支持嵌套
        /// </summary>
        /// <remarks>
        /// 移除每段的最后一个回车.
        /// </remarks>
        /// <param name="str"></param>
        /// <param name="forName"></param>
        /// <param name="source"></param>
        /// <returns></returns>  
        public static TmpForIfSect<T> TmpFor<T>(this string str, string forName, IEnumerable<T> source)
        {
            //简化规则,只删除前面的空行.
            //str = Regex.Replace(str, string.Format(@"\r?\n[\t\x20]*({0}for:.*?{1})", LeftMark, RightMark), @"$1", RegexOptions.Compiled);
            //str = Regex.Replace(str, string.Format(@"\r?\n[\t\x20]*({0}endfor{1})", LeftMark, RightMark), @"$1", RegexOptions.Compiled);


            return new TmpForIfSect<T>(Tmp_For(str, forName, source));
        }

        /// <summary>
        /// 嵌套循环。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static TmpForSect<T> DoEach<T>(this TmpForSect<T> result, Func<T, string, string> func)
            where T : class
        {
            if (result.Text.HasValue() && result.Each != null)
            {
                result.Text = func(result.Each, result.Text);
            }
            if (result.Source != null && result.Source.Any())
            {
                //先循环内部
                foreach (var subSect in result.Templates)
                {
                    subSect.DoEach(func);
                }
            }

            //再循环链.

            if (result.Next != null)
                result.Next.DoEach(func);

            return result;
        }

        public static TmpForSect<T> DoFor<T>(this TmpForSect<T> result, string forKey, Func<T, string> func)
            where T : class
        {
            //if (result.Each.HasValue())
            if (result.Text.HasValue() && result.Each != null)
            {
                if (result.Text.Contains(VarLeftMark.ToString() + forKey + VarRightMark.ToString()))
                {
                    result.Text = result.Text.Replace(VarLeftMark.ToString() + forKey + VarRightMark.ToString(),
                                                      func(result.Each));
                }
            }

            if (result.Source != null && result.Source.Any())
            {
                //先循环内部
                foreach (var subSect in result.Templates)
                {
                    subSect.DoFor(forKey, func);
                }
            }

            //再循环链.

            if (result.Next != null)
                result.Next.DoFor(forKey, func);

            return result;
        }

        /// <summary>
        ///  DoIf 在 DO 循环里只能调用一次。 这个函数只是为了简化调用方式。
        /// </summary>
        public static TmpForIfSect<T> DoIf<T>(this TmpForSect<T> result, Func<T, bool> func)
        {
            return result.DoIf(new DoIfObject<T>(string.Empty, func));
        }

        public static TmpForIfSect<T> DoIf<T>(this TmpForSect<T> result, string IfName, Func<T, bool> func)
        {
            return result.DoIf(new DoIfObject<T>(IfName, func));
        }

        /// <summary>
        /// 仅一级判断
        /// </summary>
        /// <param name="result"></param>
        /// <param name="funcs"> </param>
        /// <returns></returns>
        public static TmpForIfSect<T> DoIf<T>(this TmpForSect<T> result, params DoIfObject<T>[] funcs)
        {
            return new TmpForIfSect<T>(result).DoIf(funcs);
        }

        /// <summary>
        /// For 里面的 每一个 IF . 不能嵌套.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="funcs"> </param>
        private static void ProcEachIf<T>(TmpForSect<T> item, params DoIfObject<T>[] funcs)
        {
            if (item.Text.HasValue() == false)
                return;

            string strItem = item.Text;

            strItem = strItem.Replace(LeftMark.ToString() + "if" + RightMark.ToString(),
                                      LeftMark.ToString() + "if:" + RightMark.ToString());


            TmpIfSect tmpIf = strItem.TmpIf();
            funcs.All(o =>
                          {
                              tmpIf = tmpIf.Var(o.VarName, () => o.EachFunc(item.Each));
                              return true;
                          });

            item.Text = tmpIf.EndIf();
        }


        //public static void End_For(this TmpForSect result)
        //{
        //    //只处理每个For前及EndFor后的回车.
        //    var curOne = result;
        //    while (true)
        //    {
        //        if (curOne == null) break;
        //        if (curOne.Text.HasValue())
        //        {
        //            curOne.Text = Tidy(curOne.Text, TidyPosition.End);
        //        }
        //        else
        //        {
        //            foreach (var item in curOne.Templates)
        //            {
        //                item.End_For();
        //            }
        //            curOne = curOne.Next;
        //            if (curOne == null) break;
        //            if (curOne.Text.HasValue())
        //            {
        //                curOne.Text = Tidy(curOne.Text, TidyPosition.Begin);
        //            }
        //            continue;
        //        }
        //        curOne = curOne.Next;
        //    }


        //}

        public static string EndFor<T>(this TmpForSect<T> sect)
        {
            //End_For(sect);
            return sect.ToString();
        }

        #region Nested type: DoIfObject

        public class DoIfObject<T>
        {
            public DoIfObject(string VarName, Func<T, bool> EachFunc)
            {
                this.VarName = VarName;
                this.EachFunc = EachFunc;
            }

            public string VarName { get; set; }
            public Func<T, bool> EachFunc;
        }

        #endregion

        #region Nested type: TmpForSect

        /// <summary>
        /// 一个ForSect包含 ForName 相同的 子级及兄弟级 结构.
        /// </summary>
        [Serializable]
        public class TmpForSect<T> : ICloneable
        {
            public TmpForSect()
            {
                Templates = new List<TmpForSect<T>>();
                Source = new List<T>();
                Ifed = false;
            }

            public bool Ifed { get; set; }

            public string Text { get; set; }

            public IEnumerable<TmpForSect<T>> Templates;

            /// <summary>
            /// 数据源各项。
            /// </summary>
            public IEnumerable<T> Source;

            /// <summary>
            /// 循环标识。
            /// </summary>
            public string forName { get; set; }

            public T Each { get; set; }

            public TmpForSect<T> Next;

            #region ICloneable Members

            public object Clone()
            {
                var retVal = new TmpForSect<T>();
                if (Equals(Each, null) == false)
                {
                    retVal.Each = ((T)Each).CloneEntity();
                }
                retVal.forName = forName;
                if (Equals(Next, null) == false)
                {
                    retVal.Next = Next.Clone() as TmpForSect<T>;
                }
                if (Equals(Source, null) == false)
                {
                    retVal.Source = Source.Select(o => ((T)o).CloneEntity());
                }
                if (Equals(Templates, null) == false)
                {
                    retVal.Templates = Templates.Select(o => o.Clone() as TmpForSect<T>);
                }
                retVal.Text = Text;
                retVal.Ifed = Ifed;
                return retVal;
            }

            #endregion

            public override string ToString()
            {
                var sl = new StringLinker();
                if (Source == null || !Source.Any())
                {
                    string strText = Text;

                    //if (strText.AsString().StartsWith(Environment.NewLine))
                    //{
                    //    strText = strText.Substring(Environment.NewLine.Length);
                    //}

                    sl += strText;
                }
                else
                {
                    sl += string.Join("", Templates.Select(o => o.ToString()).ToArray());
                }
                if (Next != null)
                    sl += Next.ToString();
                return sl;
            }

            public TmpForSect<T> GetLast()
            {
                if (Next == null)
                    return this;
                return Next.GetLast();
            }
        }


        public class TmpForIfSect<T>
        {
            private TmpForSect<T> For { get; set; }


            public TmpForIfSect<T> DoIf(Func<T, bool> func)
            {
                DoIf(new DoIfObject<T>("", func));
                return this;
            }

            public TmpForIfSect<T> DoIf(string ifName, Func<T, bool> func)
            {
                DoIf(new DoIfObject<T>(ifName, func));
                return this;
            }

            public TmpForIfSect<T> DoIf(params DoIfObject<T>[] doIfs)
            {
                this.DoIfs.AddRange(doIfs);
                return this;
            }

            public List<DoIfObject<T>> DoIfs { get; set; }

            public TmpForIfSect(TmpForSect<T> For)
            {
                if (For.Ifed) throw new GodError("不能重复EndForIf: " + For.ToString());

                this.For = For;
                this.DoIfs = new List<DoIfObject<T>>();
            }

            public TmpForSect<T> EndForIf()
            {
                TmpForSect<T> curOne = this.For;
                while (true)
                {
                    if (curOne == null)
                        break;
                    foreach (var item in curOne.Templates)
                    {
                        TmpForSect<T> subOne = item;
                        while (true)
                        {
                            if (subOne == null)
                            {
                                break;
                            }

                            ProcEachIf(item, this.DoIfs.ToArray());

                            subOne = subOne.Next;
                        }
                    }

                    curOne = curOne.Next;
                }

                For.Ifed = true;
                return For;
            }
        }

        #endregion
    }
}