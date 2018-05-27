using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClockMe.Models;
using ClockMe.App_Start;

namespace ClockMe.Controllers
{
    public class UsersController : Controller
    {
        private ClockMeContext db = new ClockMeContext();

        // GET: Users
        public ActionResult Index(string firstName, string lastName, string email, string role, string workingHours)
        {
            var users = from u in db.Users select u;
            if (firstName != null && lastName != null && email != null && role != null && workingHours != null)
            {
                users = users.Where(s => s.FirstName.Contains(firstName) && s.LastName.Contains(lastName) && s.Email.Contains(email) && s.Role.Contains(role) && s.WorkingHours.ToString().Contains(workingHours));
            }
            return View(users.ToList());
        }

        public ActionResult UserSettings()
        {
            User user = db.Users.Find(1);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserSettings([Bind(Include = "Id,FirstName,LastName,Email,Password,Role,WorkingHours")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index","Home");
            }
            return View(user);
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create(string pin)
        {
            var pinInDb = db.PinManagers.FirstOrDefault(p => p.Pin.ToString() == pin);
            if (pinInDb != null)
            {
                var user = new User
                {
                    Id = pinInDb.UserId
                };
                return View(user);
            }
            return RedirectToAction("Index", "Register");
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,Email,Password,Role,WorkingHours")] User user)
        {
            if (ModelState.IsValid)
            {
                var pinManager = db.PinManagers.First(pin => pin.UserId == user.Id);
                user.Role = "user";
                db.Users.Add(user);
                db.PinManagers.Remove(pinManager);
                db.SaveChanges();
                return RedirectToAction("RegisterDone","Register");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,Email,Password,Role,WorkingHours")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            Global.IdToBeDeleted = id.ToString();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
