using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using HOTEL_MANAGEMENT.Models;

namespace HOTEL_MANAGEMENT.Controllers
{
    public class RoomsController : Controller
    {
        private DB_HOTEL_MANAGEMENTEntities db = new DB_HOTEL_MANAGEMENTEntities();

        //Search
        public ActionResult Search(string City, int? TypeRoom, string StartDay, string EndDay)
        {
            if (TypeRoom.ToString() == "Open this select menu" || TypeRoom == null)
            {
                ViewData["ErrorTypeRoom"] = "Please, verify the fields";
                return RedirectToAction("Index", "Hotels");
            }

            Session["Name_Hotel"] = null;

            DateTime Begin_Date = Convert.ToDateTime(StartDay);
            DateTime End_Date = Convert.ToDateTime(EndDay);

            var Hotels = db.Hotels.Where(h => h.City_Hotel == City).Select(r => r.Id_Hotel);

            var inRange = GetReservationOnRange(Begin_Date, End_Date).Select(r => r.Id_Room);

            var rooms = db.Rooms.Where(r => !inRange.Contains(r.Id_Room) && Hotels.Contains(r.Id_Hotel) && r.Type_Room == TypeRoom).Include(h => h.Hotel);
            return View("Index", rooms.ToList());
        }
        private IEnumerable<Reservation> GetReservationOnRange(DateTime StartDate, DateTime EndDate)
        {
            //Func<Reservation, bool> CondtionSelect = (Reservation reservation, startDate, endDate) => (startDate < reservation.Date_Begin || startDate > reservation.Date_End) && (endDate < reservation.Date_Begin || endDate > reservation.Date_End));
            return db.Reservations.Where(reservation => (StartDate >= reservation.Date_Begin && StartDate <= reservation.Date_End) || (EndDate >= reservation.Date_Begin && EndDate <= reservation.Date_End));
        }

        // GET: Rooms
        public ActionResult Index()
        {
            //if (Session["Name_Hotel"] == null)
            //{
            //    return RedirectToAction("Index", "Hotels");
            //}
            if (Session["Id_Hotel"] != null)
            {
                int id = Int32.Parse(Session["Id_Hotel"].ToString());
                var rooms = db.Rooms.Where(elt => elt.Id_Hotel.Equals(id)).Include(r => r.Hotel);
                Session["Name_Hotel"] = rooms.Select(r => r.Hotel.Name_Hotel).FirstOrDefault()+"'s";
                return View(rooms.ToList());
            }
            else
            {
                return RedirectToAction("Index", "Hotels");
            }
        }
        //public ActionResult IndexSearch()
        //{
        //    if (Session["Id_Hotel"] != null)
        //    {
        //        int id = Int32.Parse(Session["Id_Hotel"].ToString());
        //        var rooms = db.Rooms.Where(elt => elt.Id_Hotel.Equals(id)).Include(r => r.Hotel);
        //        Session["Name_Hotel"] = rooms.Select(r => r.Hotel.Name_Hotel).FirstOrDefault();
        //        return View(rooms.ToList());
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Hotels");
        //    }
        //}

        // GET: Rooms/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Rooms.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // GET: Rooms/Create
        public ActionResult Create()
        {
            if (Session["Id_user"] == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if (Session["Roles"].ToString().ToLower() == "true")
            {
                ViewBag.Id_Hotel = new SelectList(db.Hotels, "Id_Hotel", "Name_Hotel");
                return View();
            }
            return RedirectToAction("ErrorAuthorisation", "Home");

        }

        // POST: Rooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id_Room,Type_Room,Price,Id_Hotel,Image_Room")] Room room)
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

                    room.Image_Room = imagePath;
                }

                room.Id_Hotel = Int32.Parse(Session["Id_Hotel"].ToString());

                db.Rooms.Add(room);
                db.SaveChanges();
                return RedirectToAction("Details/" + Session["Id_Hotel"], "Hotels");
            }

            ViewBag.Id_Hotel = new SelectList(db.Hotels, "Id_Hotel", "Name_Hotel", room.Id_Hotel);
            return View(room);
        }

        // GET: Rooms/Edit/5
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
            if (Session["Roles"].ToString().ToLower() == "true")
            {
                Room room = db.Rooms.Find(id);
                if (room == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Id_Hotel = new SelectList(db.Hotels, "Id_Hotel", "Name_Hotel", room.Id_Hotel);
                return View(room);
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // POST: Rooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id_Room,Type_Room,Price,Id_Hotel,Image_Room")] Room room)
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

                    room.Image_Room = imagePath;
                }

                db.Entry(room).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Id_Hotel = new SelectList(db.Hotels, "Id_Hotel", "Name_Hotel", room.Id_Hotel);
            return View(room);
        }

        // GET: Rooms/Delete/5
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
            if (Session["Roles"].ToString().ToLower() == "true")
            {
                Room room = db.Rooms.Find(id);
                if (room == null)
                {
                    return HttpNotFound();
                }
                return View(room);
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {   
            Room room = db.Rooms.Find(id);
            db.Rooms.Remove(room);
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
