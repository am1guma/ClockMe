using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZXing;
using ZXing.Common;

namespace ClockMe.QrGenerator
{
    public static class QrGenerator
    {
        public static string GenerateQrCode(string pin)
        {
            var qrWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions() { Height = 100, Width = 100, Margin = 0 }
            };

            using (var q = qrWriter.Write("192.168.0.178:81/Users/Create?pin=" + pin))
            {
                using (var ms = new MemoryStream())
                {
                    q.Save(ms, ImageFormat.Bmp);

                    using (Image image = Image.FromStream(ms))
                    {
                        var bmp = new Bitmap(100, 100, PixelFormat.Format16bppRgb555);
                        var gr = Graphics.FromImage(bmp);
                        gr.DrawImage(image, new Rectangle(0, 0, 100, 100));
                        Bitmap a = bmp;

                        var argb = "";
                        for (int i = 0; i < 100; i++)
                        {
                            for (int j = 0; j < 100; j++)
                            {
                                Color pixelColor = a.GetPixel(i, j);
                                byte red = pixelColor.R;
                                byte green = pixelColor.G;
                                byte blue = pixelColor.B;

                                int b = (blue >> 3) & 0x1f;
                                int g = ((green >> 2) & 0x3f) << 5;
                                int r = ((red >> 3) & 0x1f) << 11;

                                int result = r | g | b;
                                var st = result.ToString("X");
                                if (st == "FFFF")
                                {
                                    st = "1";
                                }
                                argb += st;
                            }
                        }
                        return argb;
                    }
                }
            }
        }
    }
}