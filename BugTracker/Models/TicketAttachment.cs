﻿using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        //public DateTime Updated { get; set; }
        public string UserId { get; set; }
        public string FileUrl { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}