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
            var register = db.PinManagers.FirstOrDefault(reg => reg.Pin.Equals(pinManager.Pin));
            if (register != null)
            {
                Session["RegisterId"] = register.UserId;
                //cand nu exista user cu id in tabela users
                return RedirectToAction("Create", "Users");
                //cand exista redirect la reset pass
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