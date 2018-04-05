using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using Microsoft.AspNet.Identity;

namespace BugTracker.Controllers
{
    public class TicketAttachmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: TicketAttachments
        public ActionResult Index()
        {
            var ticketAttachments = db.TicketAttachments.Include(t => t.Ticket).Include(t => t.User);
            return View(ticketAttachments.ToList());
        }

        // GET: TicketAttachments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Create
        public ActionResult Create(int ticketId)
        {

            TicketAttachment model = new TicketAttachment();
            model.TicketId = ticketId;
            model.Ticket = db.Tickets.Find(ticketId);
            return View(model);
            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title");
            //ViewBag.UserId = new SelectList(db.Users, "Id", "Email");
            //return View();
        }

        // POST: TicketAttachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TicketId,Description,Created,UserId,FileUrl")] TicketAttachment ticketAttachment, HttpPostedFileBase image)
        {
            var FullName = User.Identity.GetUserId();
            Ticket ticket = db.Tickets.Find(ticketAttachment.TicketId);
            ApplicationUser user = db.Users.Find(ticket.AssignedToUserId);

            if (ModelState.IsValid)

            {
                //{
                //    db.TicketAttachments.Add(ticketAttachment);
                //    db.SaveChanges();
                //    return RedirectToAction("Index");
                //}

                if (ImageUploadViewModel.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    ticketAttachment.FileUrl = "/Uploads/" + fileName;

                    ticketAttachment.UserId = FullName;
                    ticketAttachment.Created = DateTimeOffset.Now;
                    db.TicketAttachments.Add(ticketAttachment);
                    db.SaveChanges();
                    return RedirectToAction("Index");

                }


                var callbackUrl = Url.Action("Details", "Tickets", new { id = ticketAttachment.TicketId }, protocol: Request.Url.Scheme);
                try
                {
                    EmailService ems = new EmailService();
                    IdentityMessage msg = new IdentityMessage();
                    msg.Body = "A new attachment has been added." + Environment.NewLine + "Please click the following link to view the details" + "<a href=\"" + callbackUrl + "\"> NEW TICKET</a>";
                    msg.Destination = user.Email;
                    msg.Subject = "Attachment Added";
                    //msg.Subject = "Assigned Ticket";

                    ems.Send(msg);
                    //await ems.SendMailAsync(msg);
                }
                catch (Exception ex)
                {
                    Task.FromResult(0);
                    //await Task.FromResult(0);
                }
                return RedirectToAction("Details", "Tickets", new { id = ticketAttachment.TicketId });
            }


            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", ticketAttachment.UserId);
            return View(ticketAttachment);

        }

        // GET: TicketAttachments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // POST: TicketAttachments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TicketId,Description,Created,UserId,FileUrl")] TicketAttachment ticketAttachment, HttpPostedFileBase image)
        {
            var FullName = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                //db.Entry(ticketAttachment).State = EntityState.Modified;
                //db.SaveChanges();
                //return RedirectToAction("Index");
            }
            if (ImageUploadViewModel.IsWebFriendlyImage(image))
            {
                var fileName = Path.GetFileName(image.FileName);
                image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                ticketAttachment.FileUrl = "/Uploads/" + fileName;

                //ticketAttachment.Updated = DateTime.Now;
                ticketAttachment.UserId = FullName;
                ticketAttachment.Created = DateTimeOffset.Now;
                db.Entry(ticketAttachment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Email", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }
            return View(ticketAttachment);
        }

        // POST: TicketAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            db.TicketAttachments.Remove(ticketAttachment);
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
