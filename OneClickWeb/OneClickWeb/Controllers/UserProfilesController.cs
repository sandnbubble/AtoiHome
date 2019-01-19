using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OneClickWeb.Models;

namespace OneClickWeb.Controllers
{
    public class UserProfilesController : Controller
    {
        private OneClickDBContext db = new OneClickDBContext();

        // GET: UserProfiles
        public ActionResult Index()
        {
#if DEBUG
            return View(db.UserProfiles.OrderBy(s => s.Email).ToList());
#else
            if (Session["Email"] != null)
                return View(db.UserProfiles.OrderBy(s => s.Email).ToList());
            else
                return RedirectToAction("SignIn");
#endif
        }

        public ActionResult SignIn()
        {
            if (Session["Email"] == null)
                return View();
            else
                return RedirectToAction("UserDashBoard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(UserProfile objUser)
        {
            var obj = db.UserProfiles.Where(a => a.Email.Equals(objUser.Email) && a.Password.Equals(objUser.Password)).FirstOrDefault();
            if (obj != null)
            {
                Session["EMail"] = obj.Email.ToString();
                //Session["UserName"] = obj.UserName.ToString();
                return RedirectToAction("UserDashBoard");
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        public ActionResult UserDashBoard()
        {
            if (Session["Email"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("SignIn");
            }
        }

        public ActionResult UploadImages(String Email)
        {
            ViewData["Email"] = Email;
            return View(db.UploadImages.OrderByDescending(s => s.UploadDate).Where(s => s.UserID == Email).ToList());
        }
        // GET: UserProfiles/Details/5
        public ActionResult Details(string Email)
        {
            if (Email == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserProfile userProfile = db.UserProfiles.Find(Email);
            if (userProfile == null)
            {
                return HttpNotFound();
            }
            return View(userProfile);
        }

        // GET: UserProfiles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserProfiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Email,ID,Password,TelNo,SessionID")] UserProfile userProfile)
        {
            if (ModelState.IsValid)
            {
                db.UserProfiles.Add(userProfile);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(userProfile);
        }

        // GET: UserProfiles/Edit/5
        public ActionResult Edit(string Email)
        {
            if (Email == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserProfile userProfile = db.UserProfiles.Find(Email);
            if (userProfile == null)
            {
                return HttpNotFound();
            }
            return View(userProfile);
        }

        // POST: UserProfiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Email,ID,Password,TelNo,SessionID")] UserProfile userProfile)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userProfile).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(userProfile);
        }

        // GET: UserProfiles/Delete/5
        public ActionResult Delete(string Email)
        {
            if (Email == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserProfile userProfile = db.UserProfiles.Find(Email);
            if (userProfile == null)
            {
                return HttpNotFound();
            }
            return View(userProfile);
        }

        // POST: UserProfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string Email)
        {
            UserProfile userProfile = db.UserProfiles.Find(Email);
            db.UserProfiles.Remove(userProfile);
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
