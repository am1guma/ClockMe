using ClockMe.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using ClockMe.App_Start;

namespace ClockMe.Controllers
{
    public class ArduinoController : Controller
    {
        private ClockMeContext db = new ClockMeContext();

        public object ArduinoRequest()
        {
            var requestType = Request.QueryString["requestType"];
            var id = Request.QueryString["id"];
            switch(requestType)
            {
                case "0":
                {
                    return GetTime(id);
                }
                case "1":
                {
                    return CheckForToBeDeleted();
                }
                case "2":
                {
                    return RegisterUser(id);
                }
                case "3":
                {
                    return GetNextId();
                }
                case "4":
                {
                    return RegisterActivity(id, "in");
                }
                case "5":
                {
                    return RegisterActivity(id, "out");
                }
                case "6":
                {
                    return GetQrBytes(id);
                }
            }
            return "0";
        }

        private string GetTime(string id)
        {
            var time = DateTime.Now;
            var minute = time.Minute + "";
            if (time.Minute < 10)
            {
                minute = "0" + time.Minute;
            }
            if (id == "1" || Global.LastMinute != time.Minute)
            {
                Global.LastMinute = time.Minute;
                return time.Month + "/" + time.Day + "/" + time.Year + ", " + time.Hour + ":" + minute;
            }
            return "0";
        }

        public string CheckForToBeDeleted()
        {
            if (Global.IdToBeDeleted != null)
            {
                var idToDelete = Global.IdToBeDeleted;
                Global.IdToBeDeleted = null;
                return idToDelete.ToString();
            }
            return "0";
        }

        private string RegisterUser(string id)
        {
            var pinManager = new PinManager
            {
                UserId = Convert.ToInt16(id),
                Pin = new Random().Next(1000, 9999)
            };
            db.PinManagers.Add(pinManager);
            db.SaveChanges();
            Global.QrBytes = null;
            return pinManager.Pin.ToString();
        }

        private string GetNextId()
        {
            var targetId = 1;
            var users = db.Users.ToList();
            while (users.Any(u => u.Id == targetId))
            {
                targetId += 1;
            }

            return targetId.ToString();
        }

        private string RegisterActivity(string id, string type)
        {
            var activity = new Activity
            {
                UserId = Convert.ToInt32(id),
                Time = DateTime.Now,
                Type = type
            };
            if (activity.Type == "out")
            {
                Activity lastActivity = db.Activities.Where(s => s.UserId == activity.UserId).OrderByDescending(s => s.Time).First();
                if (lastActivity.Type == "in")
                {
                    double hours = (activity.Time - lastActivity.Time).TotalHours;
                    if (hours == 0)
                        hours = 0.01;
                    var timesheet = new Timesheet { UserId = activity.UserId, Date = DateTime.Now, Hours = Math.Round(hours, 2), Type = "workingday" };
                    db.Timesheets.Add(timesheet);
                }
            }
            db.Activities.Add(activity);
            db.SaveChanges();
            return db.Users.Find(Convert.ToInt32(id))?.FirstName;
        }

        public object GetQrBytes(string id)
        {
            var pinManager = db.PinManagers.Find(Convert.ToInt32(id));
            if (Global.QrBytes == null)
            {
                QrGenerator.QrGenerator.GenerateQrCode(pinManager.Pin.ToString());
            }

            var qrCode = Global.QrBytes.ElementAt(0);
            Global.QrBytes.RemoveAt(0);
            return qrCode;
        }
    }
}