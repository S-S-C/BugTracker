using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Helpers
{
    

    //We want to change information from displaying primary kets to more readable data
    public class Utilities
    {
        public static Dictionary<string, string> PropDisplayValues = new Dictionary<string, string>()
        {
            { "TicketStatusId", "Ticket Status" },
            { "TicketPriorityId", "Ticket Priority" },
            { "TicketTypeId", "Ticket Type" },
            { "AssignedToUserId", "Assigned User" }
        };


        private static ApplicationDbContext db = new ApplicationDbContext();
        
        //Method that takes in a user by Id and returns the full name
        public static string GetUserNameById (string userId)
        {
            return db.Users.FirstOrDefault(u => u.Id == userId).FullName;
        }

        public static string GetPriorityById(int Id)
        {
            return db.TicketPriorities.FirstOrDefault(t => t.Id == Id).Name;
        }

        public static string GetStatusById(int Id)
        {
            return db.TicketStatuses.FirstOrDefault(t => t.Id == Id).Name;
        }

        public static string GetTypeById(int Id)
        {
            return db.TicketTypes.FirstOrDefault(t => t.Id == Id).Name;
        }

        public static string GetPropertyDisplayValue(string prop) =>
            PropDisplayValues[prop];
    }
}