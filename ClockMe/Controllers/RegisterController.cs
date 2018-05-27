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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(PinManager pinManager)
        {
            var _pinManager = db.PinManagers.FirstOrDefault(pin => pin.Pin.Equals(pinManager.Pin));
            if (_pinManager != null)
            {
                return RedirectToAction("Create", "Users", new { pin = _pinManager.Pin });
            }
            ModelState.AddModelError("Pin", "Pin is incorrect!");
            return View(pinManager);
        }

        public ActionResult RegisterDone()
        {
            return View();
        }
    }
}