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
    public class UsersController : Controller
    {
        private DB_HOTEL_MANAGEMENTEntities db = new DB_HOTEL_MANAGEMENTEntities();

        public ActionResult Login(User user)
        {
            if (user.Email != null && user.Pwd != null)
            {
                bool b = false;

                var OneUser = db.Users.Where(elt => elt.Email.Equals(user.Email) && elt.Pwd.Equals(user.Pwd)).FirstOrDefault();
                if (OneUser != null)
                {
                    b = true;
                    Session["Id_user"] = OneUser.Id_user;
                    Session["Roles"] = OneUser.Roles;
                    Session["First_Name"] = OneUser.First_Name;

                    return RedirectToAction("Index", "Hotels");
                }
                if (b == false)
                {
                    ViewBag.MessageError = "Please verify your email or password";
                }
            }
            return View();
        }

        //Logout
        public ActionResult Logout()
        {
            Session["Id_user"] = null;
            Session["Roles"] = null;
            Session["First_Name"] = null;
            return RedirectToAction("Index", "Hotels");
        }

        // GET: Users
        public ActionResult Index()
        {
            if (Session["Id_user"] == null)
            {
                return RedirectToAction("Login", "Users");
            }
            if (Session["Roles"].ToString().ToLower() == "true")
            {
                return View(db.Users.ToList());
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // GET: Users/Details/5
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
            User user = db.Users.Find(id);
            if (Session["Roles"] != null && Session["Roles"].ToString().ToLower() == "true" || Int32.Parse(Session["Id_user"].ToString()) == id)
            {
                if (user == null)
                {
                    return HttpNotFound();
                }
                var numberComment = db.Comments.Where(comment => comment.Id_user == user.Id_user).Count();
                var numberHotel = db.Hotels.Where(hotel => hotel.Id_user == user.Id_user).Count();
                var numberReservetion = db.Reservations.Where(reservation => reservation.Id_user == user.Id_user).Count();
                ExpandoObject expandoObject = new ExpandoObject();
                dynamic model = expandoObject;
                model.User = user;
                model.numberComment = numberComment;
                model.numberHotel = numberHotel;
                model.numberReservetion = numberReservetion;

                return View(model);
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            Logout();
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id_user,First_Name,Last_Name,Email,Pwd")] User user)
        {

            if (ModelState.IsValid)
            {

                var OneUser = db.Users.Where(elt => elt.Email.Equals(user.Email) && elt.Pwd.Equals(user.Pwd)).FirstOrDefault();
                if (OneUser != null)
                {
                    ViewBag.MessageError = "This account already existe";

                    return View(user);
                }

                user.Roles = false;

                db.Users.Add(user);
                db.SaveChanges();
                
                return RedirectToAction("Login", user);
            }
            return View(user);
        }

        // GET: Users/Edit/5
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
            User user = db.Users.Find(id);
            if (user.Id_user == Int32.Parse(Session["Id_user"].ToString()) || Session["Roles"]!= null && Session["Roles"].ToString().ToLower() == "true")
            {
                if (user == null)
                {
                    return HttpNotFound();
                }
                return View(user);
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id_user,First_Name,Last_Name,Email,Pwd,Roles")] User user)
        {
            if (ModelState.IsValid)
            {
                if (user.Roles.ToString() == "True")
                {
                    user.Roles = true;
                }
                if (Session["Id_user"]!= null && Int32.Parse(Session["Id_user"].ToString()) == user.Id_user)
                {
                    Session["First_Name"] = user.First_Name;
                }
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Hotels");
            }
            return View(user);
        }

        // GET: Users/Delete/5
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
            User user = db.Users.Find(id);
            if (Session["Roles"].ToString().ToLower() == "true" && user.Roles == false)
            {
                if (user == null)
                {
                    return HttpNotFound();
                }
                return View(user);
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
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
