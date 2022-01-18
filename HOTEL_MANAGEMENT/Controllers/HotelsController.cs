using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HOTEL_MANAGEMENT.Models;

// La methode pour passer des models pour la methode DisplayHotel \\
using System.Dynamic;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Web.Helpers;
using System.IO;

namespace HOTEL_MANAGEMENT.Controllers
{
    public class HotelsController : Controller
    {
        private DB_HOTEL_MANAGEMENTEntities db = new DB_HOTEL_MANAGEMENTEntities();

        // GET: Hotels
        //public ActionResult Index()
        //{
        //    var hotels = db.Hotels.Include(h => h.User);
        //    return View(hotels.ToList());
        //}
        public ActionResult Index()
        {
            ExpandoObject expandoObject = new ExpandoObject();
            dynamic model = expandoObject;
            model.Hotels = db.Hotels.Take(4).ToList();
            model.Rooms = db.Rooms.ToList();
            return View(model);
        }

        // GET: Hotels/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hotel hotel = db.Hotels.Find(id);
            if (hotel == null)
            {
                return HttpNotFound();
            }

            Session["Id_Hotel"] = id;

            var room = (from h in db.Hotels
                        join r in db.Rooms
                        on h.Id_Hotel equals r.Id_Hotel
                        where h.Id_Hotel == id
                        select r).Take(8).ToList();

            var comment = db.Comments.Where(elt => elt.Id_Hotel == hotel.Id_Hotel).Include(c => c.User);

            dynamic model = new ExpandoObject();
            model.Hotels = hotel;
            model.Rooms = room;
            model.Comments = comment;

            return View(model);
        }

        // GET: Hotels/Create
        public ActionResult Create()
        {
            if (Session["Id_user"] == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if (Session["Roles"] != null && Session["Roles"].ToString().ToLower() == "true")
            {
                ViewBag.Id_user = new SelectList(db.Users, "Id_user", "First_Name");
                return View();
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // POST: Hotels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id_Hotel,Name_Hotel,Address_Hotel,City_Hotel,Stars,Description_Hotel,Image_Hotel,Number_Room,Telephone")] Hotel hotel)
        {
            if (ModelState.IsValid)
            {

                WebImage photo;
                var newFileName = "";
                var imagePath = "";

                photo = WebImage.GetImageFromRequest();
                if (photo != null)
                {
                    newFileName = Guid.NewGuid().ToString() + "_" +
                        Path.GetFileName(photo.FileName);
                    imagePath = @"\Images\" + newFileName;

                    photo.Resize(width: 500, height: 500, preserveAspectRatio: true, preventEnlarge: true);
                    photo.Save(@"~" + imagePath);

                    hotel.Image_Hotel = imagePath;
                }

                hotel.Id_user = Int32.Parse(Session["Id_user"].ToString());

                db.Hotels.Add(hotel);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Id_user = new SelectList(db.Users, "Id_user", "First_Name", hotel.Id_user);
            return View(hotel);
        }

        // GET: Hotels/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["Id_user"] == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hotel hotel = db.Hotels.Find(id);
            if (Session["Roles"] != null && Session["Roles"].ToString().ToLower() == "true")
            {
                if (hotel == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Id_user = new SelectList(db.Users, "Id_user", "First_Name", hotel.Id_user);
                return View(hotel);
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // POST: Hotels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id_Hotel,Name_Hotel,Address_Hotel,City_Hotel,Stars,Description_Hotel,Image_Hotel,Number_Room,Telephone")] Hotel hotel)
        {
            if (ModelState.IsValid)
            {
                WebImage photo;
                var newFileName = "";
                var imagePath = "";

                photo = WebImage.GetImageFromRequest();
                if (photo != null)
                {
                    newFileName = Guid.NewGuid().ToString() + "_" +
                        Path.GetFileName(photo.FileName);
                    imagePath = @"\Images\" + newFileName;

                    photo.Resize(width: 500, height: 500, preserveAspectRatio: true, preventEnlarge: true);
                    photo.Save(@"~" + imagePath);

                    hotel.Image_Hotel = imagePath;
                }

                db.Entry(hotel).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Id_user = new SelectList(db.Users, "Id_user", "First_Name", hotel.Id_user);
            return View(hotel);
        }

        // GET: Hotels/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["Id_user"] == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (Session["Roles"] != null && Session["Roles"].ToString().ToLower() == "true")
            {
                Hotel hotel = db.Hotels.Find(id);
                if (hotel == null)
                {
                    return HttpNotFound();
                }
                return View(hotel);
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // POST: Hotels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Hotel hotel = db.Hotels.Find(id);
            db.Hotels.Remove(hotel);
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
