using ClockMe.Models;
using System;
using System.Web.Mvc;

namespace ClockMe.Controllers
{
    public class ArduinoController : Controller
    {
        private ClockMeContext db = new ClockMeContext();
        
        public ActionResult Index()
        {
            return View();
        }

        public object ArduinoRequest()
        {
            var p1 = Request.QueryString["p1"];
            var p2 = Request.QueryString["p2"];

            switch(p1)
            {
                //register user
                case "2":
                    {
                        Random generator = new Random();

                        Register rg = new Register();
                        rg.Id = Convert.ToInt16(p2);
                        rg.Pin = generator.Next(1000, 9999);
                        db.Registers.Add(rg);
                        db.SaveChanges();
                        break;
                    }
            }
            return "0";
        }

        public object Status()
        {
            if (Session["ToBeDeleted"] != null)
            {
                var idToDelete = Session["ToBeDeleted"];
                Session["ToBeDeleted"] = null;
                return idToDelete;
            }
            return "0";
        }
        
        public object Test()
        {
            var time = DateTime.Now;
            if(Session["lastMinute"] == null)
            {
                Session["lastMinute"] = time.Minute;
            }
            if(Convert.ToInt32(Session["lastMinute"]) != time.Minute)
            {
                string minute = time.Minute + "";
                if (time.Minute < 10)
                {
                    minute = "0" + time.Minute;
                }
                return time.Day + "." + time.Month + "." + time.Year + ", " + time.Hour + ":" + minute;
            }
            return "0";
        }
    }
}