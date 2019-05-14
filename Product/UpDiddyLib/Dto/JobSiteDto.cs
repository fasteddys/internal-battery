using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobSiteDto
    {
        public Guid JobSiteGuid { get; set; }
    
        public string Name { get; set; }
 
        public Uri Uri { get; set; }
        // not including the joblisting, since I don't currently see a need 
        // public List<JobPage> JobListings { get; set; } = new List<JobPage>();
    }
}
