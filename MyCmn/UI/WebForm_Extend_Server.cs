using System;
using System.Web;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

namespace MyCmn
{
    public static partial class WebFormHelper
    {



        /// <summary>
        /// 传入的参数是 /MyWeb/WebResource.axd?d= 中 d的部分.返回它在 Dll 中资源的名称.
        /// Window 和 Linux 下实现方式是不同的， 所以这里要分别对待。
        /// </summary>
        /// <param name="QueryResourceID">加密的嵌入式资源的TypeID</param>
        /// <returns>Dll中嵌入式资源的名称 .</returns>
        public static string GetEmbedResourceName(string QueryResourceID)
        {
            if (string.Equals(Environment.OSVersion.Platform.ToString(), "Win32NT", StringComparison.CurrentCultureIgnoreCase))
            {
                Type type = typeof(System.Web.UI.Page);
                System.Reflection.MethodInfo mi = type.GetMethod("DecryptString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, new Type[] { typeof(string) }, null);
                string[] result = mi.Invoke((HttpContext.Current.CurrentHandler), new object[] { QueryResourceID }).ToString().Split('|');
                return result[1];
            }
            else if (string.Equals(Environment.OSVersion.Platform.ToString(), "Unix", StringComparison.CurrentCultureIgnoreCase))
            {
                Type type = typeof(System.Web.UI.Page);
                System.Reflection.MethodInfo mi = type.GetMethod("DecryptString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, new Type[] { typeof(string) }, null);
                if (mi != null)
                {
                    string[] result = mi.Invoke((HttpContext.Current.CurrentHandler), new object[] { QueryResourceID }).ToString().Split('|');
                    return result[1];
                }
                else return "";
            }
            else return "";
        }
    }
}
