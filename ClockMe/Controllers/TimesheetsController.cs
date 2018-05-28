using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClockMe.Models;

namespace ClockMe.Controllers
{
    public class TimesheetsController : Controller
    {
        private ClockMeContext db = new ClockMeContext();

        // GET: Timesheets
        public ActionResult Index(string email, string startDate, string endDate,string hours, string type)
        {
            var timesheets = from t in db.Timesheets select t;
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
            {
                email = "";
            }
            if (email != null && startDate != null && endDate != null && type != null)
            {
                var sd = new DateTime(1000, 1, 1);
                var ed = new DateTime(3000, 1, 1);
                if (startDate != "")
                    sd = DateTime.ParseExact(startDate, "dd.MM.yyyy", null);
                if (endDate != "")
                    ed = DateTime.ParseExact(endDate, "dd.MM.yyyy", null);
                timesheets = timesheets.Where(s => s.User.Email.Contains(email) && s.Date >= sd && s.Date <= ed && s.Hours.ToString().Contains(hours) && s.Type.Contains(type));
            }
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
            {
                var userId = Convert.ToInt32(Session["UserId"]);
                timesheets = timesheets.Where(s => s.UserId == userId);
            }
            return View(timesheets.ToList());
        }

        // GET: Timesheets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Timesheet timesheet = db.Timesheets.Find(id);
            if (timesheet == null)
            {
                RedirectToAction("Index");
            }
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
            {
                var userId = Convert.ToInt32(Session["UserId"]);
                if (timesheet.UserId != userId)
                    return RedirectToAction("Index");
            }
            return View(timesheet);
        }

        // GET: Timesheets/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email");
            return View();
        }

        // POST: Timesheets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserId,Date,Hours,Type")] Timesheet timesheet)
        {
            if (ModelState.IsValid)
            {
                db.Timesheets.Add(timesheet);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", timesheet.UserId);
            return View(timesheet);
        }

        // GET: Timesheets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Timesheet timesheet = db.Timesheets.Find(id);
            if (timesheet == null)
            {
                RedirectToAction("Index");
            }
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
            {
                var userId = Convert.ToInt32(Session["UserId"]);
                if (timesheet.UserId != userId)
                    return RedirectToAction("Index");
            }
            return View(timesheet);
        }

        // POST: Timesheets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,Date,Hours,Type")] Timesheet timesheet)
        {
            if (ModelState.IsValid)
            {
                db.Entry(timesheet).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(timesheet);
        }

        // GET: Timesheets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Timesheet timesheet = db.Timesheets.Find(id);
            if (timesheet == null)
            {
                RedirectToAction("Index");
            }
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
            {
                var userId = Convert.ToInt32(Session["UserId"]);
                if (timesheet.UserId != userId)
                    return RedirectToAction("Index");
            }
            return View(timesheet);
        }

        // POST: Timesheets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Timesheet timesheet = db.Timesheets.Find(id);
            db.Timesheets.Remove(timesheet);
            db.SaveChanges();
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
