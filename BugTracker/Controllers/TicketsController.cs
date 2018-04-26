
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
using BugTracker.Models.Helpers;
using Microsoft.AspNet.Identity;

namespace BugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        [Authorize]
        public ActionResult Index()
        {
            var tickets = db.Tickets.Where(t => t.IsDeleted==false).Include(t => t.AssignedToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View(tickets.ToList());

        }


        //GET:My tickets
        //[Authorize(Roles = "Admin, Developer, ProjectManager")]
        public ActionResult MyTickets()
        {

            var userid = User.Identity.GetUserId();
            var user = db.Users.Find(userid);
            if (User.IsInRole("ProjectManager"))
            {
                var model = db.Tickets.Where(p => p.Project.ProjectManagerId == userid && p.IsDeleted == false).ToList();
                return View(model);
            }
            else
            {
                var model = db.Tickets.Where(u => u.AssignedToUserId == userid && u.IsDeleted == false).Include(t => t.AssignedToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
                //var model = db.Projects.Where(p => p.Users.Select(u => u.Id).Contains(userid)).Include("Users").ToList();
                return View(model);
                //    var userid = User.Identity.GetUserId();
                //var tickets = db.Tickets.Where(u=>u.AssignedToUserId  == userid  /*&& u.IsDeleted == false*/).Include(t => t.AssignedToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
                //return View(tickets.ToList());
            }
        }

        //GET:OwnedTickets
        public ActionResult OwnedTickets()
        {
            var userid = User.Identity.GetUserId();
            var tickets = db.Tickets.Where(u => u.OwnerUserId == userid).
                Include(t => t.AssignedToUser).Include(t => t.OwnerUser).Include(t => t.Project).
                Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View(tickets.ToList());
        }

        // GET: Tickets/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }


        //Assign Ticket
        public ActionResult AssignTicket(int? id)
        {
            UserRolesHelper helper = new UserRolesHelper();

            var ticket = db.Tickets.Find(id);

            var users = helper.UsersInRole("DEVELOPER").ToList();

            ViewBag.AssignedToUserId = new SelectList(users, "Id", "FullName", ticket.AssignedToUserId);

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignTicket(Ticket model)
        {
            var ticket = db.Tickets.Find(model.Id);
            ticket.AssignedToUserId = model.AssignedToUserId;

            db.SaveChanges();

            var callbackUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, protocol: Request.Url.Scheme);
            try
            {
                EmailService ems = new EmailService();
                IdentityMessage msg = new IdentityMessage();
                ApplicationUser user = db.Users.Find(model.AssignedToUserId);

                msg.Body = "You have been assigned a new Ticket." + Environment.NewLine + "Please click the following link to view the details" + "<a href=\"" + callbackUrl + "\">NEW TICKET</a>";

                msg.Destination = user.Email;
                msg.Subject = "Ticket Assigned";

                await ems.SendMailAsync(msg);
            }
            catch (Exception ex) { await Task.FromResult(0); }

            return RedirectToAction("Index");
        }



        // GET: Tickets/Create

        [Authorize(Roles ="Admin, Submitter")]
        public ActionResult Create(int id)
        {
            
            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FullName");
            //ViewBag.FullName = db.Users.Find(User.Identity.GetUserId()).FullName;
            //ViewBag.OwnerUserId = User.Identity.GetUserId();
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FullName");
            ViewBag.ProjectId = id;
            //ViewBag.Project = db.Projects.Find(id).Name;
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            Ticket model = new Ticket();
            model.ProjectId = id;
            return View(model);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Submitter")]
        public ActionResult Create([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignedToUserId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
               
                ticket.OwnerUserId = User.Identity.GetUserId();
                ticket.TicketStatusId = 1;
                ticket.Created = DateTimeOffset.Now;
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FullName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FullName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        //[Authorize(Roles = "Admin, ProjectManager, Developer, ")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FullName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FullName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId, AssignedToUserId")] Ticket ticket)
        {
            //if (ModelState.IsValid)
            //{
            //    ticket.Updated = DateTimeOffset.Now;
            //    db.Entry(ticket).State = EntityState.Modified;
            //    db.SaveChanges();
            //    return RedirectToAction("Index");
            //}
            //if (ModelState.IsValid)
            //{
            //    ticket.Updated = DateTimeOffset.Now;

            //    var oldTic = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);
            //    foreach (var prop in typeof(Ticket).GetProperties())
            //    {
            //        if(prop.Name != null)
            //        {
            //            var OldValue = oldTic..GetType().GetField(prop.Name).ToString();
            //            var NewValue = ticket.GetType().GetField(prop.Name).ToString();

            //            if (OldValue != NewValue)
            //            {
            //                TicketHistory ticketHistory = new TicketHistory()
            //                {
            //                    TicketId = ticket.Id,
            //                    UserId = User.Identity.GetUserId(),
            //                    Property = prop.Name,
            //                    OldValue = oldTic.GetType().GetField(prop.Name).ToString(),
            //                    NewValue = ticket.GetType().GetField(prop.Name).ToString(),
            //                    Changed = DateTime.Now
            //                };
            //                db.TicketHistories.Add(ticketHistory);
            //                db.SaveChanges();
            //            }
            //        }
            //    }
            //    return RedirectToAction("Details", new { id = ticket.Id });
            //}

            if (ModelState.IsValid)
            {
                ticket.Updated = DateTimeOffset.Now;
                var oldTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);

                foreach (var prop in typeof(Ticket).GetProperties())
                {
                    if (prop.Name != null && prop.Name.In("Title", "Description", "TicketTypeId", "TicketPriorityId", "TicketStatusId"))
                    {
                        var oldVal = oldTicket.GetType().GetProperty(prop.Name).GetValue(oldTicket).ToString();
                        var newVal = ticket.GetType().GetProperty(prop.Name).GetValue(ticket).ToString();

                        if (oldVal != newVal)
                        {
                            TicketHistory ticketHistory = new TicketHistory()
                            {
                                TicketId = oldTicket.Id,
                                UserId = User.Identity.GetUserId(),
                                Property = prop.Name,
                                OldValue = oldVal,
                                NewValue = newVal,
                                Changed = DateTime.Now
                            };
                            db.TicketHistories.Add(ticketHistory);
                        }
                    }
                }

                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();

                var callbackUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, protocol: Request.Url.Scheme);
                try
                {
                    EmailService ems = new EmailService(); IdentityMessage msg = new IdentityMessage(); ApplicationUser user = new ApplicationUser();

                    msg.Body = "You have been assigned a new Ticket." + Environment.NewLine + "Please click the following link to view the details" + "<a href=\"" + callbackUrl + "\">NEW TICKET</a>";

                    msg.Destination = user.Email;
                    //msg.Subject = "Invite to Household";
                    msg.Subject = "Tickets Assigned";
                    ems.Send(msg);
                }
                catch (Exception ex)

                {
                    Task.FromResult(0);
                }

                return RedirectToAction("Index");
            }

            
            //var tickets = db.Tickets.Find(ticket.Id);
            //ticket.AssignedToUserId = ticket.AssignedToUserId;

            //db.SaveChanges();

            //var callbackUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, protocol: Request.Url.Scheme);
            //try
            //{
            //    EmailService ems = new EmailService(); IdentityMessage msg = new IdentityMessage(); ApplicationUser user = new ApplicationUser();

            //    msg.Body = "You have been assigned a new Ticket." + Environment.NewLine + "Please click the following link to view the details" + "<a href=\"" + callbackUrl + "\">NEW TICKET</a>";

            //    msg.Destination = user.Email;
            //    msg.Subject = "Invite to Household";

            //    ems.Send(msg);
            //}
            //catch (Exception ex)

            //{
            //    Task.FromResult(0);
            //}

            //return RedirectToAction("Index");
        

            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FullName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FullName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            //db.Tickets.Remove(ticket);
            ticket.IsDeleted = true;
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
