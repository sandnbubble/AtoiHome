using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AtoiHomeWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;

namespace AtoiHomeWeb.Controllers
{
    public class UploadImageModelsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: UploadImageModels
        public ActionResult Index(int? page)
        {
            // get full userinfo
            //var store = new UserStore<ApplicationUser>(new ApplicationDbContext());
            //var userManager = new UserManager<ApplicationUser>(store);
            //ApplicationUser user = userManager.FindByNameAsync(User.Identity.Name).Result;

            var Images = User.Identity.Name.Equals("admin@atoihome.site") ?
                db.UploadImageModels.OrderByDescending(s => s.UploadDate).ToList() :
                db.UploadImageModels.OrderByDescending(s => s.UploadDate).Where(s => s.UserID == User.Identity.Name).ToList();
            //if (User.Identity.Name.Equals("admin@atoihome.site"))
            //    return View(db.UploadImageModels.OrderByDescending(s => s.UploadDate).ToList());
            //else
            //    return View(db.UploadImageModels.OrderByDescending(s => s.UploadDate).Where(s => s.UserID == User.Identity.Name).ToList());
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfImages = Images.ToPagedList(pageNumber, 1); // will only contain 25 products max because of the pageSize
            ViewBag.OnePageOfImages = onePageOfImages;

            return View();
        }

        // GET: UploadImageModels/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UploadImageModels uploadImageModels = db.UploadImageModels.Find(id);
            if (uploadImageModels == null)
            {
                return HttpNotFound();
            }
            return View(uploadImageModels);
        }

        // GET: UploadImageModels/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UploadImageModels/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,UserID,UploadDate,ImagePath")] UploadImageModels uploadImageModels)
        {
            if (ModelState.IsValid)
            {
                db.UploadImageModels.Add(uploadImageModels);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(uploadImageModels);
        }

        // GET: UploadImageModels/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UploadImageModels uploadImageModels = db.UploadImageModels.Find(id);
            if (uploadImageModels == null)
            {
                return HttpNotFound();
            }
            return View(uploadImageModels);
        }

        // POST: UploadImageModels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,UserID,UploadDate,ImagePath")] UploadImageModels uploadImageModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(uploadImageModels).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(uploadImageModels);
        }

        // GET: UploadImageModels/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UploadImageModels uploadImageModels = db.UploadImageModels.Find(id);
            if (uploadImageModels == null)
            {
                return HttpNotFound();
            }
            return View(uploadImageModels);
        }

        // POST: UploadImageModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UploadImageModels uploadImageModels = db.UploadImageModels.Find(id);
            db.UploadImageModels.Remove(uploadImageModels);
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
