using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ZXing;
using ZXing.Common;

namespace ClockMe.App_Start
{
    public static class Global
    {
        public static int LastMinute { get; set; }
        public static string IdToBeDeleted { get; set; }
        public static List<string> QrBytes { get; set; }
        public static IEnumerable<dynamic> CurrentActivities { get; set; }
        public static IEnumerable<dynamic> CurrentTimesheets { get; set; }
        public static IEnumerable<dynamic> CurrentUsers { get; set; }
        public static double Total { get; set; }

        private static MD5 md5Hash;

        public static void Init()
        {
            md5Hash = MD5.Create();
        }

        public static string GetMd5Hash(string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static string GenerateQrCode(string pin)
        {
            var qrWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions() { Height = 100, Width = 100, Margin = 0 }
            };

            QrBytes = new List<string>();

            using (var q = qrWriter.Write("192.168.43.251:81/Users/Create?pin=" + pin))
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
                                    st = "F";
                                }

                                argb += st;

                                if (argb.Length == 1000)
                                {
                                    QrBytes.Add(argb);
                                    argb = "";
                                }

                            }
                        }
                        return argb;
                    }
                }
            }
        }
    }
}