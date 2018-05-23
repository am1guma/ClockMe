using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ClockMe.Models;

namespace ClockMe.Controllers
{
    public class LoginController : Controller
    {
        private ClockMeContext db = new ClockMeContext();

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Login loginInfo)
        {
            var user = db.Users.FirstOrDefault(u => u.Email.Equals(loginInfo.Email));
            if (user != null)
            {
                if (user.Password.Equals(loginInfo.Password))
                {
                    Session["userId"] = user.Id;
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("Password", "Password incorrect!");
            }
            else
            {
                ModelState.AddModelError("Email", "Email does not exist!");
            }
            return View(loginInfo);
        }

        public ActionResult Logout()
        {
            Session["userId"] = null;
            return RedirectToAction("Index");
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(Login loginInfo)
        {
            var user = db.Users.FirstOrDefault(u => u.Email.Equals(loginInfo.Email));
            if (user != null)
            {
                var body = "<p>Your password is: " + user.Password + "</p>";
                var message = new MailMessage();
                message.To.Add(new MailAddress(loginInfo.Email));
                message.From = new MailAddress("clockmecontact@gmail.com");
                message.Subject = "Password recover";
                message.Body = string.Format(body);
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = "clockmecontact@gmail.com",
                        Password = "Clockme1!"
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 25;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
                return RedirectToAction("ForgotPasswordConfirmation");
            }
            ModelState.AddModelError("Email", "Email does not exist!");
            return View(loginInfo);
        }

        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
    }
}