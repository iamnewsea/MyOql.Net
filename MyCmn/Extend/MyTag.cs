using System;
using System.Web;
using System.Web.UI;


namespace MyCmn
{
    /// <summary>
    /// 生成自定义Tag标签.
    /// </summary>
    /// <example>
    /// <code>
    /// using (MyTag tag = new MyTag(HtmlTextWriterTag.A, new { href = "g.cn", id = "id" }))
    /// {
    ///      Ronse += "Google";
    /// }
    /// </code>
    /// </example>
    public class MyTag : IDisposable
    {
        protected HtmlTextWriterTag Tag ;
        public MyTag(HtmlTextWriterTag tag, object Attributes)
        {
            this.Tag = tag;
            HttpContext.Current.Response.Write(MyHelper.BeginTag(null, tag, Attributes));
        }

        public void Dispose()
        {
            HttpContext.Current.Response.Write(MyHelper.EndTag(null, this.Tag));
        }
    }
}
