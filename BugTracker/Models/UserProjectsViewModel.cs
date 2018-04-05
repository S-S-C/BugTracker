using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class UserProjectsViewModel
    {
        public Project Project { get; set; }
        public MultiSelectList Users { get; set; }
        public List <string> SelectedUsers { get; set; }
    }
}