using System.Collections.Generic;

namespace ClockMe.App_Start
{
    public static class Global
    {
        public static int LastMinute { get; set; }
        public static string IdToBeDeleted { get; set; }
        public static List<string> QrBytes { get; set; }
    }
}