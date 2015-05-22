using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;


namespace MyCmn
{
    /// <remarks>
    /// 当采用 MS Ajax 的 UpdatePanel 之后，页面回发之后，在 PageLoad 里不必再次执行加载，利用从Form Request 过来的值，
    /// 可以再进行一系列操作，这样可减少数据库的交互，提高性能。
    /// 
    /// 所以，约定： PageLoad 回发不加载，当回发之后， 用 GetPostValue 或 GetPostValues 来取客户端的值。
    /// GridView，也可以用 
    /// </remarks>
    public static partial class WebFormHelper
    {

        ///// <summary>
        ///// 得到 Js 在 页面的 包含表达式。得到 &lt;script type=""text/javascript"" language=""javascript"" src="{0}" {1} {2}>{3}&lt;/script> 样式。 [★]
        ///// </summary>
        ///// <param name="JsFile">要包含的 JS 文件的路径。</param>
        ///// <param name="IsDefer">是否添加 defer="defer" 属性。</param>
        ///// <param name="ID">Script 标签的 ID </param>
        ///// <param name="InnerJs">Script 标签包含的 文本。</param>
        ///// <returns>用于在客户端渲染的完整的 Script 标记段。</returns>
        //public static string GetJsInclude(string JsFile, bool IsDefer, string ID, string InnerJs)
        //{
        //    return string.Format(@"<script type=""text/javascript"" language=""javascript"" {0} {1} {2}>{3}</script>"
        //        , JsFile.HasValue() ? string.Format(@"src=""{0}""", JsFile) : string.Empty
        //        , IsDefer ? @"defer=""defer""" : string.Empty
        //        , ID.HasValue() ? string.Format(@"id=""{0}""", ID) : string.Empty
        //        , InnerJs.AsString()
        //        );
        //}

        /// <summary>
        /// 得到从客户端 Post 回来的 指定的控件名称。
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="BindName"></param>
        /// <returns></returns>
        public static string GetPostValue<T>(this GridViewRow Row, T BindName) where T : IComparable, IFormattable, IConvertible
        {
            string PostID = Row.UniqueID + "$" + BindName;

            if (HttpContext.Current.Request[PostID] == null)
            {
                PostID += "$Value";
            }

            return HttpContext.Current.Request[PostID];
        }


        /// <summary>
        /// 得到从客户端 Post 回来的 指定的控件名称。
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="BindName"></param>
        /// <returns></returns>
        public static string GetPostValue(this GridViewRow Row, string BindName)
        {
            string PostID = Row.UniqueID + "$" + BindName;

            if (HttpContext.Current.Request[PostID] != null)
            {
                return HttpContext.Current.Request[PostID];
            }
            else
            {
                PostID += "$Value";
                if (HttpContext.Current.Request[PostID] != null)
                {
                    return HttpContext.Current.Request[PostID];
                }
            }

            string key = HttpContext.Current.Request.Form.AllKeys
                .Where(o => string.Equals(o, BindName, StringComparison.CurrentCultureIgnoreCase))
                .FirstOrDefault();

            if (key.HasValue())
            {
                return HttpContext.Current.Request[key];
            }
            return "";
        }

        public static string GetPostValue(this TextBox text)
        {
            return HttpContext.Current.Request[text.UniqueID];
        }

        public static string[] GetPostValues(this CheckBoxList checkList)
        {
            return _GetPostValues(checkList);
        }

        private static string[] _GetPostValues(ListControl checkList)
        {
            List<string> list = new List<string>();
            foreach (string item in HttpContext.Current.Request.Form.AllKeys)
            {
                if (item.HasValue() == false) continue;

                if (item.StartsWith(checkList.UniqueID + "$"))
                {
                    string val = item.Substring((checkList.UniqueID + "$").Length);
                    if (Regex.IsMatch(val, @"^\d+$", RegexOptions.Compiled))
                    {
                        int index = val.AsInt(-1);
                        if (index >= 0)
                        {
                            list.Add(checkList.Items[index].Value);
                        }
                    }
                }
            }

            return list.ToArray();
        }

        public static string GetPostValue(this DropDownList dropDown)
        {
            return HttpContext.Current.Request[dropDown.UniqueID];
        }

        public static string GetPostValue(this RadioButtonList dropDown)
        {
            string[] str = _GetPostValues(dropDown);
            if (str == null || str.Length == 0) return string.Empty;
            else return str[0];
        }


    }
}