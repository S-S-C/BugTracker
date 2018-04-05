using BugTracker.Models;
using BugTracker.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class AdminController : Controller
    
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: AdminUserView
        //GET:Admin/Index
        [Authorize(Roles ="Admin")]
        public ActionResult Index()
        {
            List<AdminIndexViewModel> model = new List<AdminIndexViewModel>();
            UserRolesHelper helper = new UserRolesHelper();
            //Cycle through Users in DB
            foreach (var user in db.Users)
            {
                AdminIndexViewModel mod = new AdminIndexViewModel();
                mod.User = user;
                mod.Roles = helper.ListUserRoles(user.Id);
                model.Add(mod);
            }
            return View(model);
        }

        //GET:EditUser
        public ActionResult AdminUserView(string id)//EditUser(string id)
        {
        var user = db.Users.Find(id);
        AdminUserViewModel AdminModel = new AdminUserViewModel();
        UserRolesHelper helper = new UserRolesHelper();
        var selected = helper.ListUserRoles(id);
        List<string> rolesList = new List<string>();

        AdminModel.Roles = new MultiSelectList(db.Roles, "Name", "Name", selected);
            //AdminModel.User = new ApplicationUser();
            // AdminModel.User.Id = user.Id;
            //AdminModel.User.FullName = user.FullName;
            AdminModel.User = user;
        return View(AdminModel);
        }

        //POST:EditUser
        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize]
        public ActionResult AdminUserView(AdminUserViewModel model)
        {
            //var user = db.Users.Find(model.User.Id);
            UserRolesHelper helper = new UserRolesHelper();
            foreach (var role in db.Roles.Select(r => r.Name).ToList())
            {
                helper.RemoveUserFromRole(model.User.Id, role);
            }
            foreach (var roleadd in model.SelectedRoles)
            {
                helper.AddUserToRole(model.User.Id, roleadd);
            }
            return RedirectToAction("Index");
        }
        

    }
}