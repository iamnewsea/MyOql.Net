using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace MyCmn
{
    public static partial class WebFormHelper
    {
        /// <summary>
        /// 设置控件集合里的控件的只读属性.  [★]
        /// </summary>
        /// <param name="controlWithSubControls">要设置成 ReadOnly 的控件的 父控件。</param>
        /// <param name="isReadOnly">是否设置为 ReadOnly </param>
        public static void SetReadOnly<T>(this Control controlWithSubControls, bool isReadOnly) where T : Control
        {
            if (controlWithSubControls is T)
            {
                if (controlWithSubControls is HtmlControl)
                {
                    if (isReadOnly)
                    {
                        (controlWithSubControls as HtmlControl).Attributes["ReadOnly"] = isReadOnly.ToString();
                    }
                    else
                    {
                        (controlWithSubControls as HtmlControl).Attributes.Remove("ReadOnly");
                    }
                }
                else if (controlWithSubControls is WebControl)
                {
                    if (isReadOnly)
                    {
                        (controlWithSubControls as WebControl).Attributes["ReadOnly"] = isReadOnly.ToString();
                    }
                    else
                    {
                        (controlWithSubControls as WebControl).Attributes.Remove("ReadOnly");
                    }
                }
            }

            foreach (Control c in controlWithSubControls.Controls)
            {
                if (c.HasControls())
                {
                    SetReadOnly<T>(c, isReadOnly);
                }
            }
        }


        /// <summary>
        /// 只针对 ImageButton 或 Submit 按钮。且不会生成  __doPostBack 脚本。
        /// </summary>
        /// <param name="TheGridView"></param>
        /// <param name="Command"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public static string GetGridViewCommandButtonJs(this GridView TheGridView, string Command, int Index)
        {
            return string.Format(@"$('#__EVENTTARGET').val('{0}');$('#__EVENTARGUMENT').val('{1}');", TheGridView.UniqueID, Command + "$" + Index.ToString());
        }

        /// <summary>
        /// 控件替换.
        /// </summary>
        /// <param name="TheControl"></param>
        /// <param name="NewControl"></param>
        public static void Replace(this Control TheControl, Control NewControl)
        {
            Replace(TheControl, NewControl, false);
        }
        /// <summary>
        /// 控件替换.
        /// </summary>
        /// <param name="TheControl"></param>
        /// <param name="NewControl"></param>
        /// <param name="UseNewID"></param>
        public static void Replace(this Control TheControl, Control NewControl, bool UseNewID)
        {
            if (TheControl == null) return;
            Control container = TheControl.Parent;
            if (container == null) return;

            int index = container.Controls.IndexOf(TheControl);

            container.Controls.Remove(TheControl);

            if (NewControl != null)
            {
                if (UseNewID == false)
                {
                    NewControl.ID = TheControl.ID;
                }

                container.Controls.AddAt(index, NewControl);
            }
        }


        /// <summary>
        /// 设置 控件的 客户端 Display 属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ControlWithSubControls"></param>
        /// <param name="IsDisplay"></param>
        public static void SetDisplay<T>(this Control ControlWithSubControls, bool IsDisplay) where T : Control
        {
            if (ControlWithSubControls is T)
            {
                (ControlWithSubControls as HtmlControl).Style[HtmlTextWriterStyle.Display] = IsDisplay ? "inline" : "none";
            }

            foreach (Control c in ControlWithSubControls.Controls)
            {
                if (c.HasControls())
                {
                    SetDisplay<T>(c, IsDisplay);
                }
            }
        }

        ///// <summary>
        ///// 取出 InnerText 部分， 供C#端使用。
        ///// </summary>
        ///// <param name="Value"></param>
        ///// <returns></returns>
        //public static string GetHtmlInnerText(this string Value)
        //{
        //    // [^x]  	匹配除了x以外的任意字符
        //    Regex objRegExp = new Regex("<[^>^<]+?>");
        //    string strOutput = HttpUtility.HtmlDecode(objRegExp.Replace(Value, ""));

        //    ////把所有空格变为一个空格
        //    //Regex r = new Regex(@"\s+");
        //    //strOutput = r.Replace(strOutput, " ");
        //    //strOutput.Trim();

        //    return strOutput.Trim();
        //}

        public static string GetHtmlPropertyValue(this string Value, string PropertyName)
        {
            //判断是不是完整的Xml表义.
            //1.取出 标签头.

            Value = Value.Replace(@"\'", ValueProc.SplitSect.ToString()).Replace(@"\""", ValueProc.SplitLine.ToString());
            Match mthValue = Regex.Match(Value, string.Format(@"(?<={0}=(""|')).+?(?=\1)", PropertyName), RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (mthValue != null) return mthValue.Value.Replace(ValueProc.SplitSect.ToString(), @"\'").Replace(ValueProc.SplitLine.ToString(), @"\""");
            else return null;
        }

        ///// <summary>
        ///// 从集合中取出文字来.应用在从单元格里取内容文字。
        ///// 如果文本是  &amp;nbsp; 或空格的话 ， 忽略内容，继续递归查找。
        ///// </summary>
        ///// <param name="webCon">要取文本的控件。</param>
        ///// <returns>控件里的文本。</returns>
        //public static string GetText(this Control webCon)
        //{
        //    string retVal = "";

        //    if (webCon is ITextControl)
        //    {
        //        retVal = (webCon as ITextControl).Text;
        //    }
        //    else if (webCon is IButtonControl)
        //    {
        //        retVal = (webCon as IButtonControl).Text;
        //    }
        //    else if (webCon is TableCell)
        //    {
        //        retVal = (webCon as TableCell).Text;
        //    }
        //    else if (webCon is HtmlInputControl)
        //    {
        //        retVal = (webCon as HtmlInputControl).Value;
        //    }
        //    else if (webCon is HtmlContainerControl)
        //    {
        //        retVal = (webCon as HtmlContainerControl).InnerText;
        //    }
        //    else if (webCon is CheckBox)
        //    {
        //    }

        //    retVal = retVal.Replace("&nbsp;", "").GetHtmlInnerText();

        //    if (retVal == "")
        //    {
        //        foreach (Control c in webCon.Controls)
        //        {
        //            return GetText(c);
        //        }
        //    }

        //    return retVal;
        //}


        /// <summary>
        /// 从 控件集合中 中递归取出 指定控件类型(T) 的第一个控件。 如果有 多个T类型的控件, 请使用 FindControl
        /// </summary>
        /// <typeparam name="T">要取控件的类型</typeparam>
        /// <param name="Ctl">要从哪个单元格中取.</param>
        /// <returns></returns>
        public static T GetControl<T>(this Control Ctl)
        {
            foreach (Control con in Ctl.Controls)
            {
                if (con is T)
                {
                    return (T)(object)con;
                }
                else if (con.HasControls())
                {
                    T subCon = GetControl<T>(con);
                    if (subCon != null) return subCon;
                }
            }
            return default(T);
        }

        public static T GetControl<T>(this Control Ctl, Func<T, bool> predicate)
        {
            foreach (Control con in Ctl.Controls)
            {
                if (con is T && predicate.Invoke((T)(object)con) == true)
                {
                    return (T)(object)con;
                }
                else if (con.HasControls())
                {
                    T subCon = GetControl<T>(con, predicate);
                    if (subCon != null) return subCon;
                }
            }
            return default(T);
        }
        /// <summary>
        /// 得到指定类型的最顶层的控件。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Ctl"></param>
        /// <returns></returns>
        public static T GetTopControl<T>(this Control Ctl)
        {
            T retVal = GetParentControl<T>(Ctl);
            if (retVal == null) return (T)(object)Ctl;
            else
            {
                return GetTopControl<T>((Control)(object)retVal);
            }
        }

        /// <summary>
        /// 得到指定类型的上层控件。从该控件的父控件开始算。
        /// </summary>
        /// <typeparam name="T">要得到控件的类型。</typeparam>
        /// <param name="Ctl">要得到控件的最底层的儿子。</param>
        /// <returns>指定类型的，最近层的控件。</returns>
        public static T GetParentControl<T>(this Control Ctl)
        {
            if (Ctl == null) return default(T);
            Control con = Ctl.Parent;
            if (con == null) return default(T);

            if (con is T)
            {
                return (T)(object)con;
            }
            else
            {
                return con.GetParentControl<T>();
            }
        }


        /// <summary>
        /// 从集合中取出指定类型的集合。递归性能不是很好，目前也没有方法实现更好的方法。应该避免大量使用。100次90毫秒，10000次5000毫秒。
        /// </summary>
        /// <typeparam name="T">要取控件的类型</typeparam>
        /// <param name="Ctl">要取控件的源</param>
        /// <returns>取到的指定类型的控件。</returns>
        public static List<T> GetControlList<T>(this Control Ctl)
        {
            List<T> retList = new List<T>();
            foreach (Control con in Ctl.Controls)
            {
                if (con is T)
                {
                    retList.Add((T)(object)con);
                }
                else if (con.HasControls())
                {
                    List<T> subList = GetControlList<T>(con);

                    foreach (T var in subList)
                    {
                        retList.Add(var);
                    }
                }
            }
            return retList;
        }

        public static List<T> GetControlList<T>(this Control Ctl, Func<T, bool> predicate)
        {
            List<T> retList = new List<T>();
            foreach (Control con in Ctl.Controls)
            {
                if (con is T && predicate.Invoke((T)(object)con) == true)
                {
                    retList.Add((T)(object)con);
                }
                else if (con.HasControls())
                {
                    List<T> subList = GetControlList<T>(con, predicate);

                    foreach (T var in subList)
                    {
                        retList.Add(var);
                    }
                }
            }
            return retList;
        }
    }
}