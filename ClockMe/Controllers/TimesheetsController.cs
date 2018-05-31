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
using ClockMe.App_Start;
using ClockMe.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.text.html.simpleparser;

namespace ClockMe.Controllers
{
    public class TimesheetsController : Controller
    {
        private ClockMeContext db = new ClockMeContext();

        // GET: Timesheets
        public ActionResult Index(string email, string startDate, string endDate, string hours, string type)
        {
            var timesheets = from t in db.Timesheets select t;
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
            {
                email = "";
            }
            if (email != null && startDate != null && endDate != null && hours != null && type != null)
            {
                ViewBag.email = email;
                ViewBag.hours = hours;
                ViewBag.type = type;
                if (type == "all")
                    type = "";
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
            Global.CurrentTimesheets = timesheets.Select(item => new
            {
                item.User.Email,
                item.Date,
                item.Hours,
                item.Type
            }).OrderByDescending(s => s.Date).ToList();
            Global.Total = timesheets.Select(s => s.Hours).DefaultIfEmpty().Sum();
            return View(timesheets.OrderByDescending(s => s.Date).ToList());
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
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
            {
                var id = Convert.ToInt32(Session["UserId"]);
                return View(new Timesheet { UserId = id });
            }
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
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
                ViewBag.UserId = Session["UserId"].ToString();
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

        public ActionResult ExportToExcel()
        {
            var gv = new GridView
            {
                DataSource = Global.CurrentTimesheets
            };
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            var currentDate = DateTime.Now;
            Response.AddHeader("content-disposition", "attachment; filename=Timesheets_" + currentDate.Hour + "_" + currentDate.Minute + "_" + currentDate.Second + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Output.Write("Total Hours: ");
            Response.Output.Write(Global.Total);
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
                    DataSource = Global.CurrentTimesheets
                };
                gv.DataBind();
                var currentDate = DateTime.Now;
                StringWriter objStringWriter = new StringWriter();
                HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
                gv.RenderControl(objHtmlTextWriter);
                objHtmlTextWriter.InnerWriter.Write("<div>Total Hours: " + Global.Total + "</div>");
                StringReader sr = new StringReader(objStringWriter.ToString());
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 100f, 0f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", "Timesheets_" + currentDate.Hour + "_" + currentDate.Minute + "_" + currentDate.Second + ".pdf");
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
