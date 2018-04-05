using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace BugTracker.Models
{
    //[SoftDelete("IsDelete")]
    public class TicketComment
    {
        
        public int Id { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public int TicketId { get; set; }
        public string FullName { get; set; }
        public string UserId { get; set; }
        //public bool IsDeleted { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}