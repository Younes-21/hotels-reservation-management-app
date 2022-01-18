using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HOTEL_MANAGEMENT.Models;

namespace HOTEL_MANAGEMENT.Controllers
{
    public class ReservationsController : Controller
    {
        private DB_HOTEL_MANAGEMENTEntities db = new DB_HOTEL_MANAGEMENTEntities();

        // GET: Reservations
        public ActionResult Index()
        {
            if (Session["Id_user"] == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if(Session["Id_user"] != null)
            {
                if (Session["Roles"] != null && Session["Roles"].ToString().ToLower() != "true")
                {
                    int Id_user = Int32.Parse(Session["Id_user"].ToString());
                    var reservations = db.Reservations.Where(r => r.Id_user == Id_user).Include(r => r.Room).Include(r => r.User).ToList();

                    var rooms = db.Rooms.Include(h => h.Hotel);
                    var room = (from ro in rooms
                                    join re in db.Reservations.Where(r => r.Id_user == Id_user)
                                    on ro.Id_Room equals re.Id_Room
                                    select ro).ToList();

                    ExpandoObject expandoObject = new ExpandoObject();
                    dynamic model = expandoObject;

                    model.reservation = reservations.ToList();
                    model.room = room.ToList();

                    return View(model);
                }
                else if(Session["Roles"] != null && Session["Roles"].ToString().ToLower() == "true")
                {
                    var reservations = db.Reservations.Include(r => r.Room).Include(r => r.User).ToList();

                    var rooms = db.Rooms.Include(h => h.Hotel);
                    var room = (from ro in rooms
                                join re in db.Reservations
                                on ro.Id_Room equals re.Id_Room
                                select ro).ToList();

                    ExpandoObject expandoObject = new ExpandoObject();
                    dynamic model = expandoObject;

                    model.reservation = reservations;
                    model.room = room;

                    return View(model);
                }
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // GET: Reservations/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["Id_user"] == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservations.Find(id);
            if (Session["Id_user"]!= null && Int32.Parse(Session["Id_user"].ToString()) == reservation.Id_user || Session["Roles"].ToString().ToLower() == "true")
            {
                if (reservation == null)
                {
                    return HttpNotFound();
                }
                return View(reservation);
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // GET: Reservations/Create
        //public ActionResult Create(int? id)
        //{
        //    Session["Id_Room"] = id;
        //    return RedirectToAction("Create");
        //}

        // GET: Reservations/Create
        public ActionResult Create(int? id)
        {
            if (Session["Id_user"] == null)
            {
                return RedirectToAction("Login", "Users");
            }
            Session["Id_Room"] = id;
            if(Session["Id_Room"] == null)
            {
                ViewBag.errorroom = "please, choose a room to reserve";
                return RedirectToAction("index", "rooms");
            }
            ViewBag.Id_Room = new SelectList(db.Rooms, "Id_Room", "Image_Room");
            ViewBag.Id_user = new SelectList(db.Users, "Id_user", "First_Name");
            return View();
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Date_Begin,Date_End")] Reservation reservation)
        {
            if (reservation.Date_Begin > reservation.Date_End)
            {
                ViewBag.ErrorReservation = "Error, verify your date";
                return View(reservation);
            }
            if (ModelState.IsValid)
            {
                reservation.Id_Room = Int32.Parse(Session["Id_Room"].ToString());
                reservation.Id_user = Int32.Parse(Session["Id_user"].ToString());
                reservation.Date_Reservation = DateTime.Now;

                Room room = db.Rooms.Find(reservation.Id_Room);
                if (room == null)
                {
                    return HttpNotFound();
                }
                //Manque d'espace

                //var allrooms = db.Rooms.Where(elt => elt.Id_Hotel == room.Id_Hotel); //All rooms in the current hotel

                var allExisteReservation = db.Reservations.Where(res => (reservation.Date_Begin >= res.Date_Begin && reservation.Date_Begin <= res.Date_End) || (reservation.Date_End >= res.Date_Begin && reservation.Date_End <= res.Date_End)).Select(r=> r.Id_Room);

                var allroomsAvailable = db.Rooms.Where(elt => elt.Id_Hotel == room.Id_Hotel && !allExisteReservation.Contains(elt.Id_Room) && elt.Type_Room == room.Type_Room ).Count(); //All rooms in the current hotel

                if (allroomsAvailable < 1)
                {
                    ViewBag.ErrorReservation = "full hotel for this date and type of room ";
                    return View(reservation);
                }

                //continue the procedure
                int Days = (int)(reservation.Date_End - reservation.Date_Begin).TotalDays;
                reservation.Bill = Days * room.Price;

                db.Reservations.Add(reservation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Id_Room = new SelectList(db.Rooms, "Id_Room", "Image_Room", reservation.Id_Room);
            ViewBag.Id_user = new SelectList(db.Users, "Id_user", "First_Name", reservation.Id_user);
            return View(reservation);
        }

        // GET: Reservations/Edit/5
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
            Reservation reservation = db.Reservations.Find(id);
            if (Int32.Parse(Session["Id_user"].ToString()) == reservation.Id_user)
            { 
                if (reservation == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Id_Room = new SelectList(db.Rooms, "Id_Room", "Image_Room", reservation.Id_Room);
                ViewBag.Id_user = new SelectList(db.Users, "Id_user", "First_Name", reservation.Id_user);
                return View(reservation);
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id_Reservation,Date_Begin,Date_End,Date_Reservation,Bill,Id_user,Id_Room")] Reservation reservation)
        {
            if (reservation.Date_Begin > reservation.Date_End)
            {
                ViewBag.ErrorReservation = "Error, verify your date";
                return View(reservation);
            }
            if (ModelState.IsValid)
            {

                Room room = db.Rooms.Find(reservation.Id_Room);
                if (room == null)
                {
                    return HttpNotFound();
                }
                //Manque d'espace

                //var allrooms = db.Rooms.Where(elt => elt.Id_Hotel == room.Id_Hotel); //All rooms in the current hotel

                var allExisteReservation = db.Reservations.Where(res => (reservation.Date_Begin >= res.Date_Begin && reservation.Date_Begin <= res.Date_End) || (reservation.Date_End >= res.Date_Begin && reservation.Date_End <= res.Date_End)).Select(r => r.Id_Room);

                var allroomsAvailable = db.Rooms.Where(elt => elt.Id_Hotel == room.Id_Hotel && !allExisteReservation.Contains(elt.Id_Room) && elt.Type_Room == room.Type_Room).Count(); //All rooms in the current hotel

                if (allroomsAvailable < 1)
                {
                    ViewBag.ErrorReservation = "Hotel full for this date and type of room";
                    return View(reservation);
                }

                db.Entry(reservation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Id_Room = new SelectList(db.Rooms, "Id_Room", "Image_Room", reservation.Id_Room);
            ViewBag.Id_user = new SelectList(db.Users, "Id_user", "First_Name", reservation.Id_user);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
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
            Reservation reservation = db.Reservations.Find(id);
            if (Session["Id_user"]!= null && Int32.Parse(Session["Id_user"].ToString()) == reservation.Id_user)
            {
                if (reservation == null)
                {
                    return HttpNotFound();
                }
                return View(reservation);
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Reservation reservation = db.Reservations.Find(id);
            db.Reservations.Remove(reservation);
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
