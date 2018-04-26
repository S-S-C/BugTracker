using BugTracker.Models;
using BugTracker.Models.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Projects
       [Authorize(Roles ="Admin, ProjectManager")]
        public ActionResult Index()
        {
            //IEnumerable<Project> projects = db.Projects.Where(p => p.IsDeleted == false).Include("Tickets").ToList();
            ProjectHelper helper = new ProjectHelper();
            //List<ProjectDetailsViewModel> pList = new List<ProjectDetailsViewModel>();

            List<ProjectDetailsViewModel> model = new List<ProjectDetailsViewModel>();
                foreach(var proj in db.Projects.Where(p => p.IsDeleted == false).Include("Users").ToList())
            {
                ProjectDetailsViewModel vm = new ProjectDetailsViewModel();
                vm.Project = proj;
                vm.ProjectManager = db.Users.Find(proj.ProjectManagerId);
                model.Add(vm);
            }

            return View(model);
        }
        // GET: Projects
        //[Authorize]
        public ActionResult MyProjects()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            if (User.IsInRole("ProjectManager"))
            {
                var model = db.Projects.Where(p => p.ProjectManagerId == userId && p.IsDeleted == false).ToList();
                return View(model);
            }
            else
            {
                var model = db.Projects.Where(p => p.Users.Select(u => u.Id) .Contains(userId) /*&& p.IsDeleted == false*/).Include("Users").ToList();
                return View(model);
                //var allProjects = db.Projects.Where(p=>p.Tickets.Select(t => t.AssignedToUserId).Contains(userId) || p.Tickets.Select(t => t.OwnerUserId).Contains(userId)).Include("Tickets").ToList();

            }
        }

        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            ProjectDetailsViewModel vm = new ProjectDetailsViewModel();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vm.Project = db.Projects.Find(id);
            if (vm.Project == null)
            {
                return HttpNotFound();
            }

            vm.ProjectManager = db.Users.Find(vm.Project.ProjectManagerId);
            return View(vm);
        }

        // GET: Projects/Create
        [Authorize(Roles ="Admin, ProjectManager")]
        public ActionResult Create()
        {
            UserRolesHelper helper = new UserRolesHelper();
            var pms = helper.UsersInRole("ProjectManager");
            ViewBag.ProjectManagerId = new SelectList(pms, "Id", "FullName");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="Admin,ProjectManager")]
        public ActionResult Create([Bind(Include = "Id,Name")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles ="Admin, ProjectManager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            UserRolesHelper helper = new UserRolesHelper();
            var pms = helper.UsersInRole("ProjectManager");

            ViewBag.ProjectManagerId = new SelectList(pms, "Id", "FullName");
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,ProjectManagerId")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }
        [Authorize(Roles ="Admin, ProjectManager")]
        public ActionResult EditUsers(int id)
        {
            var project = db.Projects.Find(id);
            UserProjectsViewModel pvm = new UserProjectsViewModel();
            ProjectHelper helper = new ProjectHelper();
            var selected = helper.ListUsersOnProject(id);
            pvm.Users = new MultiSelectList(db.Users, "Id", "FirstName", selected);
            pvm.Project = project;
            return View(pvm);
        }

        //POST:Admin/EditUser
        [HttpPost]
        [Authorize(Roles ="Admin, ProjectManager")]
        public ActionResult EditUsers(UserProjectsViewModel model)
        {
            ProjectHelper helper = new ProjectHelper();
            
            foreach (var user in db.Users)
            {
                helper.RemoveUserFromProject(user.Id, model.Project.Id);
            }
            foreach (var user in model.SelectedUsers)
            {
                helper.AddUserToProject(user, model.Project.Id);
            }
            return RedirectToAction("Index");
        }

        // GET: Projects/Delete/5

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            //db.Projects.Remove(project);
            project.IsDeleted = true;
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
