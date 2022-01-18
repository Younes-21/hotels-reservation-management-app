using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HOTEL_MANAGEMENT.Models;
using HOTEL_MANAGEMENTML.Model;

namespace HOTEL_MANAGEMENT.Controllers
{
    public class CommentsController : Controller
    {
        private DB_HOTEL_MANAGEMENTEntities db = new DB_HOTEL_MANAGEMENTEntities();

        // GET: Comments
        public ActionResult Index()
        {
            //var comments = db.Comments.Include(c => c.Hotel).Include(c => c.User);
            //return View(comments.ToList());
            return RedirectToAction("Index", "Hotels");
        }

        // GET: Comments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // GET: Comments/Create
        public ActionResult Create()
        {
            if (Request.Form["comment"] != null)
            {
                if (Session["Id_user"] == null)
                {
                    return RedirectToAction("Login", "Users");
                }
                ModelInput data = new ModelInput()
                {
                    Col0 = HttpContext.Request.Form["comment"],
                };
                var result = ConsumeModel.Predict(data);
                bool sentiment = result.Prediction == "1" ? true : false;

                Comment cmnt = new Comment();
                cmnt.Opinion = sentiment;
                cmnt.Comment1 = HttpContext.Request.Form["comment"];
                cmnt.Date_Comment = DateTime.Now;
                cmnt.Id_user = (int)Session["Id_user"];
                cmnt.Id_Hotel = Int32.Parse(HttpContext.Request.Form["Id_Hotel"]);
                db.Comments.Add(cmnt);
                db.SaveChanges();
                return RedirectToAction("Details/" + Request.Form["Id_Hotel"], "Hotels");
            }
            return RedirectToAction("Details/" + Request.Form["Id_Hotel"], "Hotels");
        }

        // GET: Comments/Edit/5
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
            Comment comment = db.Comments.Find(id);
            if (comment.Id_user == Int32.Parse(Session["Id_user"].ToString()))
            {
                if (comment == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Id_Hotel = new SelectList(db.Hotels, "Id_Hotel", "Name_Hotel", comment.Id_Hotel);
                ViewBag.Id_user = new SelectList(db.Users, "Id_user", "First_Name", comment.Id_user);
                return View(comment);
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id_Comment,Opinion,Comment1,Date_Comment,Id_user,Id_Hotel")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                ModelInput data = new ModelInput()
                {
                    Col0 = comment.Comment1,
                };
                var result = ConsumeModel.Predict(data);
                bool sentiment = result.Prediction == "1" ? true : false;

                comment.Opinion = sentiment;

                //comment.Date_Comment = DateTime.Now;
                comment.Id_user = (int)Session["Id_user"];
                comment.Id_Hotel = (int)(Session["Id_Hotel"]);

                db.Entry(comment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details/"+comment.Id_Hotel, "Hotels");
            }
            ViewBag.Id_Hotel = new SelectList(db.Hotels, "Id_Hotel", "Name_Hotel", comment.Id_Hotel);
            ViewBag.Id_user = new SelectList(db.Users, "Id_user", "First_Name", comment.Id_user);
            return View(comment);
        }

        // GET: Comments/Delete/5
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
            Comment comment = db.Comments.Find(id);
            if (comment.Id_user == Int32.Parse(Session["Id_user"].ToString()) || Session["Roles"] != null && Session["Roles"].ToString().ToLower() == "true")
            {
                if (comment == null)
                {
                    return HttpNotFound();
                }
                return View(comment);
            }
            return RedirectToAction("ErrorAuthorisation", "Home");
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Comment comment = db.Comments.Find(id);
            db.Comments.Remove(comment);
            db.SaveChanges();
            return RedirectToAction("Details/" + Session["Id_Hotel"], "Hotels");
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
