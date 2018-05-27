using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        public ActionResult ResetPassword(string pin)
        {
            var pinManager = db.PinManagers.FirstOrDefault(f => f.Pin.ToString() == pin);
            if (pinManager != null)
            {
                ResetPassword resetInfo = new ResetPassword
                {
                    Id = pinManager.UserId
                };
                return View(resetInfo);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPassword resetInfo)
        {
            if (ModelState.IsValid)
            {
                var fgInfo = db.PinManagers.Find(resetInfo.Id);
                var user = db.Users.Find(resetInfo.Id);
                user.Password = resetInfo.NewPassword;
                user.ConfirmPassword = resetInfo.ConfirmNewPassword;

                db.PinManagers.Remove(fgInfo);
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("ResetPasswordConfirmation");

            }
            return View(resetInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(Login loginInfo)
        {
            var user = db.Users.FirstOrDefault(u => u.Email.Equals(loginInfo.Email));
            if (user != null)
            {
                Random generator = new Random();
                PinManager fg = new PinManager();
                fg.UserId = Convert.ToInt16(user.Id);
                fg.Pin = generator.Next(1000, 9999);
                db.PinManagers.Add(fg);
                db.SaveChanges();

                var link = Url.Action("ResetPassword", "Login", new { pin = fg.Pin }, Request.Url.Scheme);
                var body = "<p>To reset your password follow this link:</p><a href='" + link + "'>Reset password</a>";
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

        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}