using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class ProjectDetailsViewModel
    {
        public Project Project { get; set; }
        public ApplicationUser ProjectManager { get; set; }
    }
}