using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class CourseDetailDto  
    {

        public string VendorLogoUrl { get; set; }
        public Guid CourseGuid {get;set;}
        public string Title { get; set; }
        public string Duration { get; set; }
        public string Description { get; set; }
        public Guid CourseLevelGuid { get; set; }
        public string VendorName { get; set; }
        public int NumEnrollments { get; set; }
        public Guid VendorGuid { get; set; }

        public string Code { get; set; }

        public string Level { get; set; }

        public string Topic { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ModifyDate { get; set; }

        public int IsDeleted { get; set; }

        public string CourseSkills { get; set; }

        public string TabletImage { get; set; }

        public string DesktopImage { get; set; }

        public string MobileImage { get; set; }
    

    }
    
}
