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
            var userId = Convert.ToInt32(Session["UserId"]);
            var events = db.PlanningCalendars.Where(s => s.UserId == userId);
            ViewBag.events = events.Select(item => new
            {
                item.Id,
                item.UserId,
                item.Name,
                item.Location,
                item.StartDate,
                item.EndDate,
            }).ToList();
            return View();
        }

        public ActionResult Create(PlanningCalendar cEvent)
        {
            cEvent.UserId = Convert.ToInt32(Session["UserId"]);

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
            var calendarEvent = db.PlanningCalendars.FirstOrDefault(s => s.UserId == cEvent.UserId && s.Id == cEvent.Id);
            if (calendarEvent != null)
            {
                db.PlanningCalendars.Remove(calendarEvent);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(PlanningCalendar cEvent)
        {
            cEvent.UserId = Convert.ToInt32(Session["UserId"]);

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