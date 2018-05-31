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
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.text.html.simpleparser;

namespace ClockMe.Controllers
{
    public class UsersController : Controller
    {
        private ClockMeContext db = new ClockMeContext();

        // GET: Users
        public ActionResult Index(string firstName, string lastName, string email, string role, string workingHours)
        {
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
                return RedirectToAction("Index", "Home");
            var users = from u in db.Users select u;
            if (firstName != null && lastName != null && email != null && role != null && workingHours != null)
            {
                ViewBag.firstName = firstName;
                ViewBag.lastName = lastName;
                ViewBag.email = email;
                ViewBag.role = role;
                ViewBag.workingHours = workingHours;
                if (role == "all")
                    role = "";
                users = users.Where(s => s.FirstName.Contains(firstName) && s.LastName.Contains(lastName) && s.Email.Contains(email) && s.Role.Contains(role) && s.WorkingHours.ToString().Contains(workingHours));
            }
            Global.CurrentUsers = users.Select(item => new
            {
                item.FirstName,
                item.LastName,
                item.Email,
                item.Role,
                item.WorkingHours
            }).ToList();
            return View(users.ToList());
        }

        public ActionResult UserSettings()
        {
            User user = db.Users.Find(Convert.ToInt32(Session["UserId"]));
            user.Password = "";
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserSettings([Bind(Include = "Id,FirstName,LastName,Email,Password,ConfirmPassword,Role,WorkingHours")] User user)
        {
            if (ModelState.IsValid)
            {
                if (user.Password == null || user.ConfirmPassword == null)
                {
                    User u = db.Users.Find(Convert.ToInt32(Session["UserId"]));
                    user.Password = u.Password;
                    user.ConfirmPassword = u.Password;
                    db.Dispose();
                    db = new ClockMeContext();
                }
                else
                {
                    user.Password = Global.GetMd5Hash(user.Password);
                    user.ConfirmPassword = Global.GetMd5Hash(user.ConfirmPassword);
                }
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            return View(user);
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
                return RedirectToAction("Index", "Home");
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
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
                return RedirectToAction("Index", "Home");
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
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,Email,Password,ConfirmPassword,Role,WorkingHours")] User user)
        {
            if (ModelState.IsValid)
            {
                var pinManager = db.PinManagers.First(pin => pin.UserId == user.Id);
                user.Role = "user";
                user.Password = Global.GetMd5Hash(user.Password);
                user.ConfirmPassword = Global.GetMd5Hash(user.ConfirmPassword);
                db.Users.Add(user);
                db.PinManagers.Remove(pinManager);
                db.SaveChanges();
                return RedirectToAction("RegisterDone", "Register");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
                return RedirectToAction("Index", "Home");
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
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,Email,Password,ConfirmPassword,Role,WorkingHours")] User user)
        {
            user.ConfirmPassword = user.Password;
            ModelState["ConfirmPassword"].Errors.Clear();
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
            if (Session["Role"] != null && Session["Role"].ToString() != "admin")
                return RedirectToAction("Index", "Home");
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
        public ActionResult ExportToExcel()
        {
            var gv = new GridView
            {
                DataSource = Global.CurrentUsers
            };
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            var currentDate = DateTime.Now;
            Response.AddHeader("content-disposition", "attachment; filename=Users_" + currentDate.Hour + "_" + currentDate.Minute + "_" + currentDate.Second + ".xls");
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
                    DataSource = Global.CurrentUsers
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
                return File(stream.ToArray(), "application/pdf", "Users_" + currentDate.Hour + "_" + currentDate.Minute + "_" + currentDate.Second + ".pdf");
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
