using ClockMe.Models;
using System.Linq;
using System.Web.Mvc;

namespace ClockMe.Controllers
{
    public class PinManagerController : Controller
    {
        private ClockMeContext db = new ClockMeContext();

        // GET: PinManager
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(PinManager pinManager)
        {
            if (db.PinManagers.FirstOrDefault(reg => reg.Pin.Equals(pinManager.Pin)) != null)
            {
                var user = db.Users.Find(pinManager.UserId);
                if (user == null)
                {
                    return RedirectToAction("Create", "Users", new { pin = pinManager.Pin });
                }
                else
                {
                    return RedirectToAction("ResetPassword", "Login", new { pin = pinManager.Pin });
                }
            }
            else
            {
                ModelState.AddModelError("Pin", "Pin is incorrect!");
            }
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