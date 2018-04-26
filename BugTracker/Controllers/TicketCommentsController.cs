using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using Microsoft.AspNet.Identity;

namespace BugTracker.Controllers
{
    public class TicketCommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: TicketComments
        public ActionResult Index()
        {
            var FullName = User.Identity.GetUserId();
            var ticketComments = db.TicketComments.Include(t => t.Ticket).Include(t => t.User);
            return View(ticketComments.ToList());
        }

        // GET: TicketComments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment ticketComment = db.TicketComments.Find(id);
            if (ticketComment == null)
            {
                return HttpNotFound();
            }
            return View(ticketComment);
        }

        // GET: TicketComments/Create
        [Authorize]
        public ActionResult Create(int ticketId)
        {
            TicketComment model = new TicketComment();
            model.TicketId = ticketId;
            model.Ticket = db.Tickets.Find(ticketId);
            return View(model);
        }

        // POST: TicketComments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize]
        public ActionResult Create([Bind(Include = "Comment,TicketId")] TicketComment ticketComment)
        {
            var FullName = User.Identity.GetUserId();
            Ticket ticket = db.Tickets.Find(ticketComment.TicketId);
            ApplicationUser user = db.Users.Find(ticket.AssignedToUserId);

            if (ModelState.IsValid)
            {
                ticketComment.UserId = FullName;
                ticketComment.Created = DateTimeOffset.Now;
                db.TicketComments.Add(ticketComment);
                db.SaveChanges();
                //return RedirectToAction("Index");

                var callbackUrl = Url.Action("Details", "Tickets", new { id = ticketComment.TicketId }, protocol: Request.Url.Scheme);
                try
                {
                    EmailService ems = new EmailService();
                    IdentityMessage msg = new IdentityMessage();
                    msg.Body = "A new comment has been added. " + Environment.NewLine + "Please click the following link to view the details" + "<a href=\"" + callbackUrl + "\"> NEW TICKET</a>";
                    msg.Destination = user.Email;
                    msg.Subject = "New Comment";
                    //await ems.SendMailAsync(msg);
                    ems.Send(msg);
                }
                catch (Exception ex)
                {
                    Task.FromResult(0);
                    //await Task.FromResult(0);
                }
                return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", ticketComment.FullName);
            return View(ticketComment);

        }
   

        //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComment.TicketId);
        //ViewBag.UserId = new SelectList(db.Users, "Id", "Email", ticketComment.FullName);
        //    return View(ticketComment);
    

        // GET: TicketComments/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment ticketComment = db.TicketComments.Find(id);
            if (ticketComment == null)
            {
                return HttpNotFound();
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", ticketComment.FullName);
            return View(ticketComment);
        }

        // POST: TicketComments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        {
            var FullName = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                ticketComment.UserId = FullName;
                ticketComment.Created = DateTimeOffset.Now;

                db.Entry(ticketComment).State = EntityState.Modified;
                db.SaveChanges();
                //return RedirectToAction("Index");
                return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", ticketComment.FullName);
            return View(ticketComment);
        }

        // GET: TicketComments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment ticketComment = db.TicketComments.Find(id);
            if (ticketComment == null)
            {
                return HttpNotFound();
            }
            return View(ticketComment);
        }

        // POST: TicketComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var userid = User.Identity.GetUserId();
            TicketComment ticketComment = db.TicketComments.Find(id);
            db.TicketComments.Remove(ticketComment);
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
