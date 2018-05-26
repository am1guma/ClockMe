using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClockMe.App_Start
{
    public static class Global
    {
        public static int LastMinute { get; set; }
        public static string IdToBeDeleted { get; set; }
    }
}