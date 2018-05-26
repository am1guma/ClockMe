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
                return time.Day + "." + time.Month + "." + time.Year + ", " + time.Hour + ":" + minute;
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
            var rg = new Register
            {
                Id = Convert.ToInt16(id),
                Pin = new Random().Next(1000, 9999)
            };
            db.Registers.Add(rg);
            db.SaveChanges();
            return rg.Pin.ToString();
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
            var inActivity = new Activity
            {
                UserId = Convert.ToInt32(id),
                Time = DateTime.Now,
                Type = type
            };
            db.Activities.Add(inActivity);
            db.SaveChanges();
            return db.Users.Find(Convert.ToInt32(id))?.FirstName;
        }
    }
}