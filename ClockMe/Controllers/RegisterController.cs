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
        public ActionResult Index(Register registerInfo)
        {
            var register = db.Registers.FirstOrDefault(reg => reg.Pin.Equals(registerInfo.Pin));
            if (register != null)
            {
                Session["RegisterId"] = register.Id;
                return RedirectToAction("Create", "Users");
            }
            else
            {
                ModelState.AddModelError("Pin", "Pin is incorrect!");
            }
            return View(registerInfo);
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