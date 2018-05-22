using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClockMe.Models;
using Newtonsoft.Json;

namespace ClockMe.Controllers
{
    public class PlanningCalendarController : Controller
    {
        private ClockMeContext db = new ClockMeContext();

        // GET: PlanningCalendar
        public ActionResult Index()
        {
            var events = db.PlanningCalendars.Include(p => p.User);
            ViewBag.events = events.ToList().Select(item => new
            {
                item.Id,
                item.Name,
                item.Location,
                item.StartDate,
                item.EndDate,
            }).ToList();
            return View();
        }

        public ActionResult Create(PlanningCalendar cEvent)
        {
            //TODO get current user id
            cEvent.UserId = 1;

            if (cEvent.Name is null)
            {
                cEvent.Name = "";
            }
            if (cEvent.Location is null)
            {
                cEvent.Location = "";
            }
            db.PlanningCalendars.Add(cEvent);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(PlanningCalendar cEvent)
        {
            var calendarEvent = db.PlanningCalendars.Find(cEvent.Id);
            if (calendarEvent != null)
            {
                db.PlanningCalendars.Remove(calendarEvent);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(PlanningCalendar cEvent)
        {
            //TODO get current user id
            cEvent.UserId = 1;

            if (cEvent.Name is null)
            {
                cEvent.Name = "";
            }
            if (cEvent.Location is null)
            {
                cEvent.Location = "";
            }
            db.Entry(cEvent).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}