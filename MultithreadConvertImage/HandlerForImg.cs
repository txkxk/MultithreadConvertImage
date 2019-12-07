using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    static class HandlerForImg
    {
        public static bool GetPicThumbnail(string sFile, string dFile, ref string errMsg, int flag = 60, int maxLine = 1600)
        {
            try
            {
                using (Image iSource = Image.FromFile(sFile))
                {
                    ImageFormat tFormat = iSource.RawFormat;

                    int sW, sH = 0;

                    //按比例缩放
                    int dWidth = iSource.Width;
                    int dHeight = iSource.Height;

                    if (iSource.Width > iSource.Height)
                    {
                        if (iSource.Width <= maxLine)
                        {
                            sW = iSource.Width;
                            sH = iSource.Height;
                        }
                        else
                        {
                            sW = maxLine;
                            sH = sW * iSource.Height / iSource.Width;
                        }
                    }
                    else
                    {
                        if (iSource.Height <= maxLine)
                        {
                            sW = iSource.Width;
                            sH = iSource.Height;
                        }
                        else
                        {
                            sH = maxLine;
                            sW = sH * iSource.Width / iSource.Height;
                        }
                    }

                    using (Bitmap ob = new Bitmap(sW, sH))
                    {
                        using (Graphics g = Graphics.FromImage(ob))
                        {
                            g.CompositingQuality = CompositingQuality.HighQuality;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.DrawImage(iSource, new Rectangle(0, 0, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);
                        }

                        //以下代码为保存图片时，设置压缩质量
                        EncoderParameters ep = new EncoderParameters();
                        long[] qy = new long[1];
                        qy[0] = flag;//设置压缩的比例1-100
                        EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
                        ep.Param[0] = eParam;

                        ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                        ImageCodecInfo jpegICIinfo = null;
                        for (int x = 0; x < arrayICI.Length; x++)
                        {
                            if (arrayICI[x].FormatDescription.Equals("JPEG"))
                            {
                                jpegICIinfo = arrayICI[x];
                                break;
                            }
                        }

                        iSource.Dispose();
                        File.Delete(sFile);

                        if (jpegICIinfo != null)
                        {
                            ob.Save(dFile, jpegICIinfo, ep);//dFile是压缩后的新路径
                        }
                        else
                        {
                            ob.Save(dFile, tFormat);
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message + ",堆栈:" + ex.StackTrace + "InnerException:" + ex.InnerException;
                return false;
            }
        }
    }
}
