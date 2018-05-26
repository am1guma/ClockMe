using ClockMe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClockMe.Controllers
{
    public class RegisterController : Controller
    {
        private ClockMeContext db = new ClockMeContext();

        // GET: Register
        public ActionResult Index()
        {
            var pin = Request.QueryString["pin"];
            if (pin != null)
            {
                var convertedPin = Convert.ToInt32(pin);
                var pinManager = db.PinManagers.FirstOrDefault(p => p.Pin.Equals(convertedPin));
                if (pinManager != null)
                {
                    Session["RegisterId"] = pinManager.UserId;
                    return RedirectToAction("Create", "Users");
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(PinManager pinManager)
        {
            var _pinManager = db.PinManagers.FirstOrDefault(pin => pin.Pin.Equals(pinManager.Pin));
            if (_pinManager != null)
            {
                Session["RegisterId"] = _pinManager.UserId;
                return RedirectToAction("Create", "Users");
            }
            ModelState.AddModelError("Pin", "Pin is incorrect!");
            return View(pinManager);
        }

        public ActionResult RegisterDone()
        {
            if (Session["RegisterId"] != null)
            {
                return View();
            }
            return RedirectToAction("Index", "Login");
        }
    }
}