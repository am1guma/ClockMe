using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ClockMe.Models;
using ClockMe.App_Start;

namespace ClockMe.Controllers
{
    public class LoginController : Controller
    {
        private ClockMeContext db = new ClockMeContext();

        // GET: Login
        public ActionResult Index()
        {
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(User user)
        {
            var u = db.Users.FirstOrDefault(s => s.Email.Equals(user.Email));
            if (u != null)
            {
                user.Password = Global.GetMd5Hash(user.Password);
                if (u.Password.Equals(user.Password))
                {
                    Session["UserId"] = u.Id;
                    Session["Role"] = u.Role;
                    Session["UserEmail"] = u.Email;
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("Password", "Password incorrect!");
            }
            else
            {
                ModelState.AddModelError("Email", "Email does not exist!");
            }
            user.Password = "";
            return View(user);
        }

        public ActionResult Logout()
        {
            Session["UserId"] = null;
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
                User user = new User
                {
                    Id = pinManager.UserId
                };
                return View(user);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(User user)
        {
            if (ModelState.IsValid)
            {
                var fgInfo = db.PinManagers.Find(user.Id);
                var u = db.Users.Find(user.Id);
                u.Password = Global.GetMd5Hash(user.Password);
                u.ConfirmPassword = Global.GetMd5Hash(user.ConfirmPassword);

                db.PinManagers.Remove(fgInfo);
                db.Entry(u).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("ResetPasswordConfirmation");

            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(User user)
        {
            var u = db.Users.FirstOrDefault(s => s.Email.Equals(user.Email));
            if (u != null)
            {
                Random generator = new Random();
                PinManager fg = new PinManager();
                fg.UserId = Convert.ToInt16(u.Id);
                fg.Pin = generator.Next(1000, 9999);
                db.PinManagers.Add(fg);
                db.SaveChanges();

                var link = Url.Action("ResetPassword", "Login", new { pin = fg.Pin }, Request.Url.Scheme);
                var body = "<p>To reset your password follow this link:</p><a href='" + link + "'>Reset password</a>";
                var message = new MailMessage();
                message.To.Add(new MailAddress(user.Email));
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
            return View(user);
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