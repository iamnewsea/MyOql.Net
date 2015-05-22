using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Reflection;
using System.Text;
using System.Collections.Specialized;
using System.IO;
using Microsoft.Win32;
using System.Drawing;
using System.Collections;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Web.Configuration;
using System.Collections.Generic;

namespace MyCmn
{
    /// <summary>
    /// 针对一些Web应用常用的处理助手函数.
    /// </summary>
    public static partial class ValueProc
    {
        /// <summary>
        /// 配置是否是Js，Css压缩输出。
        /// </summary>
        /// <returns></returns>
        public static bool IsConfigJsCssCompressed()
        {
            if (HttpContext.Current == null) return true;


            System.Configuration.Configuration configuration = WebConfigurationManager.OpenWebConfiguration(
                HttpContext.Current.Request.ApplicationPath
                );

            SystemWebSectionGroup systemWeb = (SystemWebSectionGroup)configuration.GetSectionGroup("system.web");

            return systemWeb.Compilation.Debug;
        }

        /// <summary>
        /// 通过访问注册表，得到指定类型的 ContentType 。 [★]
        /// </summary>
        /// <param name="strSuffix"></param>
        /// <returns></returns>
        public static string GetContentType(string strSuffix)
        {
            string strResult = string.Empty;
            try
            {
                RegistryKey key = Registry.LocalMachine;
                key = key.OpenSubKey("SOFTWARE\\Classes\\" + strSuffix);

                if (key != null)
                    strResult = key.GetValue("Content Type", "").ToString();
            }
            catch
            {
            }
            return strResult;
        }

        public static StringDict ToStringDict(this NameValueCollection NameValueCollectionAsDataSource, Action<string, Exception, StringDict> ErrorFunc)
        {
            var retVal = new StringDict();
            foreach (string key in NameValueCollectionAsDataSource.AllKeys)
            {
                try
                {
                    retVal.Add(key, NameValueCollectionAsDataSource[key]);
                }
                catch (Exception e)
                {
                    if (ErrorFunc != null)
                    {
                        ErrorFunc(key, e, retVal);
                    }
                }
            }

            return retVal;
        }


        /// <summary>
        /// 将 NameValueCollection 转换成 字典 的形式 [★]
        /// </summary>
        /// <param name="NameValueCollectionAsDataSource"></param>
        /// <returns></returns>
        public static XmlDictionary<string, T> ToDictionary<T>(this NameValueCollection NameValueCollectionAsDataSource)
        {
            XmlDictionary<string, T> retVal = new XmlDictionary<string, T>();
            foreach (string key in NameValueCollectionAsDataSource.AllKeys)
            {
                retVal.Add(key, (T)(object)NameValueCollectionAsDataSource[key]);
            }

            return retVal;
        }

        public static XmlDictionary<string, object> ToDictionary(this NameValueCollection NameValueCollectionAsDataSource)
        {
            return ToDictionary<object>(NameValueCollectionAsDataSource);
        }

        public static Dictionary<TKey, TValue> MapToDictionary<TM, TKey, TValue>(this IEnumerable<TM> Source, Func<TM, TKey> keyFunc, Func<TM, TValue> valFunc)
        {
            var dict = new Dictionary<TKey, TValue>();
            using (var em = Source.GetEnumerator())
            {
                while (em.MoveNext())
                {
                    if (em.Current == null) break;

                    dict.Add(keyFunc(em.Current), valFunc(em.Current));
                }
            }
            return dict;
        }

        //public static T[] ToMyArray<T>(this IEnumerable<T> Value)
        //{
        //    if (Value == null) return null;

        //    List<T> list = new List<T>();

        //    IEnumerator<T> enr = Value.GetEnumerator();

        //    while (enr.MoveNext())
        //    {
        //        list.Add( enr.Current);
        //    }

        //    return list.ToArray();
        //}


        /// <summary>
        /// 得到 指定重复次数的 string 的表达式。 [★]
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="RepeatCount"></param>
        /// <returns></returns>
        public static string GetRepeatString(this string Value, int RepeatCount)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < RepeatCount; i++)
            {
                sb.Append(Value);
            }
            return sb.ToString();
        }


        ///// <summary>
        ///// 把 ICollection 转换为 List . 兼容模式. [★] .
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="TheCollection"></param>
        ///// <returns></returns>
        //[Obsolete("这一个，要明确用法。")]
        //public static List<T> ToMyList<T>(this ICollection TheCollection) 
        //{
        //    List<T> retVal = new List<T>();
        //    var ienum = TheCollection.GetEnumerator();
        //    while (ienum.MoveNext())
        //    {
        //        retVal.Add( ValueProc.Get<T>(ienum.Current ) ) ;
        //    }
        //    return retVal;
        //}


        /// <summary>
        /// 比较两个 Array 的值 是否相等。 [★]
        /// </summary>
        /// <param name="First"></param>
        /// <param name="Second"></param>
        /// <returns></returns>
        public static bool ArrayIsEqual(this Array First, Array Second)
        {
            if (First == null && Second == null)
                return true;
            if (First == null || Second == null)
                return false;
            if (First.Length != Second.Length)
                return false;

            for (int i = 0; i < First.Length; i++)
            {
                if (First.GetValue(i).Equals(Second.GetValue(i)) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 如果文件不存在,则创建文件,如果文件存在,则返回.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns>返回第一次是否成功摸到文件. 如果文件存在,返回true,否则返回false</returns>
        public static bool Touch(this FileInfo fileInfo)
        {
            if (fileInfo.Exists) return true;
            else
            {
                if (fileInfo.Directory.Exists == false)
                {
                    fileInfo.Directory.Create();
                }

                using (var fs = fileInfo.Create())
                {
                    fs.Close();
                }
            }
            return false;
        }



        /// <summary>
        /// 为了使 GridView 显示整齐的显示方式，而定义的类。 [★]
        /// </summary>
        public class DisplayText
        {
            /// <summary>
            /// 格式化之后， 显示的内容。
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// 如果经过格式化的话，显示的 原始值。如果没有经过格式化， 显示 ""
            /// </summary>
            public string ToolTip { get; set; }
        }

        /// <summary>
        /// 针对 GridView 列的显示名称过长, 取指定配置文件里 GridLineTitleLen 指定的长度的 Text  [★]
        /// </summary>
        /// <param name="LongText">GridView 里要格式化列的显示值。</param>
        /// <returns>格式化后的对象。 包括显示的格式化的文本内容，和 提示。提示就是完整理的内容。</returns>
        public static DisplayText GetDisplayText(this string LongText)
        {
            return GetDisplayText(LongText, -1);
        }


        /// <summary>
        /// 针对 GridView 列的显示名称过长, 取指定长度的 Text , [★]
        /// </summary>
        /// <param name="LongText"></param>
        /// <param name="DisplayLength"></param>
        /// <returns></returns>
        public static DisplayText GetDisplayText(this string LongText, int DisplayLength)
        {
            if (LongText.HasValue() == false) return new DisplayText();

            if (DisplayLength < 0)
            {
                string Len = ConfigurationManager.AppSettings["DisplayTextLength"];
                DisplayLength = Len.HasValue() ? ValueProc.AsInt(Len) : 20;
            }
            if (DisplayLength < 4) DisplayLength = 4;

            DisplayText retVal = new DisplayText();

            int bytesLen = UnicodeEncoding.Default.GetByteCount(LongText);
            if (bytesLen > DisplayLength)
            {
                int charLen = UnicodeEncoding.Default.GetCharCount(UnicodeEncoding.Default.GetBytes(LongText), 0, DisplayLength - 4);
                retVal.Text = LongText.Substring(0, charLen) + "...";
                retVal.ToolTip = LongText;
            }
            else
            {
                retVal.Text = LongText;
                retVal.ToolTip = "";
            }

            return retVal;
        }

        public static SizeF GetStringGdiSize(string Text, Font FontPt)
        {
            SizeF s = SizeF.Empty;
            using (Bitmap img = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(img))
                {
                    return g.MeasureString(Text, FontPt);
                }
            }
        }
        /// <summary>
        /// 计算控件内容的宽度，忽略字间距。
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="FontPt"></param>
        /// <returns></returns>
        public static int GetGdiStringWidth(string Text, Font FontPt)
        {
            SizeF s = GetStringGdiSize(Text, FontPt);

            if (s.IsEmpty) return 0;
            return ValueProc.AsInt(Math.Ceiling(s.Width));
        }



        /// <summary>
        /// 返回 Color 对象 的 十六进制表示形式。 [★]
        /// </summary>
        /// <param name="TheColorValue"></param>
        /// <returns></returns>
        public static string ToHexColorString(this Color TheColorValue)
        {
            if (TheColorValue.IsEmpty)
                return null;
            if (TheColorValue.A != 255)
                return Color.Transparent.Name;

            return "#" + TheColorValue.R.ToString("X2") + TheColorValue.G.ToString("X2") + TheColorValue.B.ToString("X2");
        }

        /// <summary>
        /// 将十六进制 表式形式的颜色值（#123456） 转换为 Color 对象。 [★]
        /// </summary>
        /// <param name="TheColorValue"></param>
        /// <returns></returns>
        public static Color ToColor(string TheColorValue)
        {
            if (TheColorValue.HasValue() == false)
                return Color.Empty;
            if (TheColorValue.StartsWith("#") == false)
                return Color.FromName(TheColorValue);
            if (TheColorValue.Length != 7)
                return Color.Empty;


            return Color.FromArgb(byte.Parse(TheColorValue.Substring(1, 2), System.Globalization.NumberStyles.HexNumber), byte.Parse(TheColorValue.Substring(3, 2), System.Globalization.NumberStyles.HexNumber), byte.Parse(TheColorValue.Substring(5, 2), System.Globalization.NumberStyles.HexNumber));
        }

        public static string TakeOut(this string Value, string BeginSign, string EndSign, int StartIndex = 0)
        {
            var start1 = Value.IndexOf(BeginSign, StartIndex);
            if (start1 < 0) return string.Empty;
            var start2 = Value.IndexOf(EndSign, StartIndex + BeginSign.Length);
            if (start2 < 0) return string.Empty;
            return Value.Slice(start1 + BeginSign.Length, start2);
        }

        /// <summary>
        /// 使GIF图片背景透明. [★]
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Bitmap MakeTransparentGif(this Bitmap bitmap, Color color)
        {
            byte R = color.R;
            byte G = color.G;
            byte B = color.B;
            MemoryStream fin = new MemoryStream();
            bitmap.Save(fin, ImageFormat.Gif);
            MemoryStream fout = new MemoryStream((int)fin.Length);
            int count = 0;
            byte[] buf = new byte[256];
            byte transparentIdx = 0;
            fin.Seek(0, SeekOrigin.Begin);
            //header    
            count = fin.Read(buf, 0, 13);
            if ((buf[0] != 71) || (buf[1] != 73) || (buf[2] != 70))
                return null;
            //GIF    
            fout.Write(buf, 0, 13);
            int i = 0;
            if ((buf[10] & 0x80) > 0)
            {
                i = 1 << ((buf[10] & 7) + 1) == 256 ? 256 : 0;
            }
            for (; i != 0; i--)
            {
                fin.Read(buf, 0, 3);
                if ((buf[0] == R) && (buf[1] == G) && (buf[2] == B))
                {
                    transparentIdx = (byte)(256 - i);
                }
                fout.Write(buf, 0, 3);
            }
            bool gcePresent = false;
            while (true)
            {
                fin.Read(buf, 0, 1);
                fout.Write(buf, 0, 1);
                if (buf[0] != 0x21)
                    break;
                fin.Read(buf, 0, 1);
                fout.Write(buf, 0, 1);
                gcePresent = (buf[0] == 0xf9);
                while (true)
                {
                    fin.Read(buf, 0, 1);
                    fout.Write(buf, 0, 1);
                    if (buf[0] == 0)
                        break;
                    count = buf[0];
                    if (fin.Read(buf, 0, count) != count)
                        return null;
                    if (gcePresent)
                    {
                        if (count == 4)
                        {
                            buf[0] = 0x1;
                            buf[3] = transparentIdx;
                        }
                    }
                    fout.Write(buf, 0, count);
                }
            }
            while (count > 0)
            {
                count = fin.Read(buf, 0, 1);
                fout.Write(buf, 0, 1);
            }
            fin.Close();
            fout.Flush();
            return new Bitmap(fout);
        }


        public static void MakeTransparentGif(string FileName)
        {
            using (Bitmap map = new Bitmap(FileName))
            {
                using (Bitmap retVal = MakeTransparentGif(map, Color.Black))
                {
                    if (retVal != null)
                    {
                        retVal.Save(FileName, ImageFormat.Gif);
                    }
                }
            }
        }


        /// <summary>
        /// 单位转换 ， 磅 值转换为 像素 值。
        /// </summary>
        /// <param name="PtValue">磅值</param>
        /// <returns>转换后的 像素 值 。</returns>
        public static int GetPxValue(int PtValue)
        {
            return GetPxValue(PtValue, 96);
        }

        /// <summary>
        /// 单位转换 ， 像素 值转换为 磅 值。
        /// </summary>
        /// <param name="PxValue">像素</param>
        /// <returns>转换后的 磅 值 。</returns>
        public static int GetPtValue(int PxValue)
        {
            return GetPtValue(PxValue, 96);
        }

        /// <summary>
        /// 单位转换 ， 磅 值转换为 像素 值。
        /// </summary>
        /// <param name="PtValue">磅值</param>
        /// <param name="PDI">PDI ， 默认为 96 </param>
        /// <returns>转换后的 像素 值 。</returns>
        public static int GetPxValue(int PtValue, int PDI)
        {
            return ValueProc.AsInt(PtValue * PDI * 1.0 / 72);
        }

        /// <summary>
        /// 单位转换 ， 像素 值转换为 磅 值。
        /// </summary>
        /// <param name="PxValue">像素</param>
        /// <param name="PDI">PDI ， 默认为 96 </param>
        /// <returns>转换后的 磅 值 。</returns>
        public static int GetPtValue(int PxValue, int PDI)
        {
            return ValueProc.AsInt(PxValue * 72 * 1.0 / PDI);
        }

        [Flags]
        public enum ResizeImageMode
        {
            /// <summary>
            /// 比例可以改变，强制
            /// </summary>
            Force = 2,
            /// <summary>
            /// 比例不变，允许裁剪
            /// </summary>
            Cut = 1,
        }


        /// <summary>
        /// 缩放图片.
        /// </summary>
        /// <param name="IsOK"></param>
        /// <param name="OriDiskFile"></param>
        /// <param name="MaxWidth"></param>
        /// <param name="MaxHeight"></param>
        /// <param name="Mode"></param>
        /// <returns>是否出错。true 没有问题， false，出错了</returns>
        public static Bitmap ResizeImageWidthWidth(Bitmap bitmapImg, int newWidth)
        {
            //放大倍数
            double bei = newWidth / bitmapImg.Width;
            var newHeight = Convert.ToInt32(bitmapImg.Height * bei);


            using (Bitmap retVal = new Bitmap(newWidth, newHeight))
            {
                using (Graphics g = Graphics.FromImage(retVal))
                {
                    // 插值算法的质量
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    g.DrawImage(bitmapImg, new Rectangle(0, 0, newWidth, newHeight), new Rectangle(0, 0, bitmapImg.Width, bitmapImg.Height), GraphicsUnit.Pixel);
                }
                return retVal;
            }
        }

        public static Bitmap ResizeImageWidthHeight(Bitmap bitmapImg, int newHeight)
        {
            //放大倍数
            double bei = newHeight / bitmapImg.Height;
            var newWidth = Convert.ToInt32(bitmapImg.Width * bei);


            using (Bitmap retVal = new Bitmap(newWidth, newHeight))
            {
                using (Graphics g = Graphics.FromImage(retVal))
                {
                    // 插值算法的质量
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    g.DrawImage(bitmapImg, new Rectangle(0, 0, newWidth, newHeight), new Rectangle(0, 0, bitmapImg.Width, bitmapImg.Height), GraphicsUnit.Pixel);
                }
                return retVal;
            }
        }
    }
}
