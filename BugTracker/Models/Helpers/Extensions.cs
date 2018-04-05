using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using static BugTracker.EmailService;

namespace BugTracker.Models.Helpers
{
    public static class Extensions
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static string GetFullName(this IIdentity user)
        {
            var ClaimsUser = (ClaimsIdentity)user;
            var claim = ClaimsUser.Claims.FirstOrDefault(c => c.Type == "Name");
            if (claim != null)
            {
                return claim.Value;
            }
            else
            {
                return null;
            }
        }

        
        public static async Task RefreshAuthentication(this HttpContextBase context, ApplicationUser user)
        {
            context.GetOwinContext().Authentication.SignOut();
            await context.GetOwinContext().Get<ApplicationSignInManager>()
                .SignInAsync(user, isPersistent: false, rememberBrowser: false);
        }

        public static bool In(this string source, params string[] list)
        {
            if (null == source) throw new ArgumentNullException("source");
            return list.Contains(source, StringComparer.OrdinalIgnoreCase);
        }


        //For Ticket Histories creating a method to reference in Ticket Details view for TicketHistories datatable
        //public static class TicketExtensions
        //{
            //private static ApplicationDbContext db = new ApplicationDbContext();

            public static void CreateHistories(this Ticket editedTicket)
            {
                List<TicketHistory> histories = new List<TicketHistory>();
                Ticket currentDbStateTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == editedTicket.Id);

                if (editedTicket.Title != currentDbStateTicket.Title)
                {
                    histories.Add(new TicketHistory()
                    {
                        OldValue = currentDbStateTicket.Title,
                        NewValue = editedTicket.Title,
                        Property = "Title"
                    });
                }

                if (editedTicket.Description != currentDbStateTicket.Description)
                {
                    histories.Add(new TicketHistory()
                    {
                        OldValue = currentDbStateTicket.Description,
                        NewValue = editedTicket.Description,
                        Property = "Description"
                    });
                }

                if (editedTicket.AssignedToUserId != currentDbStateTicket.AssignedToUserId)
                {
                    ApplicationUser previouslyAssignedUser = db.Users.Find(currentDbStateTicket.AssignedToUserId);
                    ApplicationUser newlyAssignedUser = db.Users.Find(editedTicket.AssignedToUserId);

                    histories.Add(new TicketHistory()
                    {
                        // You'll want to use user properties that are required, so that you will know that there will be a value to be displayed
                        // Here I am using interpolated strings, using the FirstName and LastName properties to build the users' full name
                        // This assumes that these two properties are required
                        OldValue = $"{previouslyAssignedUser.FirstName} {previouslyAssignedUser.LastName}",
                        NewValue = $"{newlyAssignedUser.FirstName} {newlyAssignedUser.LastName}",
                        Property = "Assigned User"
                    });
                }

                if (editedTicket.TicketStatusId != currentDbStateTicket.TicketStatusId)
                {
                    histories.Add(new TicketHistory()
                    {
                        OldValue = db.TicketStatuses.Find(currentDbStateTicket.TicketStatusId).Name,
                        NewValue = db.TicketStatuses.Find(editedTicket.TicketStatusId).Name,
                        Property = "Ticket Status"
                    });
                }

                if (editedTicket.TicketPriorityId != currentDbStateTicket.TicketPriorityId)
                {
                    histories.Add(new TicketHistory()
                    {
                        OldValue = db.TicketPriorities.Find(currentDbStateTicket.TicketPriorityId).Name,
                        NewValue = db.TicketPriorities.Find(editedTicket.TicketPriorityId).Name,
                        Property = "Ticket Priority"
                    });
                }

                if (editedTicket.TicketTypeId != currentDbStateTicket.TicketTypeId)
                {
                    histories.Add(new TicketHistory()
                    {
                        OldValue = db.TicketTypes.Find(currentDbStateTicket.TicketTypeId).Name,
                        NewValue = db.TicketTypes.Find(editedTicket.TicketTypeId).Name,
                        Property = "Ticket Type"
                    });
                }

                if (histories.Count > 0)
                {
                    string userId = HttpContext.Current.User.Identity.GetUserId();

                    for (int i = 0; i < histories.Count; i++)
                    {
                        histories[i].UserId = userId;
                        histories[i].Changed/*ChangeDate*/ = DateTimeOffset.Now;
                        histories[i].TicketId = editedTicket.Id;

                        db.TicketHistories.Add(histories[i]);
                    }

                    db.SaveChanges();
                }
            }
        //}

    }
}