using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<Project> Projects { get; set; }
        public IEnumerable<Ticket> Tickets { get; set; }
        public IEnumerable<TicketNotification> Notifications { get; set; }
       
    }
}