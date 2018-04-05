using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var userid = User.Identity.GetUserId();
            //List<DashboardViewModel> dvm = new List<DashboardViewModel>();
            var projects = db.Projects.Where(p => p.Users.Any(u => u.Id == userid)).ToList();
            var tickets = db.Tickets.Where(u => u.OwnerUserId == userid || u.AssignedToUserId == userid || u.Project.ProjectManagerId == userid)
                .Include(t => t.AssignedToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType).ToList();
            var notifications = db.TicketNotifications.Where(u => u.UserId == userid).ToList();
            
            DashboardViewModel dvm = new DashboardViewModel()
            {
                Projects = projects,
                Tickets = tickets,
                Notifications = notifications
            };
            return View("IndexView", dvm);
        }

        public ActionResult IndexView()
        {
            return RedirectToAction("Index");
        }

        public ActionResult LandingPage()
        {
            ViewBag.Message = "Your application Landing page.";

            return View();
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}