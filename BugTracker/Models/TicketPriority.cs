using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Ticket> Ticket { get; set; }
    }

    public enum Priority
    {
        High = 1,
        Medium = 2,
        Low = 3,
        Urgent = 4
    };
}