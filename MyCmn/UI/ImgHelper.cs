using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;

namespace MyCmn
{
    public class ImgHelper
    {
        public static byte[] ImgScale(
            byte[] byteArrayIn,
            InterpolationMode Mode = InterpolationMode.High,
            int MaxValue = 1200,
            int MinValue = 900)
        {
            using (System.IO.MemoryStream msIn = new System.IO.MemoryStream(byteArrayIn))
            {
                using (var img = System.Drawing.Image.FromStream(msIn))
                {
                    msIn.Flush();

                    var size = GetSuiteSize(img.Width, img.Height, MaxValue, MinValue);

                    if (size.IsEmpty)
                    {
                        return byteArrayIn;
                    }

                    return ImgSize(img, size, Mode);
                }
            }
        }

        private static byte[] ImgSize(Image img, Size size, InterpolationMode Mode)
        {
            using (Bitmap retVal = new Bitmap(size.Width, size.Height))
            {
                using (Graphics g = Graphics.FromImage(retVal))
                {

                    // 插值算法的质量
                    g.InterpolationMode = Mode;

                    g.DrawImage(img, new Rectangle(0, 0, size.Width, size.Height), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);


                    using (var msOut = new MemoryStream())
                    {
                        retVal.Save(msOut, System.Drawing.Imaging.ImageFormat.Jpeg);

                        msOut.Position = 0;
                        var ret = new byte[msOut.Length];
                        msOut.Read(ret, 0, msOut.Length.AsInt());
                        msOut.Flush();

                        return ret;
                    }
                }
            }
        }

        private static Size GetSuiteSize(int width, int height, int MaxValue, int MinValue)
        {

            var whBi = width * 1.0 / height;

            //横拍
            if (width > height)
            {
                //定义限宽，限高。
                var Xw = MaxValue;
                var Xh = MinValue;

                if (width <= Xw && height <= Xh)
                {
                    return Size.Empty;
                }

                if (width > Xw)
                {
                    width = Xw;
                    height = (width / whBi).AsInt();
                }

                if (height > Xh)
                {
                    height = Xh;
                    width = (height * whBi).AsInt();
                }
            }
            else
            {
                //定义限宽，限高。
                var Xw = MinValue;
                var Xh = MaxValue;

                if (width <= Xw && height <= Xh)
                {
                    return Size.Empty;
                }

                if (width > Xw)
                {
                    width = Xw;
                    height = (width / whBi).AsInt();
                }

                if (height > Xh)
                {
                    height = Xh;
                    width = (height * whBi).AsInt();
                }
            }

            return new Size(width, height);

        }

        public int Horizontal_Width { get; set; }

        public int Horizontal_Height { get; set; }

        public int Vertical_Width { get; set; }

        public int Vertical_Height { get; set; }
    }
}
