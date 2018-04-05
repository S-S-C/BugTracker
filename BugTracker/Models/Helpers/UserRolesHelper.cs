using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Helpers
{
    public class UserRolesHelper
    {
        private UserManager<ApplicationUser> userManager =
           new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

        private ApplicationDbContext db = new ApplicationDbContext();

        public bool IsUserInRole(string UserId, string roleName)
        {
            try
            {
                var result = userManager.IsInRole(UserId, roleName);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            // finally { }
        }

        //List User Roles
        public ICollection<string> ListUserRoles(string UserId)
        {
            try
            {
                return userManager.GetRoles(UserId);
            }
            catch
            {
                return null;
            }
        }
           

        //Adding User to Role
        
        public bool AddUserToRole(string UserId, string roleName)
        {
            try
            {
                var result = userManager.AddToRole(UserId, roleName);
                return result.Succeeded;
            }
            catch
            {
                return false;
            }

        }

        //Remove User From Role
        public bool RemoveUserFromRole(string UserId, string roleName)
        {
            try
            {
                var result = userManager.RemoveFromRoles(UserId, roleName);
                return result.Succeeded;
            }
            catch
            {
                return false;
            }
        }

        //UsersInRole
        public ICollection<ApplicationUser> UsersInRole(string roleName)
        {
            //return userManager.Users.Select(u=> u.Roles==)

            var resultList = new List<ApplicationUser>();
            var List = userManager.Users.ToList();

            foreach (var user in List)
            {
                if (IsUserInRole(user.Id, roleName))
                {
                    resultList.Add(user);
                }

            }

            return resultList;
        }

        //Users Not In Role
        public ICollection<ApplicationUser> UsersNotInRole(string roleName)
        {
            //return userManager.Users.Select(u=> u.Roles==)

            var resultList = new List<ApplicationUser>();
            var List = userManager.Users.ToList();

            foreach (var user in List)
            {
                if (!IsUserInRole(user.Id, roleName))
                {
                    resultList.Add(user);
                }

            }

            return resultList;
        }


    }
}