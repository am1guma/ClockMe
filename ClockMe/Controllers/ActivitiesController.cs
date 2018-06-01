using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClockMe.Models;
using ClockMe.App_Start;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.text.html.simpleparser;
using System.Globalization;

namespace ClockMe.Controllers
{
    public class ActivitiesController : Controller
    {
        private ClockMeContext db = new ClockMeContext();

        // GET: Activities
        public ActionResult Index(string email, string startDate, string endDate, string type)
        {
            var activities = from a in db.Activities select a;
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
            {
                email = "";
            }
            if (email != null && startDate != null && endDate != null && type != null)
            {
                ViewBag.email = email;
                ViewBag.type = type;
                if (type == "all")
                    type = "";
                var sd = new DateTime(1000, 1, 1);
                var ed = new DateTime(3000, 1, 1);
                if(startDate != "")
                    sd = DateTime.ParseExact(startDate, "MM/dd/yyyy HH:mm", null);
                if (endDate != "")
                    ed = DateTime.ParseExact(endDate, "MM/dd/yyyy HH:mm", null);
                activities = activities.Where(s => s.User.Email.Contains(email) && s.Time >= sd && s.Time <= ed && s.Type.Contains(type));
            }
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
            {
                var userId = Convert.ToInt32(Session["UserId"]);
                activities = activities.Where(s => s.UserId == userId);
            }
            Global.CurrentActivities = activities.Select(item => new
            {
                item.User.Email,
                item.Time,
                item.Type
            }).OrderByDescending(s => s.Time).ToList();
            return View(activities.OrderByDescending(s => s.Time).ToList());
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
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
            {
                var id = Convert.ToInt32(Session["UserId"]);
                return View(new Activity { UserId = id });
            }
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
                if(activity.Type == "out")
                {
                    Activity lastActivity = db.Activities.Where(s => s.UserId == activity.UserId).OrderByDescending(s => s.Time).First();
                    if(lastActivity.Type == "in")
                    {
                        double hours = (activity.Time - lastActivity.Time).TotalHours;
                        var timesheet = new Timesheet { UserId = activity.UserId, Date = DateTime.Now, Hours = Math.Round(hours, 2), Type = "workingday" };
                        db.Timesheets.Add(timesheet);
                    }
                }
                db.Activities.Add(activity);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", activity.UserId);
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
                ViewBag.UserId = Session["UserId"].ToString();
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

        public ActionResult ExportToExcel()
        {
            var gv = new GridView
            {
                DataSource = Global.CurrentActivities
            };
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            var currentDate = DateTime.Now;
            Response.AddHeader("content-disposition", "attachment; filename=Activities_" + currentDate.Hour + "_" + currentDate.Minute + "_" + currentDate.Second + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return View("Index");
        }
        
        public FileResult ExportToPdf()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var gv = new GridView
                {
                    DataSource = Global.CurrentActivities
                };
                gv.DataBind();
                var currentDate = DateTime.Now;
                StringWriter objStringWriter = new StringWriter();
                HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
                gv.RenderControl(objHtmlTextWriter);
                StringReader sr = new StringReader(objStringWriter.ToString());
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 100f, 0f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", "Activities_" + currentDate.Hour + "_" + currentDate.Minute + "_" + currentDate.Second + ".pdf");
            }
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
