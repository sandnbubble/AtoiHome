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
    public class UploadImagesController : Controller
    {
        private UploadImageDBContext db = new UploadImageDBContext();

        // GET: UploadImages
        public ActionResult Index()
        {
            return View(db.UploadImages.ToList());
        }

        // GET: UploadImages/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UploadImage uploadImage = db.UploadImages.Find(id);
            if (uploadImage == null)
            {
                return HttpNotFound();
            }
            return View(uploadImage);
        }

        // GET: UploadImages/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UploadImages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,UserID,UploadDate,ImagePath")] UploadImage uploadImage)
        {
            if (ModelState.IsValid)
            {
                db.UploadImages.Add(uploadImage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(uploadImage);
        }

        // GET: UploadImages/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UploadImage uploadImage = db.UploadImages.Find(id);
            if (uploadImage == null)
            {
                return HttpNotFound();
            }
            return View(uploadImage);
        }

        // POST: UploadImages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,UserID,UploadDate,ImagePath")] UploadImage uploadImage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(uploadImage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(uploadImage);
        }

        // GET: UploadImages/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UploadImage uploadImage = db.UploadImages.Find(id);
            if (uploadImage == null)
            {
                return HttpNotFound();
            }
            return View(uploadImage);
        }

        // POST: UploadImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UploadImage uploadImage = db.UploadImages.Find(id);
            db.UploadImages.Remove(uploadImage);
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
