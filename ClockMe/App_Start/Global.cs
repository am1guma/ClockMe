using ClockMe.Models;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

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
        public static int Total { get; set; }

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
    }
}