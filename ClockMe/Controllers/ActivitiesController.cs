﻿using System;
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
    public class ActivitiesController : Controller
    {
        private ClockMeContext db = new ClockMeContext();

        // GET: Activities
        public ActionResult Index(string email, string startDate, string endDate, string type)
        {
            var activities = from a in db.Activities select a;
            if (email != null && startDate != null && endDate != null && type != null)
            {
                var sd = new DateTime(1000, 1, 1);
                var ed = new DateTime(3000, 1, 1);
                if(startDate != "")
                    sd = DateTime.ParseExact(startDate, "dd.MM.yyyy HH:mm", null);
                if (endDate != "")
                    ed = DateTime.ParseExact(endDate, "dd.MM.yyyy HH:mm", null);
                activities = activities.Where(s => s.User.Email.Contains(email) && s.Time >= sd && s.Time <= ed && s.Type.Contains(type));
            }
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
            {
                var userId = Convert.ToInt32(Session["UserId"]);
                activities = activities.Where(s => s.UserId == userId);
            }
            return View(activities.ToList());
        }

        // GET: Activities/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                RedirectToAction("Index");
            }
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
            {
                var userId = Convert.ToInt32(Session["UserId"]);
                if (activity.UserId != userId)
                    return RedirectToAction("Index");
            }
            return View(activity);
        }

        // GET: Activities/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email");
            return View();
        }

        // POST: Activities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserId,Time,Type")] Activity activity)
        {
            if (ModelState.IsValid)
            {
                db.Activities.Add(activity);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", activity.UserId);
            return View(activity);
        }

        // GET: Activities/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                RedirectToAction("Index");
            }
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
            {
                var userId = Convert.ToInt32(Session["UserId"]);
                if (activity.UserId != userId)
                    return RedirectToAction("Index");
            }
            return View(activity);
        }

        // POST: Activities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,Time,Type")] Activity activity)
        {
            if (ModelState.IsValid)
            {
                db.Entry(activity).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(activity);
        }

        // GET: Activities/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                RedirectToAction("Index");
            }
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
            {
                var userId = Convert.ToInt32(Session["UserId"]);
                if (activity.UserId != userId)
                    return RedirectToAction("Index");
            }
            return View(activity);
        }

        // POST: Activities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Activity activity = db.Activities.Find(id);
            db.Activities.Remove(activity);
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
